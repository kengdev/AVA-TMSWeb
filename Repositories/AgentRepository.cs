using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class AgentRepository
{
    private readonly DbavalogContext db;
    private readonly CodeRepository codeRepo;
    private readonly IAuditLogService auditLogService;
    private readonly ILogger<AgentRepository> logger;

    public AgentRepository(
        DbavalogContext context,
        CodeRepository _codeRepo,
        IAuditLogService _auditLogService,
        ILogger<AgentRepository> _logger)
    {
        db = context;
        codeRepo = _codeRepo;
        auditLogService = _auditLogService;
        logger = _logger;
    }

    public async Task<List<AgentViewModel>> GetAgentDataList(
        CancellationToken cancellationToken = default)
    {
        return await db.TblAgents
            .AsNoTracking()
            .Where(a =>
                a.AgentCode != "1900" &&
                a.IsEnabled == true)
            .OrderBy(a => a.AgentName)
            .Select(a => new AgentViewModel
            {
                code = a.AgentCode,
                name = a.AgentName,
                is_actived = a.IsActived
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TblAgent?> GetAgentDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.AgentCode == code,
                cancellationToken);
    }

    public async Task<ResponseViewModel> AddData(
        TblAgent dataToAdd,
        CancellationToken cancellationToken = default)
    {
        if (dataToAdd == null)
        {
            return Failure("ไม่พบข้อมูล Agent");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            dataToAdd.AgentCode =
                await codeRepo.GetAgentCode();

            await db.TblAgents.AddAsync(
                dataToAdd,
                cancellationToken);

            var newValues =
                GetScalarSnapshot(dataToAdd);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Agent",
                    ActionName = AuditActions.Create,
                    EntityName = nameof(TblAgent),
                    EntityId = dataToAdd.AgentCode,
                    Description =
                        $"เพิ่มข้อมูล Agent {dataToAdd.AgentName}",
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "AddData",
                dataToAdd.AgentCode);
        }
    }

    public async Task<ResponseViewModel> EditAgentData(
        TblAgent dataToEdit,
        CancellationToken cancellationToken = default)
    {
        if (dataToEdit == null ||
            string.IsNullOrWhiteSpace(dataToEdit.AgentCode))
        {
            return Failure("ไม่พบข้อมูล Agent");
        }

        try
        {
            var agent = await db.TblAgents
                .FirstOrDefaultAsync(
                    w => w.AgentCode ==
                         dataToEdit.AgentCode,
                    cancellationToken);

            if (agent == null)
            {
                return Failure(
                    $"ไม่พบ Agent {dataToEdit.AgentCode}");
            }

            if (agent.IsEnabled != true)
            {
                return Failure(
                    "Agent รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues =
                GetScalarSnapshot(agent);

            /*
             * ใช้ tracked entity แทน Update(detached entity)
             * และกัน field ที่ไม่ควรถูก form Edit ทับ
             */
            CopyEditableScalarValues(
                agent,
                dataToEdit,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    "AgentCode",
                    "IsEnabled",
                    "IsActived",
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            SetAuditFieldsIfPresent(
                agent,
                dataToEdit);

            var newValues =
                GetScalarSnapshot(agent);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Agent",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblAgent),
                    EntityId = agent.AgentCode,
                    Description =
                        $"แก้ไขข้อมูล Agent {agent.AgentName}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "EditAgentData",
                dataToEdit.AgentCode);
        }
    }

    public async Task<ResponseViewModel> Remove(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัส Agent");
        }

        try
        {
            var agent = await db.TblAgents
                .FirstOrDefaultAsync(
                    w => w.AgentCode == code,
                    cancellationToken);

            if (agent == null)
            {
                return Failure(
                    $"ไม่พบ Agent {code}");
            }

            if (agent.IsEnabled != true)
            {
                return Failure(
                    "Agent รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues =
                GetScalarSnapshot(agent);

            agent.IsActived = false;
            agent.IsEnabled = false;

            SetAuditFieldsIfPresent(
                agent,
                username);

            var newValues =
                GetScalarSnapshot(agent);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Agent",
                    ActionName = AuditActions.Delete,
                    EntityName = nameof(TblAgent),
                    EntityId = agent.AgentCode,
                    Description =
                        $"ลบข้อมูล Agent {agent.AgentName} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "Remove",
                code);
        }
    }

    public async Task<ResponseViewModel> Active(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัส Agent");
        }

        try
        {
            var agent = await db.TblAgents
                .FirstOrDefaultAsync(
                    w => w.AgentCode == code,
                    cancellationToken);

            if (agent == null)
            {
                return Failure(
                    $"ไม่พบ Agent {code}");
            }

            if (agent.IsEnabled != true)
            {
                return Failure(
                    "ไม่สามารถเปิดใช้งาน Agent ที่ถูกยกเลิกแล้ว");
            }

            if (agent.IsActived == true)
            {
                return Success();
            }

            var oldValues =
                GetScalarSnapshot(agent);

            agent.IsActived = true;

            SetAuditFieldsIfPresent(
                agent,
                username);

            var newValues =
                GetScalarSnapshot(agent);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Agent",
                    ActionName = AuditActions.Active,
                    EntityName = nameof(TblAgent),
                    EntityId = agent.AgentCode,
                    Description =
                        $"เปิดใช้งาน Agent {agent.AgentName} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "Active",
                code);
        }
    }

    public async Task<ResponseViewModel> Inactive(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัส Agent");
        }

        try
        {
            var agent = await db.TblAgents
                .FirstOrDefaultAsync(
                    w => w.AgentCode == code,
                    cancellationToken);

            if (agent == null)
            {
                return Failure(
                    $"ไม่พบ Agent {code}");
            }

            if (agent.IsEnabled != true)
            {
                return Failure(
                    "Agent รายการนี้ถูกยกเลิกไปแล้ว");
            }

            if (agent.IsActived == false)
            {
                return Success();
            }

            var oldValues =
                GetScalarSnapshot(agent);

            agent.IsActived = false;

            SetAuditFieldsIfPresent(
                agent,
                username);

            var newValues =
                GetScalarSnapshot(agent);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Agent",
                    ActionName = AuditActions.Inactive,
                    EntityName = nameof(TblAgent),
                    EntityId = agent.AgentCode,
                    Description =
                        $"ปิดใช้งาน Agent {agent.AgentName} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "Inactive",
                code);
        }
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

            if (protectedProperties.Contains(
                propertyName))
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

    /*
     * ใช้ metadata เพื่อไม่ผูก compile-time กับ base entity
     * หาก TblAgent มี UpdateDate / UpdateBy จะอัปเดตให้
     */
    private void SetAuditFieldsIfPresent(
        TblAgent agent,
        TblAgent source)
    {
        var entry = db.Entry(agent);
        var sourceEntry = db.Entry(source);

        var updateByProperty =
            entry.Metadata.FindProperty("UpdateBy");

        if (updateByProperty != null)
        {
            var sourceUpdateBy =
                sourceEntry.Metadata.FindProperty("UpdateBy") != null
                    ? sourceEntry.Property("UpdateBy").CurrentValue
                    : null;

            if (sourceUpdateBy != null)
            {
                entry.Property("UpdateBy").CurrentValue =
                    sourceUpdateBy;
            }
        }

        SetUpdateDateIfPresent(entry);
    }

    private void SetAuditFieldsIfPresent(
        TblAgent agent,
        string username)
    {
        var entry = db.Entry(agent);

        if (entry.Metadata.FindProperty("UpdateBy") != null)
        {
            entry.Property("UpdateBy").CurrentValue =
                username;
        }

        SetUpdateDateIfPresent(entry);
    }

    private static void SetUpdateDateIfPresent(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        if (entry.Metadata.FindProperty("UpdateDate") != null)
        {
            entry.Property("UpdateDate").CurrentValue =
                DateTime.Now;
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
                "Error while rolling back Agent transaction.");
        }
    }
}
