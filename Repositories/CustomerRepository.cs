using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class CustomerRepository
{
    private readonly DbavalogContext db;
    private readonly IAuditLogService auditLogService;
    private readonly ILogger<CustomerRepository> logger;

    public CustomerRepository(
        DbavalogContext context,
        IAuditLogService _auditLogService,
        ILogger<CustomerRepository> _logger)
    {
        db = context;
        auditLogService = _auditLogService;
        logger = _logger;
    }

    public async Task<TblCustomer?> GetCustomerByCode(
        string customer_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customer_code))
        {
            return null;
        }

        return await db.TblCustomers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.CustomerCode == customer_code,
                cancellationToken);
    }

    public async Task<ResponseViewModel> EditCustomer(
        TblCustomer customerToEdit,
        TblCustomerRule ruleToEdit,
        CancellationToken cancellationToken = default)
    {
        if (customerToEdit == null ||
            string.IsNullOrWhiteSpace(customerToEdit.CustomerCode))
        {
            return Failure("ไม่พบข้อมูล Customer");
        }

        if (ruleToEdit == null)
        {
            return Failure("ไม่พบข้อมูลกติกา Customer");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var customer = await db.TblCustomers
                .FirstOrDefaultAsync(
                    w => w.CustomerCode ==
                         customerToEdit.CustomerCode,
                    cancellationToken);

            if (customer == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Failure(
                    $"ไม่พบ Customer {customerToEdit.CustomerCode}");
            }

            if (customer.IsEnabled != true)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Failure(
                    "Customer รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var duplicateMessage =
                await ValidateUniqueCustomerAsync(
                    customerToEdit,
                    customer.CustomerCode,
                    cancellationToken);

            if (duplicateMessage != null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure(duplicateMessage);
            }

            var address = await db.TblCustomerAddresses
                .FirstOrDefaultAsync(
                    w => w.CustomerCode ==
                         customer.CustomerCode,
                    cancellationToken);

            var customerRule = await db.TblCustomerRules
                .FirstOrDefaultAsync(
                    w => w.CustomerCode ==
                         customer.CustomerCode,
                    cancellationToken);

            var oldValues = new
            {
                Customer = GetScalarSnapshot(customer),

                Address =
                    address == null
                        ? null
                        : GetScalarSnapshot(address),

                Rule =
                    customerRule == null
                        ? null
                        : GetScalarSnapshot(customerRule)
            };

            CopyEditableScalarValues(
                customer,
                customerToEdit,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    nameof(TblCustomer.CustomerCode),
                    nameof(TblCustomer.IsEnabled),
                    nameof(TblCustomer.IsActived),
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            customer.UpdateDate = DateTime.Now;
            customer.UpdateBy = customerToEdit.UpdateBy;

            if (address != null)
            {
                address.TransportName =
                    customer.CustomerNameTh;

                address.TransportAddress =
                    customer.CustomerAddress;

                address.AdvanceName =
                    customer.CustomerNameTh;

                address.AdvanceAddress =
                    customer.CustomerAddress;
            }
            else
            {
                logger.LogWarning(
                    "Customer {CustomerCode} has no customer address record while editing.",
                    customer.CustomerCode);
            }

            if (customerRule == null)
            {
                ruleToEdit.CustomerCode =
                    customer.CustomerCode;

                ruleToEdit.IsTransportOther = true;

                db.TblCustomerRules.Add(
                    ruleToEdit);

                customerRule = ruleToEdit;
            }
            else
            {
                CopyEditableScalarValues(
                    customerRule,
                    ruleToEdit,
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase)
                    {
                        nameof(TblCustomerRule.CustomerCode),
                        "CreateDate",
                        "CreateBy",
                        "RowVersion",
                        "Timestamp"
                    });

                customerRule.IsTransportOther = true;

                SetUpdateAuditFieldsIfPresent(
                    customerRule,
                    customerToEdit.UpdateBy);
            }

            var newValues = new
            {
                Customer = GetScalarSnapshot(customer),

                Address =
                    address == null
                        ? null
                        : GetScalarSnapshot(address),

                Rule =
                    GetScalarSnapshot(customerRule)
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Customer",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblCustomer),
                    EntityId = customer.CustomerCode,
                    Description =
                        $"แก้ไข Customer {customer.CustomerNameTh} โดย {customer.UpdateBy}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "EditCustomer",
                customerToEdit.CustomerCode);
        }
    }

    public async Task<List<CustomerViewModel>> GetCustomerDataList(
        CancellationToken cancellationToken = default)
    {
        return await db.TblCustomers
            .AsNoTracking()
            .Where(a => a.IsEnabled == true)
            .OrderBy(a => a.CustomerAcronym)
            .ThenBy(a => a.CustomerNameEn)
            .Select(a => new CustomerViewModel
            {
                customer_code = a.CustomerCode,
                customer_name_en = a.CustomerNameEn,
                customer_acronym = a.CustomerAcronym,
                is_actived = a.IsActived
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ResponseViewModel> AddCustomer(
        TblCustomer customerToAdd,
        TblCustomerRule paymentToAdd,
        CancellationToken cancellationToken = default)
    {
        if (customerToAdd == null ||
            string.IsNullOrWhiteSpace(customerToAdd.CustomerCode))
        {
            return Failure("ไม่พบข้อมูล Customer");
        }

        if (paymentToAdd == null)
        {
            return Failure("ไม่พบข้อมูลกติกา Customer");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var codeExists = await db.TblCustomers
                .AsNoTracking()
                .AnyAsync(
                    w => w.CustomerCode ==
                         customerToAdd.CustomerCode,
                    cancellationToken);

            if (codeExists)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Failure(
                    $"Customer Code {customerToAdd.CustomerCode} มีอยู่ในระบบแล้ว");
            }

            var duplicateMessage =
                await ValidateUniqueCustomerAsync(
                    customerToAdd,
                    excludeCustomerCode: null,
                    cancellationToken);

            if (duplicateMessage != null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure(duplicateMessage);
            }

            paymentToAdd.CustomerCode =
                customerToAdd.CustomerCode;

            customerToAdd.TblCustomerRule =
                paymentToAdd;

            db.TblCustomers.Add(customerToAdd);

            var newValues = new
            {
                Customer =
                    GetScalarSnapshot(customerToAdd),

                Rule =
                    GetScalarSnapshot(paymentToAdd)
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Customer",
                    ActionName = AuditActions.Create,
                    EntityName = nameof(TblCustomer),
                    EntityId = customerToAdd.CustomerCode,
                    Description =
                        $"เพิ่มข้อมูล Customer {customerToAdd.CustomerNameTh}",
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "AddCustomer",
                customerToAdd.CustomerCode);
        }
    }

    public async Task<TblCustomerRule?> GetRuleByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblCustomerRules
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.CustomerCode == code,
                cancellationToken);
    }

    public async Task<ResponseViewModel> Remove(
        string customer_code,
        string user,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customer_code))
        {
            return Failure("ไม่พบรหัส Customer");
        }

        try
        {
            var item = await db.TblCustomers
                .FirstOrDefaultAsync(
                    w => w.CustomerCode == customer_code,
                    cancellationToken);

            if (item == null)
            {
                return Failure(
                    $"ไม่พบ Customer {customer_code}");
            }

            if (item.IsEnabled != true)
            {
                return Failure(
                    "Customer รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues =
                GetScalarSnapshot(item);

            item.IsActived = false;
            item.IsEnabled = false;
            item.UpdateDate = DateTime.Now;
            item.UpdateBy = user;

            var newValues =
                GetScalarSnapshot(item);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Customer",
                    ActionName = AuditActions.Delete,
                    EntityName = nameof(TblCustomer),
                    EntityId = item.CustomerCode,
                    Description =
                        $"ลบ Customer {item.CustomerNameTh} โดย {user}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "Remove",
                customer_code);
        }
    }

    public async Task<ResponseViewModel> Active(
        string customer_code,
        string user,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customer_code))
        {
            return Failure("ไม่พบรหัส Customer");
        }

        try
        {
            var item = await db.TblCustomers
                .FirstOrDefaultAsync(
                    w => w.CustomerCode == customer_code,
                    cancellationToken);

            if (item == null)
            {
                return Failure(
                    $"ไม่พบ Customer {customer_code}");
            }

            if (item.IsEnabled != true)
            {
                return Failure(
                    "ไม่สามารถเปิดใช้งาน Customer ที่ถูกยกเลิกแล้ว");
            }

            if (item.IsActived == true)
            {
                return Success();
            }

            var oldValues =
                GetScalarSnapshot(item);

            item.IsActived = true;
            item.UpdateDate = DateTime.Now;
            item.UpdateBy = user;

            var newValues =
                GetScalarSnapshot(item);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Customer",
                    ActionName = AuditActions.Active,
                    EntityName = nameof(TblCustomer),
                    EntityId = item.CustomerCode,
                    Description =
                        $"เปิดใช้งาน Customer {item.CustomerNameTh} โดย {user}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "Active",
                customer_code);
        }
    }

    public async Task<ResponseViewModel> Inactive(
        string customer_code,
        string user,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customer_code))
        {
            return Failure("ไม่พบรหัส Customer");
        }

        try
        {
            var item = await db.TblCustomers
                .FirstOrDefaultAsync(
                    w => w.CustomerCode == customer_code,
                    cancellationToken);

            if (item == null)
            {
                return Failure(
                    $"ไม่พบ Customer {customer_code}");
            }

            if (item.IsEnabled != true)
            {
                return Failure(
                    "Customer รายการนี้ถูกยกเลิกไปแล้ว");
            }

            if (item.IsActived == false)
            {
                return Success();
            }

            var oldValues =
                GetScalarSnapshot(item);

            item.IsActived = false;
            item.UpdateDate = DateTime.Now;
            item.UpdateBy = user;

            var newValues =
                GetScalarSnapshot(item);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Customer",
                    ActionName = AuditActions.Inactive,
                    EntityName = nameof(TblCustomer),
                    EntityId = item.CustomerCode,
                    Description =
                        $"ปิดใช้งาน Customer {item.CustomerNameTh} โดย {user}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "Inactive",
                customer_code);
        }
    }

    public async Task<CustomerViewModel?> GetCustomerDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblCustomers
            .AsNoTracking()
            .Where(a => a.CustomerCode == code)
            .Select(a => new CustomerViewModel
            {
                customer_code = a.CustomerCode,
                customer_name_th = a.CustomerNameTh,
                customer_address = a.CustomerAddress,
                customer_tax = a.CustomerVatNo
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsVatNoDuplicate(
        string vat_no,
        CancellationToken cancellationToken = default)
    {
        vat_no = vat_no?.Trim() ?? string.Empty;

        if (vat_no.Length == 0)
        {
            return false;
        }

        return await db.TblCustomers
            .AsNoTracking()
            .AnyAsync(
                w => w.CustomerVatNo == vat_no &&
                     w.IsEnabled == true,
                cancellationToken);
    }

    public async Task<bool> IsAcronymDuplicate(
        string name,
        CancellationToken cancellationToken = default)
    {
        name = name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            return false;
        }

        return await db.TblCustomers
            .AsNoTracking()
            .AnyAsync(
                w => w.CustomerAcronym == name &&
                     w.IsEnabled == true,
                cancellationToken);
    }

    private async Task<string?> ValidateUniqueCustomerAsync(
        TblCustomer customer,
        string? excludeCustomerCode,
        CancellationToken cancellationToken)
    {
        var vatNo =
            customer.CustomerVatNo?.Trim();

        if (!string.IsNullOrWhiteSpace(vatNo))
        {
            var vatDuplicate = await db.TblCustomers
                .AsNoTracking()
                .AnyAsync(
                    w =>
                        w.IsEnabled == true &&
                        w.CustomerVatNo == vatNo &&
                        (excludeCustomerCode == null ||
                         w.CustomerCode != excludeCustomerCode),
                    cancellationToken);

            if (vatDuplicate)
            {
                return
                    $"เลขประจำตัวผู้เสียภาษี {vatNo} มีอยู่ในระบบแล้ว";
            }
        }

        var acronym =
            customer.CustomerAcronym?.Trim();

        if (!string.IsNullOrWhiteSpace(acronym))
        {
            var acronymDuplicate = await db.TblCustomers
                .AsNoTracking()
                .AnyAsync(
                    w =>
                        w.IsEnabled == true &&
                        w.CustomerAcronym == acronym &&
                        (excludeCustomerCode == null ||
                         w.CustomerCode != excludeCustomerCode),
                    cancellationToken);

            if (acronymDuplicate)
            {
                return
                    $"ชื่อย่อ Customer {acronym} มีอยู่ในระบบแล้ว";
            }
        }

        return null;
    }

    private Dictionary<string, object?> GetScalarSnapshot(
        object entity)
    {
        return db.Entry(entity)
            .Properties
            .ToDictionary(
                p => p.Metadata.Name,
                p => p.CurrentValue);
    }

    private void CopyEditableScalarValues<TEntity>(
        TEntity target,
        TEntity source,
        ISet<string> protectedProperties)
        where TEntity : class
    {
        var targetEntry = db.Entry(target);
        var sourceEntry = db.Entry(source);

        var sourceProperties = sourceEntry
            .Properties
            .ToDictionary(
                p => p.Metadata.Name,
                p => p,
                StringComparer.OrdinalIgnoreCase);

        foreach (var property in targetEntry.Properties)
        {
            var propertyName =
                property.Metadata.Name;

            if (protectedProperties.Contains(propertyName))
            {
                continue;
            }

            if (sourceProperties.TryGetValue(
                propertyName,
                out var sourceProperty))
            {
                property.CurrentValue =
                    sourceProperty.CurrentValue;
            }
        }
    }

    private void SetUpdateAuditFieldsIfPresent(
        object entity,
        string? username)
    {
        var entry = db.Entry(entity);

        if (entry.Metadata.FindProperty("UpdateDate") != null)
        {
            entry.Property("UpdateDate").CurrentValue =
                DateTime.Now;
        }

        if (entry.Metadata.FindProperty("UpdateBy") != null)
        {
            entry.Property("UpdateBy").CurrentValue =
                username;
        }
    }

    private static ResponseViewModel Success()
    {
        return new ResponseViewModel
        {
            result = true,
            resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว"
        };
    }

    private static ResponseViewModel Failure(
        string message)
    {
        return new ResponseViewModel
        {
            result = false,
            resultMessage = message
        };
    }

    private ResponseViewModel Error(
        Exception ex,
        string operation,
        string? entityId = null)
    {
        logger.LogError(
            ex,
            "Error in {Operation}. EntityId: {EntityId}",
            operation,
            entityId);

        return new ResponseViewModel
        {
            result = false,
            resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>"
        };
    }

    private async Task RollbackSafelyAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        try
        {
            await transaction.RollbackAsync(
                cancellationToken);
        }
        catch (Exception rollbackEx)
        {
            logger.LogError(
                rollbackEx,
                "Error while rolling back Customer transaction.");
        }
    }
}
