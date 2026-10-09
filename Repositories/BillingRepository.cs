using Dapper;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class BillingRepository
{
    private readonly DbavalogContext db;
    private readonly string connStr;
    private readonly IAuditLogService auditLogService;
    private readonly CodeRepository codeRepo;
    private readonly ILogger<BillingRepository> logger;

    public BillingRepository(
        DbavalogContext context,
        IAuditLogService _auditLogService,
        ILogger<BillingRepository> _logger,
        CodeRepository _codeRepo,
        CodeRepository codeRepo)
    {
        db = context;
        auditLogService = _auditLogService;
        logger = _logger;
        codeRepo = _codeRepo;
        connStr = db.Database.GetConnectionString();
    }

    public async Task<List<ViewJobListActive>> GetTotalUnBilling()
    {
        var data = await db.ViewJobListActives
                        .Where(w => w.IsBilling == false)
                        .ToListAsync();
        return data;
    }

    public async Task<List<JobViewModel>> GetListBilling()
    {
        var data = await (from a in db.TblJobMasters
                          join b in db.TblJobDetails on a.JobCode equals b.JobCode
                          join c in db.TblCustomers on a.CustomerCode equals c.CustomerCode
                          join d in db.TblPlaces on b.StartPlaceCode equals d.PlaceCode
                          join e in db.TblPlaces on b.MissionPlaceCode equals e.PlaceCode
                          join f in db.TblTrucks on b.TruckCode equals f.TruckCode
                          join g in db.TblCustomerRules on c.CustomerCode equals g.CustomerCode

                          where ((b.IsBilling == false && b.IsApprove == true) ||
                                (g.IsBypassTripcost == true && b.IsBilling == false))
                          select new JobViewModel
                          {
                              job_code = a.JobCode,
                              job_detail_code = b.JobDetailCode,
                              job_type = a.JobType,
                              customer_name = c.CustomerAcronym,
                              start_name = d.PlaceName,
                              booking_no = a.BookingNo,
                              container_type = a.ContainerType,
                              container_size = a.ContainerSizeName,
                              truck_license = f.TruckLicense,
                              cost_value = b.CostValue,
                              trailer_license = b.TrailerLicense,
                              container_no = b.ContainerNo,
                              job_date = a.JobDate.ToString("yyyy-MM-dd HH:mm"),
                              job_date_value = a.JobDate,
                              mission_name = e.PlaceName
                          }).ToListAsync();
        return data;
    }

    public async Task<TblBillingDetail> GetBillingDetailData(int id)
    {
        var data = await db.TblBillingDetails.Where(w => w.Id == id).FirstOrDefaultAsync();
        return data;
    }

    public async Task<BillingViewModel> GetBillingDetailByJobDetailCode(string job_detail_code)
    {
        var data = await (from a in db.TblBillingDetails
                          join b in db.TblBillingMasters on a.BillingCode equals b.BillingCode
                          where a.JobDetailCode == job_detail_code
                          select new BillingViewModel
                          {
                              billing_code = b.BillingCode,
                              billing_date = b.BillingDate.ToString("dd-MM-yyyy")
                          }).FirstOrDefaultAsync();
        return data;
    }

    public async Task<TblJobadvance> GetJobAdvanceByCode(string job_detail_code)
    {
        var data = await db.TblJobadvances.Where(w => w.JobDetailCode == job_detail_code).FirstOrDefaultAsync();
        return data;
    }

    public async Task<List<BillingViewModel>> GetListBillingData()
    {
        var data = await (from a in db.TblBillingMasters
                          join b in db.TblCustomers on a.CustomerCode equals b.CustomerCode
                          join d in db.TblPlaces on a.MissionPlaceCode equals d.PlaceCode
                          where a.IsEnabled == true && a.IsClearing == false && a.IsCoversheet == false
                          select new BillingViewModel
                          {
                              billing_code = a.BillingCode,
                              billing_total = a.BillingNetTotal,
                              billing_amount = a.BillingAmount,
                              customer_code = a.CustomerCode,
                              customer_name = b.CustomerAcronym,
                              is_clearing = a.IsClearing,
                              billing_type = a.BillingType,
                              mission_name = d.PlaceName,
                              billing_date = a.BillingDate.ToString("yyyy-MM-dd"),
                              billing_date_value = a.BillingDate,
                              clearing_date = a.ClearingDate == null ? "-" : a.ClearingDate.Value.ToString("yyyy-MM-dd")
                          }).ToListAsync();
        return data;
    }

    public async Task<ResponseViewModel> AddBilling(TblBillingMaster billingToAdd, string[] listChecked, decimal[] listValue)
    {
        ResponseViewModel response = new ResponseViewModel();
        using (TransactionScope tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var list_detail = new List<TblBillingDetail>();
                decimal total_advance = 0;
                decimal total_other = 0;
                var job_detail_code = listChecked[0];
                var item = await db.ViewJobListActives.Where(w => w.JobDetailCode == job_detail_code).FirstOrDefaultAsync();
                var taxCustomer = await db.TblCustomers.Where(w => w.CustomerCode == item.CustomerCode).FirstOrDefaultAsync();
                var taxAddress = await db.TblCustomerAddresses.Where(w => w.CustomerCode == item.CustomerCode).FirstOrDefaultAsync();

                billingToAdd.CustomerCode = item.CustomerCode;
                billingToAdd.TaxId = taxCustomer.CustomerVatNo;
                billingToAdd.TaxName = taxAddress.TransportName;
                billingToAdd.TaxAddress = taxAddress.TransportAddress;
                billingToAdd.IsClearing = false;
                billingToAdd.IsEnabled = true;

                if (billingToAdd.IsAdvance == true)
                {
                    for (int i = 0; i < listChecked.Count(); i++)
                    {
                        var detail = new TblBillingDetail();
                        detail.JobDetailCode = listChecked[i];
                        detail.BillingValue = listValue[i];
                        detail.BillingCode = billingToAdd.BillingCode;

                        var advance = await db.TblJobadvances.FirstOrDefaultAsync(w => w.JobDetailCode == detail.JobDetailCode);
                        if (advance.IsBilling != true)
                        {
                            if (item.IsLiftCashing == false)
                            {
                                advance.IsBilling = true;
                                total_advance = total_advance + advance.AdvanceTotalup;
                                detail.AdvanceValue = advance.AdvanceTotalup;
                                list_detail.Add(detail);
                            }
                            else
                            {
                                detail.AdvanceValue = 0;
                                list_detail.Add(detail);
                            }
                        }
                        else
                        {
                            detail.AdvanceValue = 0;
                            list_detail.Add(detail);
                        }

                        var job_detail = await db.TblJobDetails.FirstOrDefaultAsync(w => w.JobDetailCode == detail.JobDetailCode);
                        job_detail.IsBilling = true;
                        billingToAdd.MissionPlaceCode = job_detail.MissionPlaceCode;

                        var other = await db.TblJobadvanceOthers.Where(w => w.AdvanceCode == advance.AdvanceCode).ToListAsync();
                        if (other.Count() > 0)
                        {
                            total_other = other.Sum(s => s.AdvanceOtherValueup) + total_other;
                        }
                    }

                    billingToAdd.BillingType = "M";
                    billingToAdd.AdvanceTotal = total_advance;
                    billingToAdd.BillingTotal = billingToAdd.TransportTotal + billingToAdd.AdvanceTotal;
                    billingToAdd.BillingVat = billingToAdd.IsVat == true ? (billingToAdd.TransportTotal * 1) / 100 : 0;
                    billingToAdd.BillingNetTotal = (billingToAdd.TransportTotal - billingToAdd.BillingVat) + total_advance;
                }
                else
                {
                    for (int i = 0; i < listChecked.Count(); i++)
                    {
                        var detail = new TblBillingDetail();
                        detail.JobDetailCode = listChecked[i];

                        var job_detail = await db.TblJobDetails.FirstOrDefaultAsync(w => w.JobDetailCode == detail.JobDetailCode);
                        job_detail.IsBilling = true;
                        billingToAdd.MissionPlaceCode = job_detail.MissionPlaceCode;

                        detail.BillingValue = listValue[i];
                        detail.AdvanceValue = 0;
                        detail.BillingCode = billingToAdd.BillingCode;
                        list_detail.Add(detail);
                    }

                    billingToAdd.BillingType = "T";
                    billingToAdd.AdvanceTotal = 0;
                    billingToAdd.BillingTotal = billingToAdd.TransportTotal + billingToAdd.AdvanceTotal;
                    billingToAdd.BillingVat = billingToAdd.IsVat == true ? (billingToAdd.TransportTotal * 1) / 100 : 0;
                    billingToAdd.BillingNetTotal = billingToAdd.TransportTotal - billingToAdd.BillingVat;


                }

                if (total_other > 0)
                {
                    await AddOtherInvoice(billingToAdd, list_detail);
                }

                Console.WriteLine("TaxId = [" + billingToAdd.TaxId + "]");
                Console.WriteLine("TaxName = [" + billingToAdd.TaxName + "]");
                Console.WriteLine("TaxAddress = [" + billingToAdd.TaxAddress + "]");

                billingToAdd.TblBillingDetails = list_detail;
                db.TblBillingMasters.Add(billingToAdd);
                await db.SaveChangesAsync();
                tr.Complete();
                response.result = true;
                response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                response.result = false;
                response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
                logger.LogError(ex, "BillingRepository operation failed.");
                response.errorException = null;
            }
        }

        return response;
    }

    public async Task<ResponseViewModel> AddOther(TblBillingMaster billingToAdd, int[] listChecked, decimal[] listValue)
    {
        ResponseViewModel response = new ResponseViewModel();
        using (TransactionScope tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var advance_code = "";
                var list_detail = new List<TblBillingOther>();
                for (int i = 0; i < listChecked.Count(); i++)
                {
                    var advance = db.TblJobadvanceOthers.FirstOrDefault(w => w.Id == listChecked[i]);
                    advance.IsBilling = true;
                    advance_code = advance.AdvanceCode;

                    var i_detail = new TblBillingOther();
                    i_detail.AdvanceCode = advance.AdvanceCode;
                    i_detail.AdvanceId = listChecked[i];
                    i_detail.BillingValue = listValue[i];
                    i_detail.BillingCode = billingToAdd.BillingCode;
                    list_detail.Add(i_detail);
                }

                var job_detail_code = listChecked[0];
                var item_detail = await (from a in db.TblJobDetails
                                         join b in db.TblJobadvances on a.JobDetailCode equals b.JobDetailCode
                                         join c in db.TblJobMasters on a.JobCode equals c.JobCode
                                         where b.AdvanceCode == advance_code
                                         select a).FirstOrDefaultAsync();
                var item_master = await (from a in db.TblJobDetails
                                         join b in db.TblJobadvances on a.JobDetailCode equals b.JobDetailCode
                                         join c in db.TblJobMasters on a.JobCode equals c.JobCode
                                         where b.AdvanceCode == advance_code
                                         select c).FirstOrDefaultAsync();

                var taxCustomer = await db.TblCustomers.Where(w => w.CustomerCode == item_master.CustomerCode).FirstOrDefaultAsync();
                var taxAddress = await db.TblCustomerAddresses.Where(w => w.CustomerCode == item_master.CustomerCode).FirstOrDefaultAsync();

                billingToAdd.CustomerCode = item_master.CustomerCode;
                billingToAdd.TaxId = taxCustomer.CustomerVatNo;
                billingToAdd.TaxName = taxAddress.TransportName;
                billingToAdd.TaxAddress = taxAddress.TransportAddress;

                billingToAdd.MissionPlaceCode = item_detail.MissionPlaceCode;
                billingToAdd.BillingTotal = billingToAdd.AdvanceTotal;
                billingToAdd.BillingVat = 0;
                billingToAdd.BillingNetTotal = billingToAdd.BillingTotal;
                billingToAdd.IsClearing = false;
                billingToAdd.IsEnabled = true;
                billingToAdd.BillingType = "O";
                billingToAdd.IsVat = false;
                billingToAdd.TblBillingOthers = list_detail;
                db.TblBillingMasters.Add(billingToAdd);

                await db.SaveChangesAsync();
                tr.Complete();
                response.result = true;
                response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                response.result = false;
                response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
                logger.LogError(ex, "BillingRepository operation failed.");
                response.errorException = null;
            }
        }

        return response;
    }

    public async Task<List<AdvanceViewModel>> GetListAdvance()
    {
        using (var connection = new SqlConnection(connStr))
        {
            var sql = "SELECT tbl_job_master.job_date, tbl_job_master.booking_no, tbl_job_detail.job_detail_code, " +
                "tbl_job_detail.container_no, tbl_truck.truck_license, tbl_customer.customer_acronym As customer_name, " +
                "tbl_jobadvance.advance_totalup As advance_total, tbl_place.place_name As start_name, tbl_place_1.place_name As mission_name " +
                "FROM tbl_job_master INNER JOIN " +
                "tbl_job_detail ON tbl_job_master.job_code = tbl_job_detail.job_code INNER JOIN " +
                "tbl_jobadvance ON tbl_job_detail.job_detail_code = tbl_jobadvance.job_detail_code INNER JOIN " +
                "tbl_truck ON tbl_job_detail.truck_code = tbl_truck.truck_code INNER JOIN " +
                "tbl_customer ON tbl_job_master.customer_code = tbl_customer.customer_code INNER JOIN " +
                "tbl_place ON tbl_job_detail.start_place_code = tbl_place.place_code INNER JOIN " +
                "tbl_place AS tbl_place_1 ON tbl_job_detail.mission_place_code = tbl_place_1.place_code INNER JOIN " +
                "tbl_customer_rule ON tbl_customer.customer_code = tbl_customer_rule.customer_code " +
                "WHERE (tbl_job_detail.is_approve = 1)  " +
                "AND (tbl_customer_rule.is_lift_cashing = 0 ) " +
                "AND (tbl_jobadvance.is_billing = 0)  " +
                "AND (tbl_jobadvance.is_enabled = 1)";

            var query = (await connection.QueryAsync<AdvanceViewModel>(sql)).ToList();

            return query;
        }
    }

    public async Task<ResponseViewModel> AddAdvance(TblBillingMaster billingToAdd, string[] listChecked, decimal[] listValue)
    {
        ResponseViewModel response = new ResponseViewModel();
        using (TransactionScope tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var list_detail = new List<TblBillingDetail>();
                for (int i = 0; i < listChecked.Count(); i++)
                {
                    var i_detail = new TblBillingDetail();
                    i_detail.JobDetailCode = listChecked[i];
                    i_detail.BillingValue = 0;
                    i_detail.AdvanceValue = listValue[i];
                    i_detail.BillingCode = billingToAdd.BillingCode;
                    list_detail.Add(i_detail);

                    var advance = await db.TblJobadvances.FirstOrDefaultAsync(w => w.JobDetailCode == listChecked[i]);
                    advance.IsBilling = true;
                }

                var job_detail_code = listChecked[0];
                var item_detail = await db.TblJobDetails.Where(w => w.JobDetailCode == job_detail_code).FirstOrDefaultAsync();
                var item_master = await db.TblJobMasters.Where(w => w.JobCode == item_detail.JobCode).FirstOrDefaultAsync();
                var taxCustomer = await db.TblCustomers.Where(w => w.CustomerCode == item_master.CustomerCode).FirstOrDefaultAsync();
                var taxAddress = await db.TblCustomerAddresses.Where(w => w.CustomerCode == item_master.CustomerCode).FirstOrDefaultAsync();

                billingToAdd.CustomerCode = item_master.CustomerCode;
                billingToAdd.TaxId = taxCustomer.CustomerVatNo;
                billingToAdd.TaxName = taxAddress.AdvanceName;
                billingToAdd.TaxAddress = taxAddress.AdvanceAddress;

                billingToAdd.MissionPlaceCode = item_detail.MissionPlaceCode;
                billingToAdd.BillingTotal = billingToAdd.AdvanceTotal;
                billingToAdd.BillingVat = 0;
                billingToAdd.BillingNetTotal = billingToAdd.BillingTotal;
                billingToAdd.CustomerCode = item_master.CustomerCode;
                billingToAdd.IsClearing = false;
                billingToAdd.IsEnabled = true;
                billingToAdd.BillingType = "A";
                billingToAdd.IsVat = false;
                billingToAdd.TblBillingDetails = list_detail;
                db.TblBillingMasters.Add(billingToAdd);

                db.TblJobadvances.FromSqlRaw("EXEC spUpdateJobAdvance");
                await db.SaveChangesAsync();
                tr.Complete();
                response.result = true;
                response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                response.result = false;
                response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
                logger.LogError(ex, "BillingRepository operation failed.");
                response.errorException = null;
            }
        }

        return response;
    }

    public async Task<List<AdvanceViewModel>> GetListOther()
    {
        var data = await (from a in db.TblJobMasters
                          join b in db.TblJobDetails on a.JobCode equals b.JobCode
                          join c in db.TblJobadvances on b.JobDetailCode equals c.JobDetailCode
                          join d in db.TblTrucks on b.TruckCode equals d.TruckCode
                          join e in db.TblCustomers on a.CustomerCode equals e.CustomerCode
                          join f in db.TblPlaces on b.StartPlaceCode equals f.PlaceCode
                          join g in db.TblPlaces on b.MissionPlaceCode equals g.PlaceCode
                          join h in db.TblJobadvanceOthers on c.AdvanceCode equals h.AdvanceCode
                          where b.IsApprove == true && h.IsBilling == false && c.IsEnabled == true
                          select new AdvanceViewModel
                          {
                              id = h.Id,
                              job_date = a.JobDate,
                              booking_no = a.BookingNo,
                              job_detail_code = b.JobDetailCode,
                              container_no = b.ContainerNo,
                              truck_license = d.TruckLicense,
                              customer_name = e.CustomerAcronym,
                              advance_total = c.AdvanceTotalup,
                              start_name = f.PlaceName,
                              mission_name = g.PlaceName,
                              advance_other = h.AdvanceOther,
                              advance_other_value = h.AdvanceOtherValueup
                          }).OrderBy(o => o.job_date).ToListAsync();
        return data;
    }

    public async Task<TblBillingMaster> GetBillingDataByCode(string code)
    {
        var data = await db.TblBillingMasters.Where(w => w.BillingCode == code).FirstOrDefaultAsync();
        return data;
    }

    public async Task<List<BillingViewModel>> GetListBillingByCode(string billing_code)
    {
        var data = await (from a in db.ViewBillingListActives
                          where a.BillingCode == billing_code
                          select new BillingViewModel
                          {
                              billing_code = a.BillingCode,
                              container_no = a.ContainerNo,
                              booking_no = a.BookingNo,
                              invoice_no = a.JobInvoice,
                              customer_name = a.CustomerName,
                              mission_name = a.MissionName,
                              cost_value = a.CostValue.Value,
                              cashing_total = a.AdvanceValue.Value,
                              job_detail_code = a.JobDetailCode,
                              billing_type = a.BillingType,
                              is_clearing = a.IsClearing.Value,
                              truck_license = a.TruckLicense,
                              billing_date = a.BillingDate.Value.ToString("yyyy-MM-dd"),
                              billing_date_value = a.BillingDate.Value,
                              job_date = a.JobDate,
                              job_date_value = a.JobDateValue,
                              advance_value = a.AdvanceValue.Value,
                              transport_value = a.BillingValue.Value
                          }).ToListAsync();
        return data;
    }

    public async Task<ResponseViewModel> EditTransport(TblBillingMaster billingToEdit, string[] listChecked, decimal[] listValue)
    {
        ResponseViewModel response = new ResponseViewModel();
        using (TransactionScope tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                decimal total_advance = 0;
                if (listChecked.Count() > 0)
                {
                    var detail = await db.TblBillingDetails.Where(w => w.BillingCode == billingToEdit.BillingCode).ToListAsync();
                    foreach (var item in detail)
                    {
                        var job_detail = await db.TblJobDetails.FindAsync(item.JobDetailCode);
                        job_detail.IsBilling = false;

                        var advance = await db.TblJobadvances.FirstOrDefaultAsync(w => w.JobDetailCode == item.JobDetailCode && w.IsEnabled == true);
                        advance.IsBilling = false;

                        await AddAuditAsync(
                            AuditActions.Update,
                            nameof(TblBillingDetail),
                            item.Id.ToString(),
                            "Update billing transport.",
                            billingToEdit.UpdateBy,
                            GetScalarSnapshot(item),
                            null);
                    }
                    db.TblBillingDetails.RemoveRange(detail);


                    for (int i = 0; i < listChecked.Count(); i++)
                    {
                        decimal advance_value = 0;
                        string job_detail_code = listChecked[i];
                        decimal billing_value = listValue[i];
                        if (billingToEdit.IsAdvance == true)
                        {
                            var advance = await db.TblJobadvances.FirstOrDefaultAsync(w => w.JobDetailCode == job_detail_code && w.IsEnabled == true);
                            advance.IsBilling = true;
                            advance_value = advance.AdvanceTotalup;
                            total_advance = total_advance + advance_value;
                        }

                        var db_detail = await db.TblJobDetails.FindAsync(job_detail_code);
                        db_detail.IsBilling = true;

                        var i_detail = new TblBillingDetail();
                        i_detail.JobDetailCode = job_detail_code;
                        i_detail.BillingValue = billing_value;
                        i_detail.AdvanceValue = advance_value;
                        i_detail.BillingCode = billingToEdit.BillingCode;
                        db.TblBillingDetails.Add(i_detail);
                    }
                }

                if (billingToEdit.IsAdvance != true)
                {
                    billingToEdit.BillingType = "T";
                    billingToEdit.AdvanceTotal = 0;
                    billingToEdit.BillingTotal = billingToEdit.TransportTotal + billingToEdit.AdvanceTotal;
                    billingToEdit.BillingVat = billingToEdit.IsVat == true ? (billingToEdit.TransportTotal * 1) / 100 : 0;
                    billingToEdit.BillingNetTotal = billingToEdit.TransportTotal - billingToEdit.BillingVat;
                }
                else
                {
                    billingToEdit.BillingType = "M";
                    billingToEdit.AdvanceTotal = total_advance;
                    billingToEdit.BillingTotal = billingToEdit.TransportTotal + billingToEdit.AdvanceTotal;
                    billingToEdit.BillingVat = billingToEdit.IsVat == true ? (billingToEdit.TransportTotal * 1) / 100 : 0;
                    billingToEdit.BillingNetTotal = (billingToEdit.TransportTotal - billingToEdit.BillingVat) + total_advance;
                }

                billingToEdit.IsClearing = false;
                db.TblBillingMasters.Update(billingToEdit);

                await AddAuditAsync(
                    AuditActions.Update,
                    nameof(TblBillingMaster),
                    billingToEdit.BillingCode,
                    "Update billing transport.",
                    billingToEdit.UpdateBy,
                    null,
                    null);
                await db.SaveChangesAsync();
                tr.Complete();
                response.result = true;
                response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                response.result = false;
                response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
                logger.LogError(ex, "BillingRepository operation failed.");
                response.errorException = null;
            }
        }

        return response;
    }

    public async Task<ResponseViewModel> EditTransportDataDetail(decimal value, string job_detail_code, string billing_code, string username)
    {
        ResponseViewModel response = new ResponseViewModel();
        using (TransactionScope tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var data = await db.TblJobDetails.FindAsync(job_detail_code);
                data.CostValue = value;

                var transport_billing = await db.TblBillingDetails.FirstOrDefaultAsync(w => w.JobDetailCode == data.JobDetailCode && w.BillingCode == billing_code);
                var log_advance_billing = transport_billing;
                transport_billing.BillingValue = value;

                var billing_detail = await db.TblBillingDetails.Where(w => w.BillingCode == transport_billing.BillingCode).ToListAsync();
                var total = billing_detail.Sum(s => s.BillingValue);

                var billing = await db.TblBillingMasters.FindAsync(billing_code);
                billing.TransportTotal = total;
                billing.BillingTotal = billing.AdvanceTotal + total;
                billing.BillingVat = billing.IsVat == true ? (total * 1) / 100 : 0;
                billing.BillingNetTotal = (total - billing.BillingVat) + billing.AdvanceTotal;

                if (billing.IsCoversheet == true)
                {
                    await UpdateCoverSheet(billing.BillingCode);
                }
                if (billing.IsClearing == true)
                {
                    await UpdateReceipt(billing.BillingCode);
                }

                await AddAuditAsync(
                    AuditActions.Update,
                    nameof(TblBillingDetail),
                    log_advance_billing.Id.ToString(),
                    "Update transport detail data.",
                    username,
                    null,
                    GetScalarSnapshot(log_advance_billing));
                await AddAuditAsync(
                    AuditActions.Update,
                    nameof(TblJobDetail),
                    data.JobDetailCode,
                    "Update transport detail data.",
                    username,
                    null,
                    null);
                await db.SaveChangesAsync();
                tr.Complete();
                response.result = true;
                response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                response.result = false;
                response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
                logger.LogError(ex, "BillingRepository operation failed.");
                response.errorException = null;
            }
        }

        return response;
    }

    public async Task<List<AdvanceViewModel>> GetListAdvanceOther()
    {
        var data = await (from a in db.TblJobMasters
                          join b in db.TblJobDetails on a.JobCode equals b.JobCode
                          join c in db.TblJobadvances on b.JobDetailCode equals c.JobDetailCode
                          join d in db.TblTrucks on b.TruckCode equals d.TruckCode
                          join e in db.TblCustomers on a.CustomerCode equals e.CustomerCode
                          join f in db.TblPlaces on b.StartPlaceCode equals f.PlaceCode
                          join g in db.TblPlaces on b.MissionPlaceCode equals g.PlaceCode
                          join h in db.TblJobadvanceOthers on c.AdvanceCode equals h.AdvanceCode
                          where b.IsApprove == true && c.IsEnabled == true && h.IsBilling == false
                          select new AdvanceViewModel
                          {
                              job_date = a.JobDate,
                              booking_no = a.BookingNo,
                              job_detail_code = b.JobDetailCode,
                              container_no = b.ContainerNo,
                              truck_license = d.TruckLicense,
                              customer_name = e.CustomerAcronym,
                              advance_total = h.AdvanceOtherValueup,
                              start_name = f.PlaceName,
                              mission_name = g.PlaceName,
                              advance_other = h.AdvanceOther,
                              id = h.Id
                          }).OrderBy(o => o.job_date).ToListAsync();
        return data;
    }

    public async Task<List<AdvanceViewModel>> GetListAdvanceOtherByBillingCode(string code)
    {
        var data = await (from a in db.TblJobMasters
                          join b in db.TblJobDetails on a.JobCode equals b.JobCode
                          join c in db.TblJobadvances on b.JobDetailCode equals c.JobDetailCode
                          join d in db.TblTrucks on b.TruckCode equals d.TruckCode
                          join e in db.TblCustomers on a.CustomerCode equals e.CustomerCode
                          join f in db.TblPlaces on b.StartPlaceCode equals f.PlaceCode
                          join g in db.TblPlaces on b.MissionPlaceCode equals g.PlaceCode
                          join h in db.TblJobadvanceOthers on c.AdvanceCode equals h.AdvanceCode
                          join i in db.TblBillingOthers on h.Id equals i.AdvanceId
                          where i.BillingCode == code
                          select new AdvanceViewModel
                          {
                              job_date = a.JobDate,
                              booking_no = a.BookingNo,
                              job_detail_code = b.JobDetailCode,
                              container_no = b.ContainerNo,
                              truck_license = d.TruckLicense,
                              customer_name = e.CustomerAcronym,
                              advance_total = h.AdvanceOtherValueup,
                              start_name = f.PlaceName,
                              mission_name = g.PlaceName,
                              advance_other = h.AdvanceOther,
                              id = h.Id
                          }).OrderBy(o => o.job_date).ToListAsync();
        return data;
    }

    public async Task<List<AdvanceViewModel>> GetListAdvanceByBillingCode(string code)
    {
        var data = await (from a in db.TblJobMasters
                          join b in db.TblJobDetails on a.JobCode equals b.JobCode
                          join c in db.TblJobadvances on b.JobDetailCode equals c.JobDetailCode
                          join d in db.TblTrucks on b.TruckCode equals d.TruckCode
                          join e in db.TblCustomers on a.CustomerCode equals e.CustomerCode
                          join f in db.TblPlaces on b.StartPlaceCode equals f.PlaceCode
                          join g in db.TblPlaces on b.MissionPlaceCode equals g.PlaceCode
                          join h in db.TblBillingDetails on b.JobDetailCode equals h.JobDetailCode
                          where h.BillingCode == code
                          select new AdvanceViewModel
                          {
                              job_date = a.JobDate,
                              booking_no = a.BookingNo,
                              job_detail_code = b.JobDetailCode,
                              container_no = b.ContainerNo,
                              truck_license = d.TruckLicense,
                              customer_name = e.CustomerAcronym,
                              advance_total = c.AdvanceTotalup,
                              start_name = f.PlaceName,
                              mission_name = g.PlaceName
                          }).OrderBy(o => o.job_date).ToListAsync();
        return data;
    }

    public async Task<ResponseViewModel> EditAdvance(TblBillingMaster billingToEdit, string[] listChecked, decimal[] listValue)
    {
        ResponseViewModel response = new ResponseViewModel();
        using (TransactionScope tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                if (listChecked.Count() > 0)
                {
                    var detail = await db.TblBillingDetails.Where(w => w.BillingCode == billingToEdit.BillingCode).ToListAsync();
                    foreach (var item in detail)
                    {
                        var advance = await db.TblJobadvances.FirstOrDefaultAsync(w => w.JobDetailCode == item.JobDetailCode && w.IsEnabled == true);
                        advance.IsBilling = false;

                        await AddAuditAsync(
                            AuditActions.Update,
                            nameof(TblBillingDetail),
                            item.Id.ToString(),
                            "Update billing advance.",
                            billingToEdit.UpdateBy,
                            GetScalarSnapshot(item),
                            null);
                    }
                    db.TblBillingDetails.RemoveRange(detail);

                    for (int i = 0; i < listChecked.Count(); i++)
                    {
                        string job_detail_code = listChecked[i];
                        decimal billing_value = listValue[i];
                        var advance = await db.TblJobadvances.FirstOrDefaultAsync(w => w.JobDetailCode == job_detail_code && w.IsEnabled == true);
                        var other = await db.TblJobadvanceOthers.Where(w => w.AdvanceCode == advance.AdvanceCode).ToListAsync();
                        decimal oth = other.Sum(s => s.AdvanceOtherValue);
                        advance.IsBilling = true;
                        advance.AdvanceTotal = advance.AdvanceTotalup + oth;

                        var i_detail = new TblBillingDetail();
                        i_detail.JobDetailCode = job_detail_code;
                        i_detail.BillingValue = 0;
                        i_detail.AdvanceValue = billing_value;
                        i_detail.BillingCode = billingToEdit.BillingCode;
                        db.TblBillingDetails.Add(i_detail);
                    }
                }

                billingToEdit.BillingTotal = billingToEdit.AdvanceTotal;
                billingToEdit.BillingVat = 0;
                billingToEdit.BillingNetTotal = billingToEdit.BillingTotal;
                billingToEdit.IsClearing = false;
                billingToEdit.IsVat = false;
                billingToEdit.BillingType = "A";
                db.TblBillingMasters.Update(billingToEdit);

                await AddAuditAsync(
                    AuditActions.Update,
                    nameof(TblBillingMaster),
                    billingToEdit.BillingCode,
                    "Update billing advance.",
                    billingToEdit.UpdateBy,
                    null,
                    null);
                await db.SaveChangesAsync();
                tr.Complete();
                response.result = true;
                response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                response.result = false;
                response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
                logger.LogError(ex, "BillingRepository operation failed.");
                response.errorException = null;
            }
        }

        return response;
    }

    public async Task<ResponseViewModel> EditAdvanceDataDetail(TblJobadvance model, string billing_code)
    {
        ResponseViewModel response = new ResponseViewModel();
        using (TransactionScope tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var data = await db.TblJobadvances.FindAsync(model.AdvanceCode);
                data.AdvanceLiftonup = model.AdvanceLifton;
                data.AdvanceLiftoffup = model.AdvanceLiftoff;
                data.AdvanceYardup = model.AdvanceYard;
                data.AdvancePortup = model.AdvancePort;
                data.AdvanceTotalup = data.AdvanceLiftonup + data.AdvanceLiftoffup + data.AdvanceYardup + data.AdvancePortup;

                var advance_billing = await db.TblBillingDetails.FirstOrDefaultAsync(w => w.JobDetailCode == data.JobDetailCode && w.BillingCode == billing_code);
                var log_advance_billing = advance_billing;
                advance_billing.AdvanceValue = data.AdvanceTotalup;

                var billing_detail = await db.TblBillingDetails.Where(w => w.BillingCode == advance_billing.BillingCode).ToListAsync();
                var total = billing_detail.Sum(s => s.AdvanceValue);

                var billing = await db.TblBillingMasters.FindAsync(billing_code);
                billing.AdvanceTotal = total;
                billing.BillingTotal = billing.TransportTotal + total;
                billing.BillingVat = billing.IsVat == true ? (billing.TransportTotal * 1) / 100 : 0;
                billing.BillingNetTotal = (billing.TransportTotal - billing.BillingVat) + total;
                await db.SaveChangesAsync();

                if (billing.IsCoversheet == true)
                {
                    await UpdateCoverSheet(billing.BillingCode);
                }
                if (billing.IsClearing == true)
                {
                    await UpdateReceipt(billing.BillingCode);
                }

                await AddAuditAsync(
                    AuditActions.Update,
                    nameof(TblBillingDetail),
                    log_advance_billing.Id.ToString(),
                    "Update advance detail data.",
                    model.UpdateBy,
                    null,
                    GetScalarSnapshot(log_advance_billing));
                await AddAuditAsync(
                    AuditActions.Update,
                    nameof(TblJobadvance),
                    data.JobDetailCode,
                    "Update advance detail data.",
                    model.UpdateBy,
                    null,
                    null);
                await db.SaveChangesAsync();
                tr.Complete();
                response.result = true;
                response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                response.result = false;
                response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
                logger.LogError(ex, "BillingRepository operation failed.");
                response.errorException = null;
            }
        }

        return response;
    }

    public async Task<ResponseViewModel> EditAdvanceOther(TblBillingMaster billingToEdit, int[] listChecked, decimal[] listValue)
    {
        ResponseViewModel response = new ResponseViewModel();
        using (TransactionScope tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                if (listChecked.Count() > 0)
                {
                    var detail = await db.TblBillingOthers.Where(w => w.BillingCode == billingToEdit.BillingCode).ToListAsync();
                    foreach (var item in detail)
                    {
                        var db_advance = await db.TblJobadvanceOthers.FindAsync(item.AdvanceId);
                        db_advance.IsBilling = false;

                        await AddAuditAsync(
                            AuditActions.Update,
                            nameof(TblBillingOther),
                            $"{item.BillingCode}:{item.AdvanceId}",
                            "Update advance other data.",
                            billingToEdit.UpdateBy,
                            GetScalarSnapshot(item),
                            null);
                    }
                    db.TblBillingOthers.RemoveRange(detail);

                    for (int i = 0; i < listChecked.Count(); i++)
                    {
                        int advance_id = listChecked[i];
                        decimal billing_value = listValue[i];

                        var job_advance = await db.TblJobadvanceOthers.FindAsync(advance_id);
                        job_advance.IsBilling = true;

                        var i_other = new TblBillingOther();
                        i_other.AdvanceCode = job_advance.AdvanceCode;
                        i_other.AdvanceId = advance_id;
                        i_other.BillingValue = billing_value;
                        i_other.BillingCode = billingToEdit.BillingCode;
                        db.TblBillingOthers.Add(i_other);
                    }
                }

                billingToEdit.BillingTotal = billingToEdit.AdvanceTotal;
                billingToEdit.IsVat = false;
                billingToEdit.BillingVat = 0;
                billingToEdit.BillingNetTotal = billingToEdit.BillingTotal;
                billingToEdit.IsClearing = false;
                billingToEdit.BillingType = "O";
                db.TblBillingMasters.Update(billingToEdit);

                await AddAuditAsync(
                    AuditActions.Update,
                    nameof(TblBillingMaster),
                    billingToEdit.BillingCode,
                    "Update advance other data.",
                    billingToEdit.UpdateBy,
                    null,
                    null);
                await db.SaveChangesAsync();
                tr.Complete();
                response.result = true;
                response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                response.result = false;
                response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
                logger.LogError(ex, "BillingRepository operation failed.");
                response.errorException = null;
            }
        }

        return response;
    }

    public async Task<List<BillingViewModel>> GetListBillingOtherByCode(string billing_code)
    {
        var data = await (from a in db.ViewBillingOtherListActives
                          where a.BillingCode == billing_code
                          select new BillingViewModel
                          {
                              billing_code = a.BillingCode,
                              container_no = a.ContainerNo,
                              booking_no = a.BookingNo,
                              customer_name = a.CustomerNameTh,
                              mission_name = a.MissionName,
                              cost_value = a.CostValue.Value,
                              job_detail_code = a.JobDetailCode,
                              billing_type = a.BillingType,
                              is_clearing = a.IsClearing.Value,
                              truck_license = a.TruckLicense,
                              billing_date = a.BillingDate.Value.ToString("yyyy-MM-dd"),
                              billing_date_value = a.BillingDate.Value,
                              job_date = a.JobDate,
                              job_date_value = a.JobDateValue,
                              advance_value = a.AdvanceOtherValueup.Value,
                              advance_other = a.AdvanceOther,
                              id = a.Id.Value
                          }).ToListAsync();
        return data;
    }

    public async Task<ResponseViewModel> RemoveBilling(string billing_code, string username)
    {
        ResponseViewModel response = new ResponseViewModel();
        using (TransactionScope tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var bill = await db.TblBillingMasters.Where(w => w.BillingCode == billing_code).FirstOrDefaultAsync();
                if (bill.BillingType != "O")
                {
                    var list = await db.TblBillingDetails.Where(w => w.BillingCode == billing_code).ToListAsync();
                    foreach (var item in list)
                    {
                        var advance = await db.TblJobadvances.FirstOrDefaultAsync(w => w.JobDetailCode == item.JobDetailCode);
                        if (advance != null)
                        {
                            advance.IsBilling = false;
                        }

                        var job_detail = await db.TblJobDetails.FindAsync(item.JobDetailCode);
                        job_detail.IsBilling = false;

                        await AddAuditAsync(
                            AuditActions.Delete,
                            nameof(TblBillingDetail),
                            item.Id.ToString(),
                            "Remove billing.",
                            username,
                            GetScalarSnapshot(item),
                            null);
                    }

                    db.TblBillingDetails.RemoveRange(list);
                }
                else
                {
                    var list = await db.TblBillingOthers.Where(w => w.BillingCode == billing_code).ToListAsync();
                    foreach (var item in list)
                    {
                        var i_other = await db.TblJobadvanceOthers.FindAsync(item.AdvanceId);
                        i_other.IsBilling = false;

                        await AddAuditAsync(
                            AuditActions.Delete,
                            nameof(TblBillingOther),
                            $"{item.BillingCode}:{item.AdvanceId}",
                            "Remove billing.",
                            username,
                            GetScalarSnapshot(item),
                            null);
                    }

                    db.TblBillingOthers.RemoveRange(list);
                }

                bill.IsEnabled = false;
                bill.UpdateDate = DateTime.Now;
                bill.UpdateBy = username;

                await AddAuditAsync(
                    AuditActions.Delete,
                    nameof(TblBillingMaster),
                    billing_code,
                    "Remove billing.",
                    username,
                    null,
                    null);
                await db.SaveChangesAsync();
                tr.Complete();
                response.result = true;
                response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                response.result = false;
                response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
                logger.LogError(ex, "BillingRepository operation failed.");
                response.errorException = null;
            }
        }

        return response;
    }

    public async Task<ResponseViewModel> EditBookingData(string value, string job_detail_code)
    {
        ResponseViewModel response = new ResponseViewModel();
        try
        {
            var detail = await db.TblJobDetails.Where(w => w.JobDetailCode == job_detail_code).FirstOrDefaultAsync();
            var data = await db.TblJobMasters.FindAsync(detail.JobCode);
            data.BookingNo = value;

            await db.SaveChangesAsync();
            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            logger.LogError(ex, "BillingRepository operation failed.");
            response.errorException = null;
        }

        return response;
    }

    public async Task<ResponseViewModel> EditInvoiceData(string value, string job_detail_code)
    {
        ResponseViewModel response = new ResponseViewModel();
        try
        {
            var detail = await db.TblJobDetails.Where(w => w.JobDetailCode == job_detail_code).FirstOrDefaultAsync();
            var data = await db.TblJobMasters.FindAsync(detail.JobCode);
            data.JobInvoice = value;

            await db.SaveChangesAsync();
            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            logger.LogError(ex, "BillingRepository operation failed.");
            response.errorException = null;
        }

        return response;
    }

    public async Task<ResponseViewModel> EditContainerData(string value, string job_detail_code)
    {
        ResponseViewModel response = new ResponseViewModel();
        try
        {
            var data = await db.TblJobDetails.FindAsync(job_detail_code);
            data.ContainerNo = value;

            await db.SaveChangesAsync();
            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            logger.LogError(ex, "BillingRepository operation failed.");
            response.errorException = null;
        }

        return response;
    }

    public async Task<ResponseViewModel> EditOtherDataDetail(int id, decimal value, string username)
    {
        ResponseViewModel response = new ResponseViewModel();
        using (TransactionScope tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var other = await db.TblJobadvanceOthers.FindAsync(id);
                other.AdvanceOtherValueup = value;

                var billing_other = await db.TblBillingOthers.FirstOrDefaultAsync(w => w.AdvanceId == id);
                billing_other.BillingValue = value;

                await db.SaveChangesAsync();

                decimal total = await db.TblBillingOthers.Where(w => w.BillingCode == billing_other.BillingCode).SumAsync(s => s.BillingValue);
                var billing = await db.TblBillingMasters.FindAsync(billing_other.BillingCode);
                billing.AdvanceTotal = total;
                billing.BillingTotal = total;
                billing.BillingNetTotal = total;

                if (billing.IsCoversheet == true)
                {
                    await UpdateCoverSheet(billing.BillingCode);
                }
                if (billing.IsClearing == true)
                {
                    await UpdateReceipt(billing.BillingCode);
                }

                await AddAuditAsync(
                    AuditActions.Update,
                    nameof(TblBillingOther),
                    id.ToString(),
                    "Update advance other detail data.",
                    username,
                    null,
                    null);
                await db.SaveChangesAsync();
                tr.Complete();
                response.result = true;
                response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                response.result = false;
                response.resultMessage = "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
                logger.LogError(ex, "BillingRepository operation failed.");
                response.errorException = null;
            }
        }

        return response;
    }

    public async Task<List<BillingViewModel>> GetListAllBillingData(string month, string year)
    {
        var data = await (from a in db.TblBillingMasters
                          join b in db.TblCustomers on a.CustomerCode equals b.CustomerCode
                          join d in db.TblPlaces on a.MissionPlaceCode equals d.PlaceCode
                          where a.IsEnabled == true &&
                                  a.BillingDate.Month == int.Parse(month) &&
                                  a.BillingDate.Year == int.Parse(year)
                          select new BillingViewModel
                          {
                              billing_code = a.BillingCode,
                              billing_total = a.BillingNetTotal,
                              billing_amount = a.BillingAmount,
                              customer_code = a.CustomerCode,
                              customer_name = b.CustomerAcronym,
                              is_clearing = a.IsClearing,
                              billing_type = a.BillingType,
                              mission_name = d.PlaceName,
                              billing_date = a.BillingDate.ToString("yyyy-MM-dd"),
                              billing_date_value = a.BillingDate,
                              clearing_date = a.ClearingDate == null ? "-" : a.ClearingDate.Value.ToString("yyyy-MM-dd")
                          }).ToListAsync();
        return data;
    }

    private async Task UpdateBillingByReceipt(string receipt_code)
    {
        var receipt_detail = await db.TblReceiptDetails.Where(w => w.ReceiptCode == receipt_code).ToListAsync();
        foreach (var item in receipt_detail)
        {
            var billing = await db.TblBillingDetails.Where(w => w.BillingCode == item.BillingCode).ToListAsync();
            var total_advance = billing.Sum(s => s.AdvanceValue);
            var total_transport = billing.Sum(s => s.BillingValue);
        }
    }

    private async Task UpdateCoverSheet(string billing_code)
    {
        var billing = await db.TblBillingMasters.Where(w => w.BillingCode == billing_code).FirstOrDefaultAsync();

        var data = await db.TblCoversheetDetails.FirstOrDefaultAsync(w => w.BillingCode == billing_code);
        data.CoversheetValue = billing.BillingNetTotal;

        var total_coversheet = await db.TblCoversheetDetails.Where(w => w.CoversheetCode == data.CoversheetCode).SumAsync(s => s.CoversheetValue);
        var model = await db.TblCoversheets.FindAsync(data.CoversheetCode);
        model.CoversheetTotal = total_coversheet;
    }

    private async Task UpdateReceipt(string billing_code)
    {
        var billing = await db.TblBillingMasters.Where(w => w.BillingCode == billing_code).FirstOrDefaultAsync();

        var data = await db.TblReceiptDetails.FirstOrDefaultAsync(w => w.BillingCode == billing_code);
        data.ReceiptTransportValue = billing.TransportTotal;
        data.ReceiptAdvanceValue = billing.AdvanceTotal;
        data.ReceiptValue = data.ReceiptAdvanceValue + data.ReceiptTransportValue;

        await db.SaveChangesAsync();

        var transport_receipt = await db.TblReceiptDetails.Where(w => w.ReceiptCode == data.ReceiptCode).SumAsync(s => s.ReceiptTransportValue);
        var advance_receipt = await db.TblReceiptDetails.Where(w => w.ReceiptCode == data.ReceiptCode).SumAsync(s => s.ReceiptAdvanceValue);
        //var total_receipt = db.TblReceiptDetail.Where(w => w.ReceiptCode == data.ReceiptCode).Sum(s => s.ReceiptValue);
        var model = await db.TblReceipts.FindAsync(data.ReceiptCode);
        var vat = (transport_receipt * 1) / 100;
        model.ReceiptTransport = transport_receipt;
        model.ReceiptAdvance = advance_receipt;
        model.ReceiptTotal = transport_receipt + advance_receipt;
        model.ReceiptNetTotal = (transport_receipt - vat) + advance_receipt;
    }

    private async Task AddOtherInvoice(TblBillingMaster model, List<TblBillingDetail> list)
    {
        var billing = new TblBillingMaster();
        var list_other = new List<TblBillingOther>();
        decimal total_other = 0;
        foreach (var item in list)
        {
            var advance = await db.TblJobadvances.Where(w => w.JobDetailCode == item.JobDetailCode && w.IsEnabled == true).FirstOrDefaultAsync();
            var item_advance = await db.TblJobadvances.FindAsync(advance.AdvanceCode);
            var item_other = await db.TblJobadvanceOthers.Where(w => w.AdvanceCode == advance.AdvanceCode).ToListAsync();
            foreach (var res in item_other)
            {
                var other = new TblBillingOther();
                var advance_other = await db.TblJobadvanceOthers.FindAsync(res.Id);
                advance_other.IsBilling = true;

                other.BillingValue = res.AdvanceOtherValueup;
                other.BillingCode = other.BillingCode;
                other.AdvanceCode = advance.AdvanceCode;
                other.AdvanceId = res.Id;
                list_other.Add(other);
            }
        }

        total_other = list_other.Sum(s => s.BillingValue);
        billing.BillingCode = await codeRepo.GetBillingCode();
        var taxCustomer = await db.TblCustomers.Where(w => w.CustomerCode == model.CustomerCode).FirstOrDefaultAsync();
        var taxAddress = await db.TblCustomerAddresses.Where(w => w.CustomerCode == model.CustomerCode).FirstOrDefaultAsync();

        billing.TaxId = taxCustomer.CustomerVatNo;
        billing.TaxName = taxAddress.TransportName;
        billing.TaxAddress = taxAddress.TransportAddress;
        billing.BillingType = "O";
        billing.TransportTotal = 0;
        billing.BillingVat = 0;
        billing.AdvanceTotal = total_other;
        billing.BillingTotal = total_other;
        billing.BillingNetTotal = total_other - billing.BillingVat;
        billing.MissionPlaceCode = model.MissionPlaceCode;
        billing.CustomerCode = model.CustomerCode;
        billing.BillingRemark = model.BillingRemark;
        billing.ClearingDate = null;
        billing.IsVat = false;
        billing.IsAdvance = true;
        billing.IsClearing = false;
        billing.IsCoversheet = false;
        billing.IsEnabled = true;
        billing.BillingDate = model.BillingDate;
        billing.CreateDate = DateTime.Now;
        billing.CreateBy = model.CreateBy;
        billing.UpdateDate = DateTime.Now;

        billing.UpdateBy = model.UpdateBy;
        billing.BillingAmount = list_other.Count;
        billing.TblBillingOthers = list_other;
        db.TblBillingMasters.Add(billing);
    }

    public async Task<List<CustomerBillingViewModel>> GetListCustomer(string year)
    {
        using (var connection = new SqlConnection(connStr))
        {
            await connection.OpenAsync();
            string sql = "SELECT a.customer_code, dbo.tbl_customer.customer_name_th As customer_name " +
                        "FROM(SELECT customer_code " +
                        "FROM  dbo.tbl_billing_master " +
                        "WHERE year(create_date) = @year AND is_enabled = 1 AND is_clearing = 0 " +
                        "GROUP BY customer_code) AS a INNER JOIN " +
                        "dbo.tbl_customer ON a.customer_code = dbo.tbl_customer.customer_code ";

            var data = (await connection.QueryAsync<CustomerBillingViewModel>(sql, new { year })).ToList();

            return data;
        }
    }

    public async Task<List<PendingCountViewModel>> GetListPending(string customer_code, string year)
    {
        var list = new List<PendingCountViewModel>();
        using (var connection = new SqlConnection(connStr))
        {
            await connection.OpenAsync();
            string sql = "SELECT m.MONTH As month, " +
                        "CASE WHEN p.count_pending IS NOT NULL THEN p.count_pending ELSE 0 END As count_pending " +
                        "FROM(SELECT '01' AS MONTH " +
                        "UNION " +
                          "SELECT '02' AS MONTH " +
                          "UNION " +
                          "SELECT '03' AS MONTH " +
                          "UNION " +
                          "SELECT '04' AS MONTH " +
                          "UNION " +
                          "SELECT '05' AS MONTH " +
                          "UNION " +
                          "SELECT '06' AS MONTH " +
                          "UNION " +
                          "SELECT '07' AS MONTH " +
                          "UNION " +
                          "SELECT '08' AS MONTH " +
                          "UNION " +
                          "SELECT '09' AS MONTH " +
                          "UNION " +
                          "SELECT '10' AS MONTH " +
                          "UNION " +
                          "SELECT '11' AS MONTH " +
                          "UNION " +
                          "SELECT '12' AS MONTH) AS m LEFT OUTER JOIN " +
                          "(SELECT COUNT(billing_code) AS count_pending, MONTH(billing_date) AS month_date " +
                          "FROM dbo.tbl_billing_master " +
                          "WHERE(YEAR(billing_date) = @year) AND (is_clearing = 0) AND (customer_code = @customer_code) " +
                          "AND (is_enabled = 1) " +
                          "GROUP BY MONTH(billing_date)) AS p ON m.MONTH = p.month_date";

            var data = (await connection.QueryAsync<PendingCountViewModel>(sql, new { customer_code, year })).ToList();

            return data;
        }
    }

    public async Task<List<BillingViewModel>> GetListPendingDataByCustomer(string month, string year, string code)
    {
        var data = await (from a in db.TblBillingMasters
                          join b in db.TblCustomers on a.CustomerCode equals b.CustomerCode
                          where a.IsEnabled == true &&
                                  a.IsClearing == false &&
                                  a.CustomerCode == code &&
                                  a.BillingDate.Month == int.Parse(month) &&
                                  a.BillingDate.Year == int.Parse(year)
                          select new BillingViewModel
                          {
                              billing_code = a.BillingCode,
                              billing_type = a.BillingType,
                              customer_name = b.CustomerAcronym,
                              billing_amount = a.BillingAmount,
                              transport_value = a.TransportTotal,
                              cashing_total = a.AdvanceTotal,
                              billing_total = a.BillingTotal,
                              billing_date = a.BillingDate.ToString("yyyy-MM-dd")
                          }).ToListAsync();
        return data;
    }


    private async Task AddAuditAsync(
        string actionName,
        string entityName,
        string entityId,
        string description,
        string username,
        object? oldValues,
        object? newValues)
    {
        await auditLogService.AddAsync(
            new AuditLogEntry
            {
                ModuleName = "Billing",
                ActionName = actionName,
                EntityName = entityName,
                EntityId = entityId,
                Description = string.IsNullOrWhiteSpace(username)
                    ? description
                    : $"{description} โดย {username}",
                OldValues = oldValues,
                NewValues = newValues,
                Result = AuditResults.Success
            });
    }

    private Dictionary<string, object?> GetScalarSnapshot(object entity)
    {
        return db.Entry(entity)
            .Properties
            .ToDictionary(
                p => p.Metadata.Name,
                p => p.CurrentValue);
    }


    /*

    public List<TblBillingDetail> GetBillingDetailByCode(string code)
    {
        var data = db.TblBillingDetail.Where(w => w.BillingCode == code).ToList();
        return data;
    }


    public bool GetMoney(string billing_code, string username)
    {
        try
        {
            var bill = db.TblBillingMaster.Find(billing_code);
            bill.IsClearing = true;
            bill.ClearingDate = DateTime.Now.Date;
            bill.UpdateDate = DateTime.Now;
            bill.UpdateBy = username;

            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    
    private void AddOther(TblBillingMaster model, List<TblBillingDetail> list)
    {
        var cd = new CodeRepository(db);
        var billing = new TblBillingMaster();
        var list_other = new List<TblBillingOther>();
        decimal total_other = 0;
        foreach (var item in list)
        {
            var advance = db.TblJobadvance.Where(w => w.JobDetailCode == item.JobDetailCode && w.IsEnabled == true).FirstOrDefault();
            var item_advance = db.TblJobadvance.Find(advance.AdvanceCode);
            var item_other = db.TblJobadvanceOther.Where(w => w.AdvanceCode == advance.AdvanceCode).ToList();
            foreach (var res in item_other)
            {
                var other = new TblBillingOther();
                var advance_other = db.TblJobadvanceOther.Find(res.Id);
                advance_other.IsBilling = true;

                other.BillingValue = res.AdvanceOtherValue;
                other.BillingCode = other.BillingCode;
                other.AdvanceCode = advance.AdvanceCode;
                other.AdvanceId = res.Id;
                list_other.Add(other);
            }
        }

        total_other = list_other.Sum(s => s.BillingValue);
        billing.BillingCode = cd.GetBillingCode();
        billing.SupplyerCode = model.SupplyerCode;
        billing.BillingType = "O";
        billing.TransportTotal = 0;
        billing.BillingVat = (total_other * 1) / 100;
        billing.AdvanceTotal = total_other;
        billing.BillingTotal = total_other;
        billing.BillingNetTotal = total_other - billing.BillingVat;
        billing.MissionPlaceCode = model.MissionPlaceCode;
        billing.CustomerCode = model.CustomerCode;
        billing.EmpCode = model.EmpCode;
        billing.BillingRemark = model.BillingRemark;
        billing.ClearingDate = null;
        billing.IsVat = true;
        billing.IsAdvance = true;
        billing.IsClearing = false;
        billing.IsCoversheet = false;
        billing.IsEnabled = true;
        billing.BillingDate = model.BillingDate;
        billing.CreateDate = DateTime.Now;
        billing.CreateBy = model.CreateBy;
        billing.UpdateDate = DateTime.Now;

        billing.UpdateBy = model.UpdateBy;
        billing.BillingAmount = list_other.Count;
        billing.TblBillingOther = list_other;
        db.TblBillingMaster.Add(billing);
    }

    */
}