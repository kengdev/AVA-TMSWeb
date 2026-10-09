using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Models;
using TMSWeb.Models.ViewModels;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;

namespace TMSWeb.Controllers;

[Authorize(Roles = "Administrator, Accounting, Super")]
public class VatInvoiceController : Controller
{
    private readonly DropListRepository dropListRepo;
    private readonly VatInvoiceRepository vatInvoiceRepo;
    private readonly SupplierRepository supplierRepo;
    private string LoggedInUser => User.Identity.Name;

    public VatInvoiceController(DbavalogContext context,
        DropListRepository _dropListRepo,
        VatInvoiceRepository _vatInvoiceRepo,
        SupplierRepository _supplierRepo)
    {
        dropListRepo = _dropListRepo;
        vatInvoiceRepo = _vatInvoiceRepo;
        supplierRepo = _supplierRepo;
    }

    public IActionResult Index()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    public async Task<JsonResult> GetInvoiceDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await vatInvoiceRepo.GetInvoiceList(iFilter.month, iFilter.year);
        var data = query.Where(w => (iFilter.filterNameTH == null || w.supplier_name.ToUpper().Contains(iFilter.filterNameTH.ToUpper())) &&
                                    (iFilter.filterNameEN == null || w.payment_name.ToUpper().Contains(iFilter.filterNameEN.ToUpper())));

        Func<VatInvoiceViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.code :
                                                                    param.iSortCol_0 == 2 ? c.create_date :
                                                                    param.iSortCol_0 == 3 ? c.supplier_name : c.code);

        IEnumerable<VatInvoiceViewModel> listQuery;
        if (param.sSortDir_0 == "asc")
        {
            listQuery = data.OrderBy(orderingFunction)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength);
        }
        else
        {
            listQuery = data.OrderByDescending(orderingFunction)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength);
        }

        return Json(new
        {
            sEcho = param.sEcho,
            iTotalRecords = data.Count(),
            iTotalDisplayRecords = data.Count(),
            aaData = listQuery
        });
    }

    public async Task<IActionResult> Add()
    {
        ViewData["VatinvoiceType"] = from a in await dropListRepo.GetPaymentType() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        return View();
    }

    public async Task<IActionResult> GetSupplierByCode(string code)
    {
        var data = await supplierRepo.GetSupplierData(code);

        return Json(data);
    }

    [HttpPost]
    public async Task<IActionResult> Add(TblVatinvoice model)
    {
        model.TruckCode = "T000000";
        model.PaymentNetTotal = model.PaymentTotal;
        model.Remark = model.Remark != null ? model.Remark : "-";
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = LoggedInUser;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = LoggedInUser;

        ResponseViewModel result = await vatInvoiceRepo.Add(model);
        return Json(result);
    }

    public async Task<IActionResult> Edit(string code)
    {
        var model = await vatInvoiceRepo.GetVatInvoiceByCode(code);
        var supplier = await supplierRepo.GetSupplierData(model.SupplierCode);

        ViewBag.Supplier = supplier;
        ViewData["VatinvoiceType"] = from a in await dropListRepo.GetPaymentType() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == model.VatinvoiceType };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TblVatinvoice model)
    {
        model.Remark = model.Remark != null ? model.Remark : "-";
        model.PaymentNetTotal = model.PaymentTotal;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = LoggedInUser;

        ResponseViewModel result = await vatInvoiceRepo.Edit(model);
        return Json(result);
    }

    public async Task<IActionResult> Remove(string code)
    {
        ResponseViewModel result = await vatInvoiceRepo.Remove(code, this.LoggedInUser);

        return Json(result);
    }
}