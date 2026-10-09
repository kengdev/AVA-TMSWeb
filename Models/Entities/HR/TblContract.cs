using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblContract
{
    public string ContractCode { get; set; } = null!;

    public string ContractTypeCode { get; set; } = null!;

    public DateOnly ContractDate { get; set; }

    public DateOnly StartDate { get; set; }

    public string EmpCode { get; set; } = null!;

    public string? Remark { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblContractType ContractTypeCodeNavigation { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblEmployeeProbation> TblEmployeeProbations { get; set; } = new List<TblEmployeeProbation>();
}
