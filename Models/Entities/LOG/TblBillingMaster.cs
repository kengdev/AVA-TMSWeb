using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblBillingMaster
{
    public string BillingCode { get; set; } = null!;

    public string BillingType { get; set; } = null!;

    public decimal TransportTotal { get; set; }

    public decimal AdvanceTotal { get; set; }

    public decimal BillingTotal { get; set; }

    public decimal BillingVat { get; set; }

    public decimal BillingNetTotal { get; set; }

    public int BillingAmount { get; set; }

    public string CustomerCode { get; set; } = null!;

    public string MissionPlaceCode { get; set; } = null!;

    public string? VendorNo { get; set; }

    public string? SpecialNo { get; set; }

    public string? BillingRemark { get; set; }

    public DateOnly? ClearingDate { get; set; }

    public DateOnly BillingDate { get; set; }

    public string TaxId { get; set; } = null!;

    public string TaxName { get; set; } = null!;

    public string TaxAddress { get; set; } = null!;

    public bool IsVat { get; set; }

    public bool IsAdvance { get; set; }

    public bool IsClearing { get; set; }

    public bool IsCoversheet { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblPlace MissionPlaceCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblBillingDetail> TblBillingDetails { get; set; } = new List<TblBillingDetail>();

    public virtual ICollection<TblBillingOther> TblBillingOthers { get; set; } = new List<TblBillingOther>();

    public virtual ICollection<TblCoversheetDetail> TblCoversheetDetails { get; set; } = new List<TblCoversheetDetail>();

    public virtual ICollection<TblReceiptDetail> TblReceiptDetails { get; set; } = new List<TblReceiptDetail>();
}
