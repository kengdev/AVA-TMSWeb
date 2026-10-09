using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using TMSWeb.Models.Entities;

namespace TMSWeb.Models;

public class OrderModel
{
    public string job_code { get; set; }
    public string job_detail_code { get; set; }
    public string booking_no { get; set; }
    public string job_invoice { get; set; }
    public string customer_name { get; set; }
    public string container_size { get; set; }
    public string container_type { get; set; }
    public int container_amount { get; set; }
    public string mission_name { get; set; }
    public string job_date { get; set; }
    public DateTime job_date_value { get; set; }
    public string job_type { get; set; }
    public int lifted_amount { get; set; }
}

public class OrderPlaceModel
{
    public string txtCustomerName { get; set; }
    public string acronym { get; set; }
    public string oceanacronym { get; set; }
    public string flooracronym { get; set; }
    public string domesticacronym { get; set; }
    public string txtStartCode { get; set; }
    public string StartName { get; set; }
    public string txtMissionCode { get; set; }
    public string txtFinishCode { get; set; }
    public string MissionName { get; set; }
    public string txtMissionType { get; set; }
    public string txtStartType { get; set; }
    public string txtOceanStartCode { get; set; }
    public string OceanStartName { get; set; }
    public string txtOceanMissionCode { get; set; }
    public string txtOceanFinishCode { get; set; }
    public string OceanMissionName { get; set; }
    public string txtFloorStartCode { get; set; }
    public string txtFloorMissionCode { get; set; }
    public string txtFloorFinishCode { get; set; }
    public int fuel { get; set; }

    public string txtDomesticCostCode { get; set; }
    public string txtDomesticStartCode { get; set; }
    public string FinishName { get; set; }
    public decimal expressway { get; set; }
    public bool chkStartPortYard { get; set; } = false;
    public bool chkFinishPortYard { get; set; } = false;
    public string[] domesticFinishCode { get; set; }
    public string route_code { get; set; }
    public string customer_code { get; set; }
}

public class OrderOceanModel
{
    public string consignee_name { get; set; }
    public string shipper_name { get; set; }
    public string shipping_name { get; set; }
    public string request_name { get; set; }
    public string description { get; set; }
    public string place_code { get; set; }
    public DateTime pickup_date { get; set; }
    public DateOnly eta_date { get; set; }
    public string vessel { get; set; }
    public string change_no { get; set; }
    public string goods_declaration { get; set; }

    public string dossier { get; set; }
    public string[] iBL { get; set; }
    public string[] iContainer { get; set; }
    public string[] iQuantity { get; set; }
    public string[] iWeight { get; set; }
    public string[] iCBM { get; set; }
    public DateTime?[] iDelivery { get; set; }
}

/*public class UpdateDriverInvoice
{
    public string job_detail_code { get; set; }
    public string job_type { get; set; }
    public string driver_code { get; set; }
    public string container_no { get; set; }
    public string seal_no { get; set; }
    public byte[] container_pic { get; set; }
    public string trailer_license { get; set; }
    public byte[] seal_pic { get; set; }
    public decimal tare_weight { get; set; }
    public decimal yard { get; set; }
    public decimal fuel { get; set; }
    public decimal port { get; set; }
    public decimal express { get; set; }
    public decimal lift1 { get; set; }
    public decimal lift2 { get; set; }
    public decimal lift3 { get; set; }
    public string Lat { get; set; }
    public string Lon { get; set; }
    public List<byte[]> floor_images { get; set; }
}

public class OrderManagementModel
{
    public string acronym { get; set; }
    public string driver_code { get; set; }
    public string driver_name { get; set; }
    public string truck_code { get; set; }
    public string truck_license { get; set; }
    public string trailer_license { get; set; }
    public bool trip { get; set; }
}

public class OrderFormModel
{
    public string customer_code { get; set; }
    public string customer_name { get; set; }
    public string customer_address { get; set; }
    public string customer_tax { get; set; }
    public string truck_license { get; set; }
    public string trailer_license { get; set; }
    public string truck_typegroup_code { get; set; }
    public string driver_code { get; set; }
    public string driver_fullname { get; set; }
    public string job_code { get; set; }
    public string job_detail_code { get; set; }
    public string job_type { get; set; }
    public string container_size_code { get; set; }
    public string container_size { get; set; }
    public string port_no { get; set; }
    public string industry { get; set; }
    public int container_amount { get; set; }
    public decimal tare { get; set; }
    public string mission_code { get; set; }
    public string mission_name { get; set; }
    public string zone_name { get; set; }
    public string receipt_name { get; set; }
    public string receipt_address { get; set; }
    public string receipt_tax { get; set; }

    public string mission_vat { get; set; }
    public string mission_address { get; set; }
    public string booking_no { get; set; }
    public string agent_code { get; set; }
    public string start_name { get; set; }
    public string finish_name { get; set; }
    public string container_no { get; set; }
    public string seal_no { get; set; }
    public DateTime job_date { get; set; }
    public string closing_time { get; set; }
    public string job_invoice { get; set; }
    public string job_remark { get; set; }
    public decimal fuel_rate { get; set; }
    public decimal lift { get; set; }
    public decimal port { get; set; }
    public decimal yard { get; set; }
    public decimal express_way { get; set; }
    public string barcode { get; set; }
    public bool is2short { get; set; }
    public string supplyer_code { get; set; }
    public string tracking_status { get; set; }
}

public class JobBookingModel
{
    public string job_code { get; set; }
    public string job_detail_code { get; set; }
    public string job_cashing_code { get; set; }
    public string job_type { get; set; }
    public string customer_name { get; set; }
    public string cost_name { get; set; }
    public decimal? cost_value { get; set; }
    public string booking_no { get; set; }
    public string container_type { get; set; }
    public string container_size { get; set; }
    public string truck_license { get; set; }
    public string trailer_license { get; set; }
    public string driver_fullname { get; set; }
    public string container_no { get; set; }
    public decimal? cashing_total { get; set; }
    public string cashing_remark { get; set; }
    public string start_name { get; set; }
    public string mission_name { get; set; }
    public string job_date { get; set; }
    public DateTime job_date_value { get; set; }
    public bool? is_document { get; set; }
    public bool? is_cashing { get; set; }
    public bool? is_billing { get; set; }
    public string supplyer_name { get; set; }
    public string job_status { get; set; }
    public string tracking_status { get; set; }
    public string seal_no { get; set; }
    public string job_invoice { get; set; }
}

public class JobDetailModel
{
    public string container_no { get; set; }
    public string seal_no { get; set; }
    public string truck_license { get; set; }
    public string trailer_license { get; set; }
    public string driver_name { get; set; }
}

public class JobPlaceModel
{
    public string customer_acronym { get; set; }
    public string start_code { get; set; }
    public string start_name { get; set; }
    public string mission_code { get; set; }
    public string mission_name { get; set; }
    public string finish_code { get; set; }
    public string finish_name { get; set; }
    public string truck_code { get; set; }
    public string truck_license { get; set; }
    public string trailer_license { get; set; }
    public string container_no { get; set; }
    public string seal_no { get; set; }
    public string driver_code { get; set; }
    public string driver_name { get; set; }
    public bool check { get; set; }
    public bool trip { get; set; }
}

public class JobLoadPlaceModel
{
    public DateTime job_date { get; set; }
    public string driver_fullname { get; set; }
    public string truck_license { get; set; }
    public string customer_name { get; set; }
    public string customer_code { get; set; }
    public string start_name { get; set; }
    public string mission_name { get; set; }
    public string finish_name { get; set; }
    public string container_no { get; set; }
    public string booking_no { get; set; }
    public string job_code { get; set; }
    public string tracking_status { get; set; }
    public decimal trip_cost { get; set; }
    public decimal advance_cost { get; set; }
    public string job_invoice { get; set; }
    public bool is_billing { get; set; }
    public List<TblJobadvanceOther> other_list { get; set; }
}

public class FinishJobModel
{
    public string job_detail_code { get; set; }
    public string container_no { get; set; }
    public string seal_no { get; set; }
    public int tare_weight { get; set; }
    public decimal cost_value { get; set; }
    public decimal driver_value { get; set; }
    public decimal supplyer_value { get; set; }
    public string[] nameCashing { get; set; }
    public decimal[] valCashing { get; set; }
    public string[] nameAdvance { get; set; }
    public decimal[] valAdvance { get; set; }
    public decimal[] valSupplyer { get; set; }
    public string username { get; set; }
}

public class ChangeDriverModel
{
    public string job_detail_code { get; set; }
    public string driver_code { get; set; }
    public string truck_code { get; set; }
    public string trailer_license { get; set; }
    public decimal driver_cost { get; set; }
    public string supplyer_code { get; set; }
}

public class CancelJobModel
{
    public string job_detail_code { get; set; }
    public string driver_code { get; set; }
    public decimal trip_cost { get; set; }
    public decimal driver_cost { get; set; }
    public string create_by { get; set; }
}

public class UpdateDataModel
{
    public string job_detail_code { get; set; }
    public string truck_code { get; set; }
    public string new_driver_code { get; set; }
    public string container_no { get; set; }
    public string seal_no { get; set; }
    public int tare_weight { get; set; }
    public string container_size { get; set; }
    public string container_size_code { get; set; }
    public string job_type { get; set; }
    public string supplyer_code { get; set; }
    public string tracking_status { get; set; }
    public string trailer_license { get; set; }
    public string truck_license { get; set; }
    public string finish_place_code { get; set; }
    public string finish_name { get; set; }
}

public class UpdateStatusModel
{
    public string job_detail_code { get; set; }
    public string status { get; set; }
    public DateTime status_stamp { get; set; }
    public string create_by { get; set; }
}*/