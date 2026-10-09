using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblEmployeeProbation
{
    public int Id { get; set; }

    public string ContractCode { get; set; } = null!;

    public int ProbationDays { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly FinishDate { get; set; }

    public string ProbationStatus { get; set; } = null!;

    public string Remark { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public virtual TblContract ContractCodeNavigation { get; set; } = null!;
}
