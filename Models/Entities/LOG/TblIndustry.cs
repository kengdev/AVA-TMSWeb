using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblIndustry
{
    public string IndustryCode { get; set; } = null!;

    public string PortCode { get; set; } = null!;

    public string IndustryNameTh { get; set; } = null!;

    public string IndustryFullname { get; set; } = null!;

    public string IndustryAddress { get; set; } = null!;

    public string IndustryTax { get; set; } = null!;

    public string ZoneCode { get; set; } = null!;

    public string IndustryLat { get; set; } = null!;

    public string IndustryLon { get; set; } = null!;

    public string ProvinceCode { get; set; } = null!;

    public virtual TblPlace IndustryCodeNavigation { get; set; } = null!;

    public virtual LcProvince ProvinceCodeNavigation { get; set; } = null!;

    public virtual TblZone ZoneCodeNavigation { get; set; } = null!;
}
