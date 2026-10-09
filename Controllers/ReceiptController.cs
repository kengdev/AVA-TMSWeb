using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;

[Authorize(Roles = "Administrator, Accounting, Super")]
public class ReceiptController : Controller
{
    private IWebHostEnvironment hostingEnvironment;
    private readonly ReceiptRepository receiptRepo;
    private readonly DropListRepository dropListRepo;
    private readonly CodeRepository codeRepo;
    private readonly FormRepository formRepo;
    private string LoggedInUser => User.Identity.Name;

    public ReceiptController(CodeRepository _codeRepo,
        ReceiptRepository _receiptRepo,
        FormRepository _formRepo,
        DropListRepository _dropListRepo,
        IWebHostEnvironment hostingEnvironment)
    {
        codeRepo = _codeRepo;
        receiptRepo = _receiptRepo;
        dropListRepo = _dropListRepo;
        this.hostingEnvironment = hostingEnvironment;
    }

    public IActionResult Index()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    public async Task<IActionResult> Add()
    {
        var data = await receiptRepo.GetBillingDataList();
        ViewBag.Data = data;
        ViewData["ReceiptType"] = from a in dropListRepo.GetReceptType() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewData["BankName"] = from a in await dropListRepo.GetBank() select new SelectListItem { Value = a.label.ToString(), Text = a.label };
        return View();
    }

    public IActionResult _EntryReceiptDetail(string code)
    {
        return ViewComponent("EntryReceipt", code);
    }

    [HttpPost]
    public async Task<IActionResult> AddReceipt(TblReceipt modelToAdd)
    {
        string username = LoggedInUser;

        if (modelToAdd.ReceiptCode == null)
        {
            modelToAdd.ReceiptCode = await codeRepo.GetReceiptCode();
        }

        modelToAdd.ReceiptDate = modelToAdd.ReceiptDate == null ? DateOnly.FromDateTime(DateTime.Now) : modelToAdd.ReceiptDate;
        modelToAdd.IsEnabled = true;
        modelToAdd.CreateDate = DateTime.Now;
        modelToAdd.CreateBy = username;
        modelToAdd.UpdateDate = DateTime.Now;
        modelToAdd.UpdateBy = username;

        ResponseViewModel result = await receiptRepo.AddReceipt(modelToAdd);
        return Json(result);
    }

    public async Task<IActionResult> Edit(string code)
    {
        var data = await receiptRepo.GetBillingDataList();
        var detail = await receiptRepo.GetReceiptDetail(code);
        var model = await receiptRepo.GetReceiptByCode(code);

        ViewBag.Data = data;
        ViewBag.Detail = detail;
        ViewData["ReceiptType"] = from a in dropListRepo.GetReceptType() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == model.ReceiptType };
        ViewData["BankName"] = from a in await dropListRepo.GetBank() select new SelectListItem { Value = a.label.ToString(), Text = a.label, Selected = a.label == model.BankName };

        return View(model);
    }

    public async Task<IActionResult> EditReceipt(TblReceipt modelToEdit)
    {
        string username = LoggedInUser;

        modelToEdit.UpdateDate = DateTime.Now;
        modelToEdit.UpdateBy = username;

        ResponseViewModel result = await receiptRepo.EditReceipt(modelToEdit);
        return Json(result);
    }

    public async Task<IActionResult> EditName(string code, string name)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await receiptRepo.EditReceiptName(code, name, username);
        return Json(result);
    }

    public async Task<IActionResult> EditCustomerName(string code, string name)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await receiptRepo.EditCustomerName(code, name, username);
        return Json(result);
    }

    public async Task<IActionResult> EditTax(string code, string tax_id)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await receiptRepo.EditTax(code, tax_id, username);
        return Json(result);
    }

    public async Task<IActionResult> EditAddress(string code, string address)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await receiptRepo.EditAddress(code, address, username);
        return Json(result);
    }

    public async Task<IActionResult> RemoveReceipt(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await receiptRepo.RemoveReceipt(code, username);
        return Json(result);
    }

    public async Task<IActionResult> GetReceiptDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await receiptRepo.GetReceiptDataList(iFilter.month, iFilter.year);
        var data = query.Where(w => (iFilter.filterCode == null || w.receipt_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterCustomer == null || w.customer_name.Contains(iFilter.filterCustomer.ToUpper())));

        Func<ReceiptViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.receipt_date :
                                                                param.iSortCol_0 == 2 ? c.receipt_part_code :
                                                                param.iSortCol_0 == 3 ? c.customer_name : c.receipt_part_code);

        IEnumerable<ReceiptViewModel> listQuery;
        if (param.sSortDir_0 == "desc")
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

    public async Task<IActionResult> Detail(string code)
    {
        var data = await formRepo.GetReceiptByCode(code);
        var customer = await formRepo.GetCustomerByCode(data.CustomerCode);
        var rule = await formRepo.GetCustomerRule(data.CustomerCode);
        var detail = await formRepo.GetReceiptValue(code);

        ViewBag.Customer = customer;
        ViewBag.Detail = detail;
        ViewBag.Rule = rule;
        return View(data);
    }

    public async Task<IActionResult> EditBillingType(string type, string billing_code, string receipt_code)
    {
        ResponseViewModel result = await receiptRepo.EditBillingType(type, billing_code, receipt_code);
        return Json(result);
    }

    public async Task<IActionResult> ProveData(string receipt_code)
    {
        ResponseViewModel result = await receiptRepo.ProveData(receipt_code);
        return Json(result);
    }

    public async Task<IActionResult> ConfirmTemp(DateOnly receiptdate)
    {
        ResponseViewModel result = await receiptRepo.ConfirmTemp(this.LoggedInUser, receiptdate);
        return Json(result);
    }

    public IActionResult Upload()
    {
        return View();
    }

    public async Task<IActionResult> GetTemp(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var data = await receiptRepo.GetTemp();

        IEnumerable<TempReceiptViewModel> listQuery;
        listQuery = data.OrderBy(o => o.billing_code);

        return Json(new
        {
            sEcho = param.sEcho,
            iTotalRecords = data.Count(),
            iTotalDisplayRecords = data.Count(),
            aaData = listQuery
        });
    }
}