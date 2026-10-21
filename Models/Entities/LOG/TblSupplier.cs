using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblSupplier
{
    public string SupplierCode { get; set; } = null!;

    public string SupplierName { get; set; } = null!;

    public string SupplierFullname { get; set; } = null!;

    public string SupplierPhone { get; set; } = null!;

    public string SupplierTypeCode { get; set; } = null!;

    public string FleetTypeCode { get; set; } = null!;

    public string SupplierAddress { get; set; } = null!;

    public string SupplierVatNo { get; set; } = null!;

    public string BankName { get; set; } = null!;

    public string BankId { get; set; } = null!;

    public string ContactName { get; set; } = null!;

    public bool IsSpecialVat { get; set; }

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblFleetExternal> TblFleetExternals { get; set; } = new List<TblFleetExternal>();

    public virtual ICollection<TblFleetInternal> TblFleetInternals { get; set; } = new List<TblFleetInternal>();

    public virtual ICollection<TblJobDetail> TblJobDetails { get; set; } = new List<TblJobDetail>();

    public virtual ICollection<TblMonthlypayment> TblMonthlypayments { get; set; } = new List<TblMonthlypayment>();

    public virtual ICollection<TblReceivePayment> TblReceivePayments { get; set; } = new List<TblReceivePayment>();

    public virtual ICollection<TblSupplierAllowance> TblSupplierAllowances { get; set; } = new List<TblSupplierAllowance>();

    public virtual ICollection<TblSupplierRate> TblSupplierRates { get; set; } = new List<TblSupplierRate>();

    public virtual ICollection<TblSupplierpayment> TblSupplierpayments { get; set; } = new List<TblSupplierpayment>();

    public virtual ICollection<TblTruck> TblTrucks { get; set; } = new List<TblTruck>();

    public virtual ICollection<TblVatinvoice> TblVatinvoices { get; set; } = new List<TblVatinvoice>();
}
