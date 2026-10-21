using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblJobDetail
{
    public string JobDetailCode { get; set; } = null!;

    public string SupplierCode { get; set; } = null!;

    public string SupplierInvoice { get; set; } = null!;

    public string ContainerNo { get; set; } = null!;

    public string SealNo { get; set; } = null!;

    public decimal TareWeight { get; set; }

    public string TruckCode { get; set; } = null!;

    public string TrailerLicense { get; set; } = null!;

    public string DriverCode { get; set; } = null!;

    public string JobStatus { get; set; } = null!;

    public string TrackingStatus { get; set; } = null!;

    public string JobCode { get; set; } = null!;

    public string StartPlaceCode { get; set; } = null!;

    public string MissionPlaceCode { get; set; } = null!;

    public string FinishPlaceCode { get; set; } = null!;

    public decimal CostValue { get; set; }

    public bool Is2short { get; set; }

    public bool IsDocument { get; set; }

    public bool IsApprove { get; set; }

    public bool IsBilling { get; set; }

    public bool IsSingleTrip { get; set; }

    public bool IsCanceled { get; set; }

    public bool IsPayment { get; set; }

    public bool IsAdvancePaid { get; set; }

    public bool IsAdvanceProve { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblDriver DriverCodeNavigation { get; set; } = null!;

    public virtual TblPlace FinishPlaceCodeNavigation { get; set; } = null!;

    public virtual TblJobMaster JobCodeNavigation { get; set; } = null!;

    public virtual ICollection<LogBudgetTransaction> LogBudgetTransactions { get; set; } = new List<LogBudgetTransaction>();

    public virtual TblPlace MissionPlaceCodeNavigation { get; set; } = null!;

    public virtual TblPlace StartPlaceCodeNavigation { get; set; } = null!;

    public virtual TblSupplier SupplierCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblBillingDetail> TblBillingDetails { get; set; } = new List<TblBillingDetail>();

    public virtual ICollection<TblDriverCost> TblDriverCosts { get; set; } = new List<TblDriverCost>();

    public virtual ICollection<TblFleetpaymentDetail> TblFleetpaymentDetails { get; set; } = new List<TblFleetpaymentDetail>();

    public virtual ICollection<TblJobImg> TblJobImgs { get; set; } = new List<TblJobImg>();

    public virtual ICollection<TblJobTracking> TblJobTrackings { get; set; } = new List<TblJobTracking>();

    public virtual TblJobadvancePayment? TblJobadvancePayment { get; set; }

    public virtual ICollection<TblJobadvance> TblJobadvances { get; set; } = new List<TblJobadvance>();

    public virtual ICollection<TblJobcashing> TblJobcashings { get; set; } = new List<TblJobcashing>();

    public virtual TblShippingadvance? TblShippingadvance { get; set; }

    public virtual ICollection<TblSupplierAllowance> TblSupplierAllowances { get; set; } = new List<TblSupplierAllowance>();

    public virtual ICollection<TblSupplierpaymentDetail> TblSupplierpaymentDetails { get; set; } = new List<TblSupplierpaymentDetail>();

    public virtual TblTruck TruckCodeNavigation { get; set; } = null!;
}
