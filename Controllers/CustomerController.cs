using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;
[Authorize(Roles = "Administrator")]
public class CustomerController : Controller
{
    private readonly CodeRepository _codeRepo;
    private readonly CustomerRepository _customerRepo;
    private readonly DropListRepository _dropListRepo;
    private string LoggedInUser => User.Identity.Name;

    public CustomerController(CustomerRepository customerRepo, DropListRepository dropListRepo, CodeRepository codeRepo)
    {
        _customerRepo= customerRepo;
        _dropListRepo = dropListRepo;
        _codeRepo = codeRepo;
    }

    public async Task<IActionResult> Edit(string code)
    {
        var data = await _customerRepo.GetCustomerByCode(code);
        var rule = await _customerRepo.GetRuleByCode(code);
        var exten = data.CustomerExten == "HEAD OFFICE" ? "1" : "2";

        ViewBag.Rule = rule;
        ViewData["customer_type_code"] = from a in await _dropListRepo.GetCustomerType() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.CustomerTypeCode };
        ViewData["branch_type"] = from a in _dropListRepo.GetBranchType() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == exten };
        return PartialView(data);
    }

    [HttpPost]
    public async Task<IActionResult> EditCustomer(TblCustomer customerToEdit, TblCustomerRule ruleToEdit, string branch_type)
    {
        string username = LoggedInUser;
        customerToEdit.UpdateDate = DateTime.Now;
        customerToEdit.UpdateBy = username;
        customerToEdit.CustomerExten = branch_type == "1" ? "HEAD OFFICE" : customerToEdit.CustomerExten;
        customerToEdit.CustomerNameEn = customerToEdit.CustomerNameEn.ToUpper();
        customerToEdit.CustomerNameTh = customerToEdit.CustomerNameTh.ToUpper();
        customerToEdit.CustomerAcronym = customerToEdit.CustomerAcronym.ToUpper();
        customerToEdit.CustomerAddress = customerToEdit.CustomerAddress.ToUpper();

        ResponseViewModel result = await _customerRepo.EditCustomer(customerToEdit, ruleToEdit);
        return Json(result);
    }

    public async Task<IActionResult> Add()
    {
        ViewData["customer_type_code"] = from a in await _dropListRepo.GetCustomerType() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewData["branch_type"] = from a in _dropListRepo.GetBranchType() select new SelectListItem { Value = a.key.ToString(), Text = a.label };

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddCustomer(TblCustomer customerToAdd, TblCustomerRule paymentToAdd, string branch_type)
    {
        string username = LoggedInUser;
        customerToAdd.CustomerCode = await _codeRepo.GetCustomerCode("01");
        customerToAdd.CustomerVatNo = customerToAdd.CustomerVatNo != null ? customerToAdd.CustomerVatNo : "-";
        customerToAdd.CustomerExten = branch_type == "1" ? "HEAD OFFICE" : customerToAdd.CustomerExten;
        customerToAdd.IsActived = true;
        customerToAdd.IsEnabled = true;
        customerToAdd.CreateDate = DateTime.Now;
        customerToAdd.CreateBy = username;
        customerToAdd.UpdateDate = DateTime.Now;
        customerToAdd.UpdateBy = username;
        customerToAdd.CustomerTypeCode = "01";
        customerToAdd.CustomerNameEn = customerToAdd.CustomerNameEn.ToUpper();
        customerToAdd.CustomerNameTh = customerToAdd.CustomerNameTh.ToUpper();
        customerToAdd.CustomerAcronym = customerToAdd.CustomerAcronym.ToUpper();
        customerToAdd.CustomerAddress = customerToAdd.CustomerAddress.ToUpper();

        TblCustomerAddress address = new TblCustomerAddress();
        address.CustomerCode = customerToAdd.CustomerCode;
        address.TransportName = customerToAdd.CustomerNameTh;
        address.TransportAddress = customerToAdd.CustomerAddress;
        address.AdvanceName = customerToAdd.CustomerNameTh;
        address.AdvanceAddress = customerToAdd.CustomerAddress;
        customerToAdd.TblCustomerAddress = address;

        ResponseViewModel result = await _customerRepo.AddCustomer(customerToAdd, paymentToAdd);
        return Json(result);
    }

    public IActionResult Index()
    {
        string username = LoggedInUser;
        return View();
    }

    public async Task<IActionResult> GetCustomerDataByCode(string code)
    {
        var data = await _customerRepo.GetCustomerDataByCode(code);

        return Json(new
        {
            data
        });
    }

    public async Task<IActionResult> GetCustomerDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await _customerRepo.GetCustomerDataList();
        var data = query.Where(w => (iFilter.filterNameEN == null || w.customer_name_en.ToUpper().Contains(iFilter.filterNameEN.ToUpper())) &&
        (iFilter.filterNameTH == null || w.customer_acronym.ToUpper().Contains(iFilter.filterNameTH.ToUpper())));

        Func<CustomerViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.customer_name_th :
        param.iSortCol_0 == 2 ? c.customer_name_en :
        param.iSortCol_0 == 3 ? c.customer_type_name : c.customer_name_th);

        IEnumerable<CustomerViewModel> listQuery;
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

    public async Task<IActionResult> IsVatNoDuplicate(string vat_no)
    {
        bool result = await _customerRepo.IsVatNoDuplicate(vat_no);

        return Json(result);
    }

    public async Task<IActionResult> IsAcronymDuplicate(string name)
    {
        bool result = await _customerRepo.IsAcronymDuplicate(name);

        return Json(result);
    }

    public async Task<IActionResult> Remove(string customer_code)
    {
        string username = LoggedInUser;
        ResponseViewModel result = await _customerRepo.Remove(customer_code, username);

        return Json(result);
    }

    public async Task<IActionResult> Active(string customer_code)
    {
        string username = LoggedInUser;
        ResponseViewModel result = await _customerRepo.Active(customer_code, username);

        return Json(result);
    }

    public async Task<IActionResult> Inactive(string customer_code)
    {
        string username = LoggedInUser;
        ResponseViewModel result = await _customerRepo.Inactive(customer_code, username);

        return Json(result);
    }
}