using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;
public class VatInvoiceRepository
{
    private readonly DbavalogContext db;

    public VatInvoiceRepository(DbavalogContext context)
    {
        db = context;
    }

    public async Task<List<VatInvoiceViewModel>> GetInvoiceList(string month, string year)
    {
        var m = int.Parse(month);
        var y = int.Parse(year);

        var startDate = new DateOnly(y, m, 1);
        var endDate = startDate.AddMonths(1);

        // ใช้ช่วงวันที่แทน .Month/.Year เพื่อให้ SQL ใช้ index ได้ดีกว่า
        // และดึงวันที่จริงออกมาก่อน แล้วค่อย format ฝั่ง memory
        var rawData = await (
            from a in db.TblWithholdingTaxes.AsNoTracking()
            join b in db.TblSuppliers.AsNoTracking()
                on a.SupplierCode equals b.SupplierCode
            join c in db.TblPaymentTypes.AsNoTracking()
                on a.PaymentTypeCode equals c.PaymentTypeCode
            where a.WithholdingTaxDate >= startDate
                  && a.WithholdingTaxDate < endDate
                  && a.IsEnabled == true
            select new
            {
                a.WithholdingTaxCode,
                a.WithholdingTaxDate,
                c.PaymentTypeName,
                b.SupplierName,
                a.PaymentNetTotal,
                b.FleetTypeCode
            }
        ).ToListAsync();

        return rawData
            .Select(a => new VatInvoiceViewModel
            {
                code = a.WithholdingTaxCode,
                create_date = a.WithholdingTaxDate.ToString("dd-MM-yyyy"),
                payment_name = a.PaymentTypeName,
                supplier_name = a.SupplierName,
                net_total = a.PaymentNetTotal,
                fleet_type = a.FleetTypeCode
            })
            .ToList();
    }

    public async Task<TblVatinvoice> GetVatInvoiceByCode(string code)
    {
        return await db.TblVatinvoices
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.VatinvoiceCode == code);
    }

    public async Task<ResponseViewModel> Add(TblVatinvoice model)
    {
        var response = new ResponseViewModel();

        try
        {
            db.TblVatinvoices.Add(model);
            await db.SaveChangesAsync();

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> Edit(TblVatinvoice model)
    {
        var response = new ResponseViewModel();

        try
        {
            db.TblVatinvoices.Update(model);
            await db.SaveChangesAsync();

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> Remove(string code, string username)
    {
        var response = new ResponseViewModel();

        try
        {
            var affected = await db.TblVatinvoices
                .Where(w => w.VatinvoiceCode == code)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.IsEnabled, false)
                    .SetProperty(x => x.UpdateBy, username)
                    .SetProperty(x => x.UpdateDate, DateTime.Now));

            if (affected == 0)
            {
                response.result = false;
                response.resultMessage = "ไม่พบข้อมูลที่ต้องการลบ";
                return response;
            }

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }
}
