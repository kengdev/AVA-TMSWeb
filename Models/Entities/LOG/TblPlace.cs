using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblPlace
{
    public string PlaceCode { get; set; } = null!;

    public string PlaceName { get; set; } = null!;

    public string PlaceType { get; set; } = null!;

    public decimal ExpresswayValue { get; set; }

    public string? PlaceRemark { get; set; }

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblBillingMaster> TblBillingMasters { get; set; } = new List<TblBillingMaster>();

    public virtual ICollection<TblCustomerPage> TblCustomerPages { get; set; } = new List<TblCustomerPage>();

    public virtual ICollection<TblFleetRate> TblFleetRates { get; set; } = new List<TblFleetRate>();

    public virtual TblIndustry? TblIndustry { get; set; }

    public virtual ICollection<TblJobDetail> TblJobDetailFinishPlaceCodeNavigations { get; set; } = new List<TblJobDetail>();

    public virtual ICollection<TblJobDetail> TblJobDetailMissionPlaceCodeNavigations { get; set; } = new List<TblJobDetail>();

    public virtual ICollection<TblJobDetail> TblJobDetailStartPlaceCodeNavigations { get; set; } = new List<TblJobDetail>();

    public virtual ICollection<TblJobPlace> TblJobPlaceFinishCodeNavigations { get; set; } = new List<TblJobPlace>();

    public virtual ICollection<TblJobPlace> TblJobPlaceStartCodeNavigations { get; set; } = new List<TblJobPlace>();

    public virtual ICollection<TblSupplierRate> TblSupplierRates { get; set; } = new List<TblSupplierRate>();

    public virtual TblYardport? TblYardport { get; set; }

    public virtual ICollection<TblCustomer> CustomerCodes { get; set; } = new List<TblCustomer>();
}
