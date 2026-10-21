using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblFleetpaymentDetail
{
    public int Id { get; set; }

    public string JobDetailCode { get; set; } = null!;

    public decimal TransportValue { get; set; }

    public decimal TransportVatValue { get; set; }

    public decimal TransportTotalValue { get; set; }

    public decimal TripValue { get; set; }

    public decimal ExpressValue { get; set; }

    public int FuelLitreValue { get; set; }

    public decimal FuelTotalValue { get; set; }

    public decimal PaymentValue { get; set; }

    public string FleetpaymentCode { get; set; } = null!;

    public virtual TblFleetpayment FleetpaymentCodeNavigation { get; set; } = null!;

    public virtual TblJobDetail JobDetailCodeNavigation { get; set; } = null!;
}
