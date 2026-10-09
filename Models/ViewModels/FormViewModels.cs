using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMSWeb.Models;
using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;

namespace TMSWeb.Models.ViewModels;

public class QuotationFormViewModel
{
    public string quotation_code { get; set; }
    public decimal start_rate { get; set; }
    public DateTime quotation_date { get; set; }
    public DateTime quotation_expire { get; set; }
    public string customer_name { get; set; }
    public string contact { get; set; }
    public string contact_line { get; set; }
    public DateOnly start { get; set; }
    public DateOnly expire { get; set; }
    public string route { get; set; }
    public string scope { get; set; }
    public string commodity { get; set; }
    public string container1 { get; set; }
    public string container2 { get; set; }
    public string container3 { get; set; }
    public string condition_hour { get; set; }
    public string condition_charged { get; set; }
    public decimal minimum_rate { get; set; }
    public decimal price_increase { get; set; }
    public decimal price_length { get; set; }
    public decimal charge_cutchassis { get; set; }
    public decimal insurance_rate { get; set; }
    public string remark { get; set; }
    public List<TblQuotationDetail> detail { get; set; }
    public List<TblQuotationCondition> condition { get; set; }
}

public class OrderFormViewModel
{
    public string customer_code { get; set; }
    public string customer_name { get; set; }
    public string customer_address { get; set; }
    public string other_address { get; set; }
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
    public int fuel_rate { get; set; }
    public decimal lift { get; set; }
    public decimal port { get; set; }
    public decimal yard { get; set; }
    public decimal express_way { get; set; }
    public string barcode { get; set; }
    public bool is2short { get; set; }
    public string supplyer_code { get; set; }
    public string tracking_status { get; set; }
    public string fleet_type { get; set; }
    public string delivery_date { get; set; }
}

public class BillingFormViewModel
{
    public string job_invoice { get; set; }
    public string job_detail_code { get; set; }
    public string billing_code { get; set; }
    public decimal billing_total { get; set; }
    public int billing_amount { get; set; }
    public string customer_code { get; set; }
    public string customer_name_th { get; set; }
    public string job_date { get; set; }
    public DateOnly billing_date { get; set; }
    public string booking_no { get; set; }
    public string container_no { get; set; }
    public string container_size { get; set; }
    public string container_type { get; set; }
    public string mission_name { get; set; }
    public string job_remark { get; set; }
    public decimal cost_value { get; set; }
    public decimal advance_lifton { get; set; }
    public decimal advance_liftoff { get; set; }
    public decimal advance_yard { get; set; }
    public decimal advance_port { get; set; }
    public string job_description { get; set; }
    public string officer { get; set; }
    public bool is_advance { get; set; }
    public bool is_vat { get; set; }
    public string billing_remark { get; set; }
}

public class AdvanceFormViewModel
{
    public string job_detail_code { get; set; }
    public DateTime job_date { get; set; }
    public string booking_no { get; set; }
    public string job_invoice { get; set; }
    public string container_no { get; set; }
    public string container_size { get; set; }
    public string start_name { get; set; }
    public string mission_name { get; set; }
    public string advance_other { get; set; }
    public decimal advance_other_value { get; set; }
    public string customer_code { get; set; }
}

public class BillingPdfFormViewModel
{
    public string billing_code { get; set; }
    public DateOnly billing_date { get; set; }
    public string vendor_no { get; set; }
    public decimal advance_total { get; set; }
    public decimal transport_total { get; set; }
    public decimal vat { get; set; }
    public decimal net_transport_total { get; set; }
    public decimal net_total { get; set; }
    public decimal s_port { get; set; }
    public decimal s_lifton { get; set; }
    public decimal s_liftoff { get; set; }
    public decimal s_transport { get; set; }
    public string col { get; set; }
    public string remark { get; set; }
    public string tax_id { get; set; }
    public string tax_name { get; set; }
    public string tax_address { get; set; }
    public string special_no { get; set; }
    public TblCustomer customer { get; set; }
    public List<BillingFormViewModel> billing { get; set; }
    public List<AdvanceFormViewModel> advance { get; set; }
}

public class ReceiptFormViewModel
{
    public string receipt_code { get; set; }
    public string receipt_part_code { get; set; }
    public string receipt_name { get; set; }
    public string customer_name { get; set; }
    public string customer_address { get; set; }
    public string customer_tax { get; set; }
    public DateTime create_date { get; set; }
    public string receipt { get; set; }
    public decimal transport_value { get; set; }
    public decimal advance_value { get; set; }
    public string receipt_type { get; set; }
    public DateOnly receipt_date { get; set; }
    public string bank_name { get; set; }
    public string bank_branch_name { get; set; }
    public string cheque_no { get; set; }
    public string receipt_remark { get; set; }
    public decimal receipt_total { get; set; }
    public bool is_excharge { get; set; }
    public string excharge_topic { get; set; }
}

public class ReceiptDetailFormViewModel
{
    public string receipt_code { get; set; }
    public string billing_code { get; set; }
    public string billing_type { get; set; }
    public decimal transport_value { get; set; }
    public decimal advance_value { get; set; }
    public decimal receipt_total { get; set; }
}

public class CoverSheetPdfFormViewModel
{
    public TblCustomer customer { get; set; }

    public List<CoverSheetFormViewModel> coversheet { get; set; }
}

public class CoverSheetFormViewModel
{
    public string coversheet_code { get; set; }
    public string billing_code { get; set; }
    public string billing_type { get; set; }
    public decimal coversheet_value { get; set; }
    public decimal billing_value { get; set; }
    public decimal coversheet_total { get; set; }
    public DateOnly coversheet_date { get; set; }
    public string fullname { get; set; }
    public string customer_code { get; set; }
    public bool is_advance { get; set; }
    public bool is_vat { get; set; }
}

public class OceanJobFormViewModel
{
    public int quantity { get; set; }
    public string container_type { get; set; }
    public string start_address { get; set; }
    public string mission_address { get; set; }
    public string vessel { get; set; }
    public string bl_no { get; set; }
    public DateOnly eta_date { get; set; }
    public DateTime delivery_date { get; set; }
    public DateTime pickup_date { get; set; }
    public string change_no { get; set; }
    public string dossier { get; set; }
    public string container { get; set; }
    public List<string> container_list { get; set; }
    public string quantity_detail { get; set; }
    public string weight { get; set; }
    public string cbm { get; set; }
    public string driver_name { get; set; }
    public string driver_phone { get; set; }
    public string license { get; set; }
    public string consignee_name { get; set; }
    public string shipper_name { get; set; }
    public string shipping_name { get; set; }
    public string request_name { get; set; }
    public string description { get; set; }
    public string job_incoterm { get; set; }
    public string goods_declaration { get; set; }
}

public class DriverPaymentFormViewModel
{
    public string driver_name { get; set; }
    public string truck_license { get; set; }
    public string truck_no { get; set; }
    public decimal trip_value { get; set; }
    public decimal fuel_value { get; set; }
    public decimal waste_value { get; set; }
    public decimal other_value { get; set; }
    public decimal total_substract { get; set; }
    public decimal total_addit { get; set; }
    public decimal net_total { get; set; }
    public List<PaymentFormViewModel> data { get; set; }
    public List<SpecialFormViewModel> substract { get; set; }
    public List<SpecialFormViewModel> addit { get; set; }
    public List<TblAllowanceDetail> allowance { get; set; }
    public ViewDriverListActive driver { get; set; }
}

public class PaymentFormViewModel
{
    public string allowance_detail_code { get; set; }
    public string job_detail_code { get; set; }
    public string booking_no { get; set; }
    public string container_no { get; set; }
    public string trip_place { get; set; }
    public string working_date { get; set; }
    public DateTime allownace_date { get; set; }
    public string truck_license { get; set; }
    public string driver_code { get; set; }
    public decimal fuel_value { get; set; }
    public decimal trip_value { get; set; }
    public decimal waste_value { get; set; }
    public decimal other_value { get; set; }
    public bool is_approve { get; set; }
}

public class SpecialFormViewModel
{
    public string type { get; set; }
    public string name { get; set; }
    public decimal value { get; set; }
}

public class FleetFormViewModel
{
    public string truck_no { get; set; }
    public string license { get; set; }
    public string supplier_name { get; set; }
    public string supplier_address { get; set; }
    public string supplier_tax { get; set; }
    public string payment_code { get; set; }
    public DateTime payment_date { get; set; }
    public decimal payment_total { get; set; }
    public decimal transport_total { get; set; }
    public decimal transport_vat { get; set; }
    public decimal transport_net { get; set; }
    public decimal trip_total { get; set; }
    public decimal fuel_total { get; set; }
    public decimal salary_total { get; set; }
    public decimal shore_total { get; set; }
    public decimal area_total { get; set; }
    public decimal express_total { get; set; }
    public decimal maintenance_total { get; set; }
    public decimal tyre_total { get; set; }
    public decimal tyre_service_total { get; set; }
    public decimal other { get; set; }
    public decimal fleetpayment_total { get; set; }
    public decimal fleetpayment_vat_total { get; set; }
    public decimal fleetpayment_net_total { get; set; }
    public string remark { get; set; }
    public List<SupplierFormDetailViewModel> detail { get; set; }
    public List<AdditionalChargeViewModel> addit { get; set; }
}

public class AdditionalChargeViewModel
{
    public string supplyerpayment_code { get; set; }
    public string container_no { get; set; }
    public string booking_no { get; set; }
    public DateTime job_date { get; set; }
    public string mission_name { get; set; }
    public string other_name { get; set; }
    public decimal other_value { get; set; }
}

public class SupplierFormDetailViewModel
{
    public DateTime job_date { get; set; }
    public string invoice_no { get; set; }
    public string booking_no { get; set; }
    public string container_no { get; set; }
    public string mission_name { get; set; }
    public decimal transport_value { get; set; }
    public decimal advance_value { get; set; }
    public decimal port_value { get; set; }
    public decimal yard_value { get; set; }
    public decimal lifton_value { get; set; }
    public decimal liftoff_value { get; set; }
    public decimal total_value { get; set; }
    public decimal additional_value { get; set; }
    public decimal trip_value { get; set; }
    public decimal fuel_value { get; set; }
    public decimal express_value { get; set; }
    public decimal other { get; set; }
}

public class PND50ViewModel
{
    public DateOnly invoice_date { get; set; }
    public string code { get; set; }
    public string supplyer_name { get; set; }
    public string supplyer_address { get; set; }
    public string supplyer_vat_no { get; set; }
    public decimal total_value { get; set; }
    public decimal vat_value { get; set; }
    public decimal net_value { get; set; }
    public string vat_text { get; set; }

    public string customer_name { get; set; }
    public string customer_address { get; set; }
    public string customer_vat_no { get; set; }
    public string invoice_type { get; set; }
    public string supplyer_type { get; set; }
}

public class SupplierFormViewModel
{
    public string supplier_name { get; set; }
    public string supplier_address { get; set; }
    public string supplier_tax { get; set; }
    public string supplierpayment_code { get; set; }
    public string payment_type { get; set; }
    public string cheque_no { get; set; }
    public DateOnly supplierpayment_date { get; set; }
    public decimal supplierpayment_total { get; set; }
    public decimal additional_total { get; set; }
    public decimal total_transport { get; set; }
    public decimal transport_vat { get; set; }
    public decimal total_advance { get; set; }
    public decimal total_port { get; set; }
    public decimal total_yard { get; set; }
    public decimal total_lifton { get; set; }
    public decimal total_liftoff { get; set; }
    public string remark { get; set; }
    public List<SupplierFormDetailViewModel> detail { get; set; }
    public List<AdditionalChargeViewModel> addit { get; set; }
}

public class SupplierPaymentFormViewModel
{
    public SupplierFormViewModel data { get; set; }
    public TblSupplier supplyer { get; set; }
}

public class ClearingFormViewModel
{
    public TblShippingpayment data { get; set; }
    public List<JobViewModel> list { get; set; }
}

public class SalaryFormViewModel
{
    public DateOnly payment_date { get; set; }
    public string payment_month { get; set; }
    public TblPaymentSalaryDetail detail { get; set; }
    public TblEmployeeFund fund { get; set; }
    public List<TblPaymentSalaryAddition> addition { get; set; }
    public List<TblPaymentSalaryDediction> deduction { get; set; }
    public TblDepartment departments { get; set; }
    public TblEmployee profile { get; set; }
    public string start_date { get; set; }
}