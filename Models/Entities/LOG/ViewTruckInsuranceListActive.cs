using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewTruckInsuranceListActive
{
    public string TruckLicense { get; set; } = null!;

    public string InsuranceCode { get; set; } = null!;

    public string InsuranceName { get; set; } = null!;

    public string InsuranceTypeName { get; set; } = null!;

    public decimal InsuranceBudget { get; set; }

    public decimal InsuranceIncrease { get; set; }

    public DateTime StartedDate { get; set; }

    public DateTime ExpiredDate { get; set; }

    public string InsuranceDetail { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public string InsuranceTypeCode { get; set; } = null!;
}
