using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblJobOcean
{
    public int Id { get; set; }

    public string ContainerNo { get; set; } = null!;

    public string OrderQuantity { get; set; } = null!;

    public string OrderWeight { get; set; } = null!;

    public string OrderCbm { get; set; } = null!;

    public DateTime PickupDate { get; set; }

    public DateTime DeliveryDate { get; set; }

    public DateOnly EtaDate { get; set; }

    public string GoodsDeclarationNo { get; set; } = null!;

    public string VesselName { get; set; } = null!;

    public string BlNo { get; set; } = null!;

    public string ChangeNo { get; set; } = null!;

    public string PlaceCode { get; set; } = null!;

    public string ConsigneeName { get; set; } = null!;

    public string ShipperName { get; set; } = null!;

    public string ShippingName { get; set; } = null!;

    public string RequestName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string JobCode { get; set; } = null!;

    public virtual TblJobMaster JobCodeNavigation { get; set; } = null!;
}
