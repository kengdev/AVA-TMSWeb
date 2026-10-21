using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblSupplierpayment
{
    public string SupplierpaymentCode { get; set; } = null!;

    public DateOnly SupplierpaymentDate { get; set; }

    public string PaymentType { get; set; } = null!;

    public string? ChequeNo { get; set; }

    public decimal TransportTotal { get; set; }

    public decimal TransportVat { get; set; }

    public decimal TotalPort { get; set; }

    public decimal TotalYard { get; set; }

    public decimal TotalLifton { get; set; }

    public decimal TotalLiftoff { get; set; }

    public decimal TotalAdvance { get; set; }

    public decimal TotalAdditionalcharge { get; set; }

    public decimal TransportNet { get; set; }

    public decimal SupplierpaymentNetTotal { get; set; }

    public int SupplierpaymentAmount { get; set; }

    public string SupplierCode { get; set; } = null!;

    public string? SupplierpaymentRemark { get; set; }

    public bool IsPort { get; set; }

    public bool IsYard { get; set; }

    public bool IsLifton { get; set; }

    public bool IsLiftoff { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblSupplier SupplierCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblSupplierpaymentDetail> TblSupplierpaymentDetails { get; set; } = new List<TblSupplierpaymentDetail>();

    public virtual ICollection<TblWithholdingTaxSupplierpayment> TblWithholdingTaxSupplierpayments { get; set; } = new List<TblWithholdingTaxSupplierpayment>();
}
