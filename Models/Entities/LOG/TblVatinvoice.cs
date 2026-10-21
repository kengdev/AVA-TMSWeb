using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblVatinvoice
{
    public string VatinvoiceCode { get; set; } = null!;

    public string VatinvoiceType { get; set; } = null!;

    public DateOnly VatinvoiceDate { get; set; }

    public string SupplierCode { get; set; } = null!;

    public string TruckCode { get; set; } = null!;

    public decimal PaymentTotal { get; set; }

    public decimal VatTotal { get; set; }

    public decimal PaymentNetTotal { get; set; }

    public string Remark { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblSupplier SupplierCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblVatinvoicePayment> TblVatinvoicePayments { get; set; } = new List<TblVatinvoicePayment>();

    public virtual TblTruck TruckCodeNavigation { get; set; } = null!;
}
