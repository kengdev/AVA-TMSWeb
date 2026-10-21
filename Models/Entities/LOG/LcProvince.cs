using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class LcProvince
{
    public string ProvinceCode { get; set; } = null!;

    public string ProvinceNameTha { get; set; } = null!;

    public string ProvinceNameEng { get; set; } = null!;

    public virtual ICollection<TblIndustry> TblIndustries { get; set; } = new List<TblIndustry>();
}
