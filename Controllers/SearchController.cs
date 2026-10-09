using Microsoft.AspNetCore.Mvc;
using TMSWeb.Models;
using TMSWeb.Repositories;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;
[Authorize]
public class SearchController : Controller
{
    private readonly SearchRepository searchRepo;

    public SearchController(SearchRepository _searchRepo)
    {
        searchRepo = _searchRepo;
    }
    public IActionResult Port()
    {
        return View();
    }

    public IActionResult Yard()
    {
        return View();
    }

    public IActionResult TransportRate(string ev)
    {
        ViewBag.Event = ev;
        return View();
    }

    public IActionResult Place()
    {
        return View();
    }

    public IActionResult Customer()
    {
        return View();
    }

    public IActionResult YardPort(string ev)
    {
        ViewBag.Event = ev;
        return View();
    }

    public IActionResult Spareparts()
    {
        return View();
    }

    public IActionResult Supplier()
    {
        return View();
    }

    public IActionResult Zone()
    {
        return View();
    }

    public async Task<IActionResult> GetYardPortDataList(jQueryDataTableParamModel param)
    {
        var data = await searchRepo.GetYardPortDataList();
        if (!(string.IsNullOrEmpty(param.sSearch)))
        {
            data = data.Where(w => w.yardport_code.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.yardport_name.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.yardport_type.ToUpper().Contains(param.sSearch.ToUpper())).ToList();
        }

        Func<YardPortViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.yardport_name :
        param.iSortCol_0 == 2 ? c.yardport_type : c.yardport_type);

        IEnumerable<YardPortViewModel> listQuery;
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

    public async Task<IActionResult> GetPortDataList(jQueryDataTableParamModel param)
    {
        var data = await searchRepo.GetPortDataList();
        if (!(string.IsNullOrEmpty(param.sSearch)))
        {
            data = data.Where(w => w.yardport_code.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.yardport_name.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.yardport_type.ToUpper().Contains(param.sSearch.ToUpper())).ToList();
        }

        Func<YardPortViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.yardport_name : c.yardport_name);

        IEnumerable<YardPortViewModel> listQuery;
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

    public async Task<IActionResult> GetYardDataList(jQueryDataTableParamModel param)
    {
        var data = await searchRepo.GetYardDataList();
        if (!(string.IsNullOrEmpty(param.sSearch)))
        {
            data = data.Where(w => w.yardport_code.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.yardport_name.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.yardport_type.ToUpper().Contains(param.sSearch.ToUpper())).ToList();
        }

        Func<YardPortViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.yardport_name : c.yardport_name);

        IEnumerable<YardPortViewModel> listQuery;
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

    public async Task<IActionResult> GetPlaceDataList(jQueryDataTableParamModel param)
    {
        var data = await searchRepo.GetPlaceDataList();
        if (!(string.IsNullOrEmpty(param.sSearch)))
        {
            data = data.Where(w => w.place_code.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.place_name.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.place_port_code.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.zone_name.ToUpper().Contains(param.sSearch.ToUpper())).ToList();
        }

        Func<PlaceViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.place_port_code :
        param.iSortCol_0 == 2 ? c.place_name :
        param.iSortCol_0 == 3 ? c.zone_name : c.place_name);

        IEnumerable<PlaceViewModel> listQuery;
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

    public async Task<IActionResult> GetCustomerCostDataList(jQueryDataTableParamModel param)
    {
        var data = await searchRepo.GetCustomerCostDataList();
        if (!(string.IsNullOrEmpty(param.sSearch)))
        {
            data = data.Where(w => w.cost_code.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.customer_acronym.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.mission_name.ToUpper().Contains(param.sSearch.ToUpper())).ToList();
        }

        Func<CostViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.customer_name_th :
                                                                param.iSortCol_0 == 2 ? c.cost_name :
                                                                param.iSortCol_0 == 3 ? c.cost_remark : c.cost_name);

        IEnumerable<CostViewModel> listQuery;
        if (param.sSortDir_0 == "asc")
        {
            if (param.iSortCol_0 == 4)
            {
                listQuery = data.OrderBy(o => o.cost_value)
                                .Skip(param.iDisplayStart)
                                .Take(param.iDisplayLength);
            }
            else
            {
                listQuery = data.OrderBy(orderingFunction)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength);
            }
        }
        else
        {
            if (param.iSortCol_0 == 4)
            {
                listQuery = data.OrderByDescending(o => o.cost_value)
                                .Skip(param.iDisplayStart)
                                .Take(param.iDisplayLength);
            }
            else
            {
                listQuery = data.OrderByDescending(orderingFunction)
                                .Skip(param.iDisplayStart)
                                .Take(param.iDisplayLength);
            }
        }

        return Json(new
        {
            sEcho = param.sEcho,
            iTotalRecords = data.Count(),
            iTotalDisplayRecords = data.Count(),
            aaData = listQuery
        });
    }

    public async Task<IActionResult> GetCustomerDataList(jQueryDataTableParamModel param)
    {
        var data = await searchRepo.GetCustomerDataList();
        if (!(string.IsNullOrEmpty(param.sSearch)))
        {
            data = data.Where(w => w.customer_code.ToLower().Contains(param.sSearch.ToLower()) ||
                                w.customer_name_th.ToLower().Contains(param.sSearch.ToLower()) ||
                                w.customer_name_en.ToLower().Contains(param.sSearch.ToLower())).ToList();
        }

        Func<CustomerViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.customer_name_en :
        param.iSortCol_0 == 2 ? c.customer_address : c.customer_name_en);

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

    public async Task<IActionResult> GetSupplierDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var data = await searchRepo.GetSupplierDataList();
        if (!(string.IsNullOrEmpty(param.sSearch)))
        {
            data = data.Where(w => w.supplier_name.ToLower().Contains(param.sSearch.ToLower())).ToList();
        }

        Func<SupplierViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.supplier_name :
        param.iSortCol_0 == 2 ? c.supplier_type : c.supplier_name);

        IEnumerable<SupplierViewModel> listQuery;
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

    public async Task<IActionResult> GetZoneDataList(jQueryDataTableParamModel param)
    {
        var data = await searchRepo.GetZoneDataList();
        if (!(string.IsNullOrEmpty(param.sSearch)))
        {
            data = data.Where(w => w.zone_name.ToLower().Contains(param.sSearch.ToLower())).ToList();
        }

        Func<ZoneViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.zone_name : c.zone_code);

        IEnumerable<ZoneViewModel> listQuery;
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
}