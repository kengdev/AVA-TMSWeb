using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTruck
{
    public string TruckCode { get; set; } = null!;

    public string TruckLicense { get; set; } = null!;

    public string SupplierCode { get; set; } = null!;

    public string FleetType { get; set; } = null!;

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblSupplier SupplierCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblJobDetail> TblJobDetails { get; set; } = new List<TblJobDetail>();

    public virtual ICollection<TblTruckChecklist> TblTruckChecklists { get; set; } = new List<TblTruckChecklist>();

    public virtual TblTruckDetail? TblTruckDetail { get; set; }

    public virtual ICollection<TblTruckDriver> TblTruckDrivers { get; set; } = new List<TblTruckDriver>();

    public virtual ICollection<TblTruckTrailer> TblTruckTrailers { get; set; } = new List<TblTruckTrailer>();

    public virtual ICollection<TblVatinvoice> TblVatinvoices { get; set; } = new List<TblVatinvoice>();

    public virtual ICollection<TblFleetpayment> FleetpaymentCodes { get; set; } = new List<TblFleetpayment>();
}
