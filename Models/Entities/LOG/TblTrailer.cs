using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTrailer
{
    public string TrailerCode { get; set; } = null!;

    public string TrailerLicense { get; set; } = null!;

    public string TrailerNo { get; set; } = null!;

    public string TrailerTypeCode { get; set; } = null!;

    public string TrailerFloorCode { get; set; } = null!;

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblTrailerInsurance> TblTrailerInsurances { get; set; } = new List<TblTrailerInsurance>();

    public virtual ICollection<TblTruckTrailer> TblTruckTrailers { get; set; } = new List<TblTruckTrailer>();

    public virtual TblTrailerFloor TrailerFloorCodeNavigation { get; set; } = null!;

    public virtual TblTrailerType TrailerTypeCodeNavigation { get; set; } = null!;
}
