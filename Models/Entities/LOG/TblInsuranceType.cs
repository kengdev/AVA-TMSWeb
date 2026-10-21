using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblInsuranceType
{
    public string InsuranceTypeCode { get; set; } = null!;

    public string InsuranceTypeName { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblTrailerInsurance> TblTrailerInsurances { get; set; } = new List<TblTrailerInsurance>();

    public virtual ICollection<TblTruckInsurance> TblTruckInsurances { get; set; } = new List<TblTruckInsurance>();
}
