using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class FormAVARepository
{
    private readonly DbavahrContext dbHr;
    private readonly DbavalogContext dbLog;

    public FormAVARepository(DbavahrContext contextHr, DbavalogContext contextLog)
    {
        dbLog = contextLog;
        dbHr = contextHr;
    }

    public async Task<List<ContainerHandlingViewModel>> GetContainerHandlingData(string code)
    {
        var data = await (from a in dbLog.TblReceiptOthers
                          join b in dbLog.TblReceiptOtherDetails on a.ReceiptOtherCode equals b.ReceiptOtherCode
                          where a.ReceiptOtherCode == code
                          select new ContainerHandlingViewModel
                          {
                              code = a.ReceiptOtherCode,
                              customer_name = a.CustomerName,
                              customer_tax = a.CustomerTax,
                              customer_address = a.CustomerAddress,
                              receipt_date = a.ReceiptDate.ToString("dd-MM-yyyy"),
                              total = a.Total,
                              vat = a.Vat,
                              net_total = a.NetTotal,
                              payment_type = a.PaymentType,
                              bank_name = a.BankName,
                              branch = a.BankBranchName,
                              cheque_no = a.ChequeNo,
                              cheque_date = a.ChequeDate,
                              container_no = b.ReceiptOtherName,
                              container_size = b.ContainerSize,
                              price = b.Price
                          }).ToListAsync();

        return data;
    }

    public async Task<List<ContainerHandlingViewModel>> GetContainerHandlingDataList(string month, string year)
    {
        var data = await (from a in dbLog.TblReceiptOthers
                          join b in dbLog.TblReceiptOtherDetails on a.ReceiptOtherCode equals b.ReceiptOtherCode
                          where a.IsEnabled == true &&
                                   a.ReceiptDate.Month == int.Parse(month) &&
                                   a.ReceiptDate.Year == int.Parse(year)
                          select new ContainerHandlingViewModel
                          {
                              code = a.ReceiptOtherCode,
                              customer_name = a.CustomerName,
                              customer_tax = a.CustomerTax,
                              customer_address = a.CustomerAddress,
                              receipt_date = a.ReceiptDate.ToString("dd-MM-yyyy"),
                              total = a.Total,
                              vat = a.Vat,
                              net_total = a.NetTotal,
                              payment_type = a.PaymentType,
                              bank_name = a.BankName,
                              branch = a.BankBranchName,
                              cheque_no = a.ChequeNo,
                              cheque_date = a.ChequeDate,
                              container_no = b.ReceiptOtherName,
                              container_size = b.ContainerSize,
                              price = b.Price
                          }).ToListAsync();

        return data;
    }

    public async Task<SalaryFormViewModel> GetTblPaymentSalaryByCode(string payment_code, string emp_code)
    {
        var data = new SalaryFormViewModel();
        var _paymentSalary = await dbHr.TblPaymentSalaries.Where(w => w.PaymentCode == payment_code).FirstOrDefaultAsync();
        var _salaryDetail = await dbHr.TblPaymentSalaryDetails.Where(w => w.PaymentCode == payment_code && w.EmpCode == emp_code).FirstOrDefaultAsync();
        var _addition = await dbHr.TblPaymentSalaryAdditions.Where(w => w.PaymentCode == payment_code && w.EmpCode == emp_code).ToListAsync();
        var _dedict = await dbHr.TblPaymentSalaryDedictions.Where(w => w.PaymentCode == payment_code && w.EmpCode == emp_code).ToListAsync();
        var _employee = await dbHr.TblEmployees.Where(w => w.EmpCode == emp_code).FirstOrDefaultAsync();
        var _contract = await dbHr.TblContracts.Where(w => w.EmpCode == emp_code && w.IsEnabled == true).FirstOrDefaultAsync();
        var _fund = await dbHr.TblEmployeeFunds.Where(w => w.EmpCode == emp_code && w.IsEnabled == true).FirstOrDefaultAsync();
        var _departments = await (from a in dbHr.TblDepartments
                                  join b in dbHr.TblEmployeeDepartments on a.DeptCode equals b.DeptCode
                                  where b.EmpCode == emp_code
                                  select a).FirstOrDefaultAsync();

        data.start_date = this.ChangeMonth(_contract.StartDate.ToString("MM"), _contract.StartDate.ToString("yyyy"));
        data.profile = _employee;
        data.departments = _departments;
        data.payment_date = _paymentSalary.PaymentDate;
        data.payment_month = this.ChangeMonth(_paymentSalary.PaymentMonth, _paymentSalary.PaymentYear);
        data.detail = _salaryDetail;
        data.fund = _fund;
        data.addition = _addition;
        data.deduction = _dedict;

        return data;
    }

    public async Task<TblDailypayment> GetTblDailyPayments(string code)
    {
        var data = await (from a in dbLog.TblDailypayments where a.DailypaymentCode == code select a).FirstOrDefaultAsync();
        data.TblDailypaymentDetails = await dbLog.TblDailypaymentDetails.Where(w => w.DailypaymentCode == code).ToListAsync();

        return data;
    }

    public async Task<TblMonthlypayment> GetTblMonthlyPayments(string code)
    {
        var data = await (from a in dbLog.TblMonthlypayments where a.MonthlyPaymentCode == code select a).FirstOrDefaultAsync();
        data.SupplierCodeNavigation = await dbLog.TblSuppliers.Where(w => w.SupplierCode == data.SupplierCode).FirstOrDefaultAsync();
        data.TblMonthlypaymentDetails = await dbLog.TblMonthlypaymentDetails.Where(w => w.MonthlyPaymentCode == code).ToListAsync();

        return data;
    }

    public async Task<TblReceivePayment> GetReceivePayments(string code)
    {
        var receipt = await (from a in dbLog.TblReceivePaymentReceipts where a.ReceivePaymentCode == code select a).FirstOrDefaultAsync();
        var invoice = await dbLog.TblReceivePayments.Where(w => w.ReceivePaymentCode == code).FirstOrDefaultAsync();
        var detail = await dbLog.TblReceivePaymentDetails.Where(w => w.ReceivePaymentCode == code).ToListAsync();
        invoice.SupplierCodeNavigation = await dbLog.TblSuppliers.Where(w => w.SupplierCode == invoice.SupplierCode).FirstOrDefaultAsync();
        invoice.TblReceivePaymentDetails = await dbLog.TblReceivePaymentDetails.Where(w => w.ReceivePaymentCode == code).ToListAsync();
        invoice.TblReceivePaymentReceipts.Add(receipt);
        invoice.TblReceivePaymentDetails = detail;

        return invoice;
    }

    private string ChangeMonth(string month, string year)
    {
        switch (month)
        {
            case "01":
                month = "มกราคม";
                break;
            case "02":
                month = "กุมพาพันธ์";
                break;
            case "03":
                month = "มีนาคม";
                break;
            case "04":
                month = "เมษายน";
                break;
            case "05":
                month = "พฤษภาคม";
                break;
            case "06":
                month = "มิถุนายน";
                break;
            case "07":
                month = "กรกฎาคม";
                break;
            case "08":
                month = "สิงหาคม";
                break;
            case "09":
                month = "กันยายน";
                break;
            case "10":
                month = "ตุลาคม";
                break;
            case "11":
                month = "พฤศจิกายน";
                break;
            case "12":
                month = "ธันวาคม";
                break;
        }

        return month + " " + year;
    }
}