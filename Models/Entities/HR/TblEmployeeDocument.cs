using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblEmployeeDocument
{
    public int Id { get; set; }

    public string DocumentType { get; set; } = null!;

    public string DocumentPath { get; set; } = null!;

    public string DocumentName { get; set; } = null!;

    public string DocumentDetail { get; set; } = null!;

    public string EmpCode { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;
}
