using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblJobPlace
{
    public int Id { get; set; }

    public string StartCode { get; set; } = null!;

    public string MissionCode { get; set; } = null!;

    public string FinishCode { get; set; } = null!;

    public int FuelValue { get; set; }

    public decimal LiftonValue { get; set; }

    public decimal LiftoffValue { get; set; }

    public decimal YardValue { get; set; }

    public decimal PortValue { get; set; }

    public decimal ExpresswayValue { get; set; }

    public bool IsStartyardport { get; set; }

    public bool IsFinishyardport { get; set; }

    public string JobCode { get; set; } = null!;

    public virtual TblPlace FinishCodeNavigation { get; set; } = null!;

    public virtual TblJobMaster JobCodeNavigation { get; set; } = null!;

    public virtual TblPlace StartCodeNavigation { get; set; } = null!;
}
