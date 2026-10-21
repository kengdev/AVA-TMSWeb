using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblBudget
{
    public string BudgetCode { get; set; } = null!;

    public string BudgetType { get; set; } = null!;

    public decimal Balance { get; set; }

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblBudgetTransaction> TblBudgetTransactions { get; set; } = new List<TblBudgetTransaction>();
}
