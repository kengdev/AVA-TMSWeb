using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;

namespace TMSWeb.Repositories;

public class BudgetRepository
{
    private const string TransportBudgetCode = "01";
    private const string GeneralBudgetCode = "02";

    private readonly DbavalogContext db;
    private readonly CodeRepository codeRepo;
    private readonly ILogger<BudgetRepository> logger;

    public BudgetRepository(
        DbavalogContext context,
        CodeRepository _codeRepo,
        ILogger<BudgetRepository> _logger)
    {
        db = context;
        codeRepo = _codeRepo;
        logger = _logger;
    }

    #region TransportBudget

    public async Task<BudgetViewModel> GetTrBudget(
        CancellationToken cancellationToken = default)
    {
        var budgetData = await db.TblBudgets
            .AsNoTracking()
            .Where(w => w.BudgetCode == TransportBudgetCode)
            .Select(w => new
            {
                w.Balance,
                w.UpdateDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (budgetData == null)
        {
            throw new InvalidOperationException(
                $"ไม่พบ Transport Budget ({TransportBudgetCode})");
        }

        var transactions = await db.ViewTransportBudgetTransactions
            .AsNoTracking()
            .Where(w => w.BudgetType == "TR")
            .OrderByDescending(o => o.CreateDate)
            .Take(5)
            .Select(item => new
            {
                item.Amount,
                item.AmountDiff,
                item.TransactionDate,
                item.CreateDate,
                item.JobDetailCode,
                item.CustomerAcronym,
                item.Start,
                item.Mission,
                item.TransactionType,
                item.PaymentType,
                item.Remark
            })
            .ToListAsync(cancellationToken);

        return new BudgetViewModel
        {
            balance = budgetData.Balance,
            update_date = budgetData.UpdateDate,

            transport_transactions = transactions
                .Select(item =>
                    new TransportBudgetTransactionViewModel
                    {
                        amount = item.Amount,
                        amount_diff = item.AmountDiff,

                        transaction_date =
                            item.TransactionDate
                                .ToString("dd-MM-yyyy"),

                        create_date = item.CreateDate,
                        job_detail_code = item.JobDetailCode,
                        customer_name = item.CustomerAcronym,
                        start = item.Start,
                        mission = item.Mission,
                        transaction_type = item.TransactionType,
                        payment_type = item.PaymentType,
                        remark = item.Remark
                    })
                .ToList()
        };
    }

    public async Task<ViewTransportBudgetTransaction?>
        GetBudgetTransactionDataByCode(
            string code,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.ViewTransportBudgetTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.TransactionCode == code,
                cancellationToken);
    }

    public async Task<TblBudget?> GetTrBudgetData(
        CancellationToken cancellationToken = default)
    {
        return await db.TblBudgets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.BudgetCode == TransportBudgetCode,
                cancellationToken);
    }

    public async Task<List<LogBudgetTransaction>>
        GetBudgetTransactionDetail(
            string code,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new List<LogBudgetTransaction>();
        }

        return await db.LogBudgetTransactions
            .AsNoTracking()
            .Where(w => w.TransactionCode == code)
            .OrderBy(o => o.PaymentType)
            .ToListAsync(cancellationToken);
    }

    public Task<ResponseViewModel> UpdateBalance(
        decimal balance,
        DateTime budget_date,
        string remark,
        string username,
        CancellationToken cancellationToken = default)
    {
        return ChangeBudgetBalanceAsync(
            budgetCode: TransportBudgetCode,
            amount: balance,
            budgetDate: budget_date,
            remark: remark,
            username: username,
            isAddition: true,
            cancellationToken: cancellationToken);
    }

    public Task<ResponseViewModel> AddPayment(
        decimal balance,
        DateTime budget_date,
        string remark,
        string username,
        CancellationToken cancellationToken = default)
    {
        return ChangeBudgetBalanceAsync(
            budgetCode: TransportBudgetCode,
            amount: balance,
            budgetDate: budget_date,
            remark: remark,
            username: username,
            isAddition: false,
            cancellationToken: cancellationToken);
    }

    public async Task<List<TransportBudgetTransactionViewModel>>
        GetTransportBudgetTransaction(
            DateTime start,
            DateTime end,
            CancellationToken cancellationToken = default)
    {
        if (end < start)
        {
            return new List<TransportBudgetTransactionViewModel>();
        }

        var startDate = start.Date;
        var endExclusive = end.Date.AddDays(1);

        var data = await db.ViewTransportBudgetTransactions
            .AsNoTracking()
            .Where(w =>
                w.TransactionDate >= startDate &&
                w.TransactionDate < endExclusive)
            .OrderByDescending(o => o.CreateDate)
            .Select(a => new
            {
                a.TransactionCode,
                a.CreateDate,
                a.JobDetailCode,
                a.BookingNo,
                a.ContainerNo,
                a.CustomerAcronym,
                a.Start,
                a.Mission,
                a.TransactionType,
                a.PaymentType,
                a.JobType,
                a.Amount,
                a.AmountDiff,
                a.Remark
            })
            .ToListAsync(cancellationToken);

        return data
            .Select(a =>
                new TransportBudgetTransactionViewModel
                {
                    transaction_code = a.TransactionCode,

                    transaction_date =
                        a.CreateDate
                            .ToString("yyyy-MM-dd HH:mm"),

                    job_detail_code = a.JobDetailCode,
                    booking = a.BookingNo,
                    container = a.ContainerNo,
                    customer_name = a.CustomerAcronym,
                    start = a.Start,
                    mission = a.Mission,
                    transaction_type = a.TransactionType,
                    payment_type = a.PaymentType,
                    job_type = a.JobType,
                    amount = a.Amount,
                    amount_diff = a.AmountDiff,
                    remark = a.Remark
                })
            .ToList();
    }

    #endregion

    #region GeneralBudget

    public async Task<BudgetViewModel> GetGeBudget(
        CancellationToken cancellationToken = default)
    {
        var budgetData = await db.TblBudgets
            .AsNoTracking()
            .Where(w => w.BudgetCode == GeneralBudgetCode)
            .Select(w => new
            {
                w.Balance,
                w.UpdateDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (budgetData == null)
        {
            throw new InvalidOperationException(
                $"ไม่พบ General Budget ({GeneralBudgetCode})");
        }

        var transactions =
            await db.ViewGeneralBudgetTransactions
                .AsNoTracking()
                .Where(w => w.BudgetType == "GE")
                .OrderByDescending(o => o.CreateDate)
                .Take(5)
                .Select(item => new
                {
                    item.DailypaymentCode,
                    item.Amount,
                    item.AmountDiff,
                    item.TransactionDate,
                    item.CreateDate,
                    item.EmpFullname,
                    item.PaymentType,
                    item.Remark
                })
                .ToListAsync(cancellationToken);

        var list = transactions
            .Select(item =>
                new GeneralBudgetTransactionViewModel
                {
                    dailypayment_code = item.DailypaymentCode,
                    amount = item.Amount,
                    amount_diff = item.AmountDiff,

                    transaction_date =
                        item.TransactionDate
                            .ToString("dd-MM-yyyy"),

                    create_date = item.CreateDate,
                    transaction_by = item.EmpFullname,
                    payment_type = item.PaymentType,
                    remark = item.Remark
                })
            .ToList();

        return new BudgetViewModel
        {
            balance = budgetData.Balance,
            update_date = budgetData.UpdateDate,
            general_transactions = list
        };
    }

    public async Task<TblBudget?> GetGeBudgetData(
        CancellationToken cancellationToken = default)
    {
        return await db.TblBudgets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.BudgetCode == GeneralBudgetCode,
                cancellationToken);
    }

    public Task<ResponseViewModel> UpdateGeneralBalance(
        decimal balance,
        DateTime budget_date,
        string remark,
        string username,
        CancellationToken cancellationToken = default)
    {
        return ChangeBudgetBalanceAsync(
            budgetCode: GeneralBudgetCode,
            amount: balance,
            budgetDate: budget_date,
            remark: remark,
            username: username,
            isAddition: true,
            cancellationToken: cancellationToken);
    }

    public Task<ResponseViewModel> AddGeneralPayment(
        decimal balance,
        DateTime budget_date,
        string remark,
        string username,
        CancellationToken cancellationToken = default)
    {
        return ChangeBudgetBalanceAsync(
            budgetCode: GeneralBudgetCode,
            amount: balance,
            budgetDate: budget_date,
            remark: remark,
            username: username,
            isAddition: false,
            cancellationToken: cancellationToken);
    }

    #endregion

    private async Task<ResponseViewModel> ChangeBudgetBalanceAsync(
        string budgetCode,
        decimal amount,
        DateTime budgetDate,
        string remark,
        string username,
        bool isAddition,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            return Failure("จำนวนเงินต้องมากกว่า 0");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return Failure("ไม่พบผู้ทำรายการ");
        }

        remark = remark?.Trim() ?? string.Empty;

        await using var dbTransaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var budget = await db.TblBudgets
                .FirstOrDefaultAsync(
                    w => w.BudgetCode == budgetCode,
                    cancellationToken);

            if (budget == null)
            {
                await dbTransaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    $"ไม่พบข้อมูลงบประมาณ {budgetCode}");
            }

            var currentBalance = budget.Balance;
            var now = DateTime.Now;

            budget.Balance =
                isAddition
                    ? currentBalance + amount
                    : currentBalance - amount;

            budget.UpdateDate = now;
            budget.UpdateBy = username;

            var budgetTransaction =
                new TblBudgetTransaction
                {
                    TransactionCode =
                        await codeRepo
                            .GetBudgetTransactionCode(
                                cancellationToken),

                    TransactionDate = budgetDate,

                    TransactionType =
                        isAddition
                            ? "GET"
                            : "OTHER",

                    Balance = currentBalance,
                    Amount = amount,
                    AmountDiff = 0,

                    PaymentType =
                        isAddition
                            ? "ADD"
                            : "MIN",

                    Remark = remark,
                    BudgetCode = budgetCode,
                    IsEnabled = true,
                    CreateDate = now,
                    CreateBy = username,
                    UpdateDate = now,
                    UpdateBy = username
                };

            db.TblBudgetTransactions.Add(
                budgetTransaction);

            await db.SaveChangesAsync(
                cancellationToken);

            await dbTransaction.CommitAsync(
                cancellationToken);

            var response = Success();

            if (isAddition)
            {
                response.notifMessage =
                    "รับยอดจำนวน " +
                    amount.ToString("#,##0.00") +
                    " บ.\n" +
                    "รับยอด ณ. วันที่ " +
                    budgetDate.ToString("dd-MM-yyyy") +
                    "\nยอดคงเหลือ : " +
                    budget.Balance.ToString("#,##0.00") +
                    " บ.";
            }
            else
            {
                response.notifMessage =
                    "จ่าย" +
                    remark +
                    " จำนวน " +
                    amount.ToString("#,##0.00") +
                    " บ.\n" +
                    "ณ. วันที่ " +
                    budgetDate.ToString("dd-MM-yyyy") +
                    "\nยอดคงเหลือ : " +
                    budget.Balance.ToString("#,##0.00") +
                    " บ.";
            }

            return response;
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                dbTransaction,
                cancellationToken);

            return Error(
                ex,
                "ChangeBudgetBalance",
                budgetCode);
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
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ" +
                "</div>"
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
                "Error while rolling back Budget transaction.");
        }
    }
}
