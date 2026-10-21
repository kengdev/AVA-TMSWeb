using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewOrderListActive
{
    public string JobCode { get; set; } = null!;

    public string JobDetailCode { get; set; } = null!;

    public string? HashCode { get; set; }

    public string PlaceName { get; set; } = null!;

    public string IndustryNameTh { get; set; } = null!;

    public string IndustryFullname { get; set; } = null!;

    public string PortCode { get; set; } = null!;

    public string ZoneName { get; set; } = null!;

    public string ReceiptName { get; set; } = null!;

    public string ReceiptAddress { get; set; } = null!;

    public string ReceiptTax { get; set; } = null!;

    public DateTime JobDate { get; set; }

    public string ContainerSizeCode { get; set; } = null!;

    public string ContainerSizeName { get; set; } = null!;

    public string JobType { get; set; } = null!;

    public string BookingNo { get; set; } = null!;

    public string JobInvoice { get; set; } = null!;

    public string AgentCode { get; set; } = null!;

    public string LiftonName { get; set; } = null!;

    public string MissionName { get; set; } = null!;

    public string LiftoffName { get; set; } = null!;

    public string ClosingTime { get; set; } = null!;

    public string JobRemark { get; set; } = null!;

    public int FuelValue { get; set; }

    public decimal LiftonValue { get; set; }

    public decimal LiftoffValue { get; set; }

    public decimal YardValue { get; set; }

    public decimal PortValue { get; set; }

    public decimal ExpresswayValue { get; set; }

    public bool Is2short { get; set; }

    public string TruckTypegroupCode { get; set; } = null!;

    public int ContainerAmount { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsLiftCashing { get; set; }

    public string? OtherAddress { get; set; }
}
