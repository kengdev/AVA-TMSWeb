using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblEmployeeDriver
{
    public int Id { get; set; }

    public string EmpCode { get; set; } = null!;

    public string DriverCode { get; set; } = null!;

    public string LicenseTypeCode { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;

    public virtual TblLicenseType LicenseTypeCodeNavigation { get; set; } = null!;
}
