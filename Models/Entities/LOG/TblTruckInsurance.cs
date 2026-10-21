using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTruckInsurance
{
    public int Id { get; set; }

    public string TruckCode { get; set; } = null!;

    public string InsuranceCode { get; set; } = null!;

    public string InsuranceTypeCode { get; set; } = null!;

    public decimal InsuranceBudget { get; set; }

    public decimal InsuranceIncrease { get; set; }

    public DateTime StartedDate { get; set; }

    public DateTime ExpiredDate { get; set; }

    public string InsuranceDetail { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblInsurance InsuranceCodeNavigation { get; set; } = null!;

    public virtual TblInsuranceType InsuranceTypeCodeNavigation { get; set; } = null!;

    public virtual TblTruckDetail TruckCodeNavigation { get; set; } = null!;
}
