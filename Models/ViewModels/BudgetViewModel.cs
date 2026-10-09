using System;
namespace TMSWeb.Models.ViewModels;
public class BudgetViewModel
{
    public decimal balance { get; set; }
    public DateTime update_date { get; set; }
    public List<TransportBudgetTransactionViewModel> transport_transactions { get; set; }
    public List<GeneralBudgetTransactionViewModel> general_transactions { get; set; }
}

public class TransportBudgetTransactionViewModel
{
    public string transaction_code { get; set; }
    public string transaction_date { get; set; }
    public DateTime transaction_date_value { get; set; }
    public DateTime create_date { get; set; }
    public string transaction_name { get; set; }
    public string transaction_type { get; set; }
    public string payment_type { get; set; }
    public decimal amount { get; set; }
    public decimal amount_diff { get; set; }
    public string job_detail_code { get; set; }
    public string job_type { get; set; }
    public string booking { get; set; }
    public string container { get; set; }
    public string customer_name { get; set; }
    public string start { get; set; }
    public string mission { get; set; }
    public string remark { get; set; }
}

public class GeneralBudgetTransactionViewModel
{
    public string transaction_code { get; set; }
    public string transaction_date { get; set; }
    public DateTime transaction_date_value { get; set; }
    public DateTime create_date { get; set; }
    public string dailypayment_code { get; set; }
    public string transaction_by { get; set; }
    public string payment_type { get; set; }
    public decimal amount { get; set; }
    public decimal amount_diff { get; set; }
    public string remark { get; set; }
}