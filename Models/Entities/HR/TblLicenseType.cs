using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblLicenseType
{
    public string LicenseTypeCode { get; set; } = null!;

    public string LicenseTypeName { get; set; } = null!;

    public virtual ICollection<TblEmployeeDriver> TblEmployeeDrivers { get; set; } = new List<TblEmployeeDriver>();
}
