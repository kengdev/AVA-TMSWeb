using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTruckDetail
{
    public string TruckCode { get; set; } = null!;

    public string TruckNo { get; set; } = null!;

    public string TruckChassisNumber { get; set; } = null!;

    public string TruckYear { get; set; } = null!;

    public string BrandCode { get; set; } = null!;

    public string TruckTypegroupCode { get; set; } = null!;

    public string TruckTypeCode { get; set; } = null!;

    public string FuelTypeCode { get; set; } = null!;

    public string TruckImage { get; set; } = null!;

    public virtual TblBrand BrandCodeNavigation { get; set; } = null!;

    public virtual TblFuelType FuelTypeCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblTruckInsurance> TblTruckInsurances { get; set; } = new List<TblTruckInsurance>();

    public virtual TblTruck TruckCodeNavigation { get; set; } = null!;

    public virtual TblTruckType TruckTypeCodeNavigation { get; set; } = null!;
}
