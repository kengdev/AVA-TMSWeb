using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class ViewTruckListActive
{
    public string TruckCode { get; set; } = null!;

    public string TruckNo { get; set; } = null!;

    public string TruckLicense { get; set; } = null!;

    public string TruckChassisNumber { get; set; } = null!;

    public string TruckYear { get; set; } = null!;

    public string TruckTypeName { get; set; } = null!;

    public string BrandName { get; set; } = null!;

    public string FuelTypeName { get; set; } = null!;

    public string TruckImage { get; set; } = null!;

    public string SupplierCode { get; set; } = null!;

    public string SupplierName { get; set; } = null!;

    public bool IsActived { get; set; }

    public string DriverCode { get; set; } = null!;
}
