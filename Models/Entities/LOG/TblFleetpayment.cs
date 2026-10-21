using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblFleetpayment
{
    public string FleetpaymentCode { get; set; } = null!;

    public string ReceiverName { get; set; } = null!;

    public string ReceiverAddress { get; set; } = null!;

    public string ReceiverTax { get; set; } = null!;

    public string FleetpaymentMonth { get; set; } = null!;

    public string FleetpaymentYear { get; set; } = null!;

    public string TruckCode { get; set; } = null!;

    public decimal TransportTotal { get; set; }

    public decimal TransportVat { get; set; }

    public decimal TransportNet { get; set; }

    public decimal TripTotal { get; set; }

    public decimal FuelLitreTotal { get; set; }

    public decimal FuelRate { get; set; }

    public decimal FuelTotal { get; set; }

    public decimal ExpressTotal { get; set; }

    public decimal SalaryTotal { get; set; }

    public decimal ShoreTotal { get; set; }

    public decimal AreaTotal { get; set; }

    public decimal TyreTotal { get; set; }

    public decimal TyreServiceTotal { get; set; }

    public decimal MaintenanceTotal { get; set; }

    public decimal Other { get; set; }

    public decimal PaymentTotal { get; set; }

    public decimal FleetpaymentTotal { get; set; }

    public decimal FleetpaymentVat { get; set; }

    public decimal FleetpaymentNetTotal { get; set; }

    public int FleetpaymentAmount { get; set; }

    public string? FleetpaymentRemark { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblFleetpaymentDetail> TblFleetpaymentDetails { get; set; } = new List<TblFleetpaymentDetail>();

    public virtual ICollection<TblTruck> TruckCodes { get; set; } = new List<TblTruck>();
}
