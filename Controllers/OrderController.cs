using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Models;
using TMSWeb.Models.ViewModels;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;

namespace TMSWeb.Controllers;
[Authorize]
public class OrderController : Controller
{
    private readonly OrderRepository orderRepo;
    private readonly DropListRepository dropListRepo;
    private string LoggedInUser => User.Identity.Name;

    public OrderController(OrderRepository _orderRepo, DropListRepository _dropListRepo)
    {
        orderRepo = _orderRepo;
        dropListRepo = _dropListRepo;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> GetOrderBookingDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await orderRepo.GetOrderBookingList();
        var data = query.Where(w => ((iFilter.filterDateRangeStart == null || iFilter.filterDateRangeEnd == null) || w.job_date_value >= iFilter.filterDateRangeStart && w.job_date_value <= iFilter.filterDateRangeEnd) &&
        (iFilter.filterCode == null || w.job_detail_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterMission == null || w.mission_name.Contains(iFilter.filterMission.ToUpper())) &&
        (iFilter.filterBookingNo == null || w.booking_no.Contains(iFilter.filterBookingNo.ToUpper())) &&
        (iFilter.filterInvoice == null || w.job_invoice.Contains(iFilter.filterInvoice.ToUpper())));

        Func<OrderModel, string> orderingFunction = (c => param.iSortCol_0 == 2 ? c.job_code :
                                                                param.iSortCol_0 == 3 ? c.mission_name :
                                                                param.iSortCol_0 == 4 ? c.booking_no :
                                                                param.iSortCol_0 == 5 ? c.job_invoice :
                                                                param.iSortCol_0 == 6 ? c.job_type : c.job_code);

        IEnumerable<OrderModel> listQuery;
        if (param.iSortCol_0 == 1)
        {
            if (param.sSortDir_0 == "asc")
            {
                listQuery = data.OrderBy(o => o.job_date_value)
                                .Skip(param.iDisplayStart)
                                .Take(param.iDisplayLength);
            }
            else
            {
                listQuery = data.OrderByDescending(o => o.job_date_value)
                                .Skip(param.iDisplayStart)
                                .Take(param.iDisplayLength);
            }
        }
        else
        {
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
        }

        return Json(new
        {
            sEcho = param.sEcho,
            iTotalRecords = data.Count(),
            iTotalDisplayRecords = data.Count(),
            aaData = listQuery
        });
    }

    [Authorize(Roles = "Administrator, CS")]
    public IActionResult Add()
    {
        return View();
    }

    public async Task<IActionResult> AddContainer(TblJobMaster model, OrderPlaceModel placeJob, string txtOther)
    {
        string username = LoggedInUser;

        model.ClosingTime = model.ClosingTime == null ? "" : model.ClosingTime;
        model.JobIncoterm = "-";
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = username;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = username;

        ResponseViewModel result = await orderRepo.AddContainerOrder(model, placeJob, txtOther);
        return Json(result);
    }

    public async Task<IActionResult> AddFloor(TblJobMaster orderToAdd, OrderPlaceModel placeJob, string txtOther)
    {
        ResponseViewModel result = new ResponseViewModel();
        string username = LoggedInUser;

        orderToAdd.ClosingTime = orderToAdd.ClosingTime == null ? "" : orderToAdd.ClosingTime;
        orderToAdd.JobIncoterm = "-";
        orderToAdd.JobType = "FLOOR";
        orderToAdd.ContainerType = "FLOOR";
        orderToAdd.IsEnabled = true;
        orderToAdd.CreateDate = DateTime.Now;
        orderToAdd.CreateBy = username;
        orderToAdd.UpdateDate = DateTime.Now;
        orderToAdd.UpdateBy = username;

        result = await orderRepo.AddFloorOrder(orderToAdd, placeJob, txtOther);
        return Json(result);
    }

    public async Task<IActionResult> AddOcean(TblJobMaster orderToAdd, OrderPlaceModel placeJob, OrderOceanModel oceanJob, string txtOceanOther)
    {
        string username = LoggedInUser;

        orderToAdd.JobType = "OCEAN";
        orderToAdd.ContainerType = "LADEN";
        orderToAdd.AgentCode = "ไม่ระบุ";
        orderToAdd.ClosingTime = orderToAdd.ClosingTime == null ? "" : orderToAdd.ClosingTime;
        orderToAdd.IsEnabled = true;
        orderToAdd.CreateDate = DateTime.Now;
        orderToAdd.CreateBy = username;
        orderToAdd.UpdateDate = DateTime.Now;
        orderToAdd.UpdateBy = username;

        ResponseViewModel result = await orderRepo.AddOceanOrder(orderToAdd, placeJob, oceanJob, txtOceanOther);
        return Json(result);
    }

    public async Task<IActionResult> AddDomestic(TblJobMaster orderToAdd, OrderPlaceModel placeJob)
    {
        string username = LoggedInUser;

        orderToAdd.CostCode = placeJob.txtDomesticCostCode;
        orderToAdd.JobIncoterm = "-";
        orderToAdd.JobType = "DOMESTIC";
        orderToAdd.ContainerType = "-";
        orderToAdd.AgentCode = "ไม่ระบุ";
        placeJob.txtFinishCode = 
        orderToAdd.ClosingTime = orderToAdd.ClosingTime == null ? "" : orderToAdd.ClosingTime;
        orderToAdd.IsEnabled = true;
        orderToAdd.CreateDate = DateTime.Now;
        orderToAdd.CreateBy = username;
        orderToAdd.UpdateDate = DateTime.Now;
        orderToAdd.UpdateBy = username;

        ResponseViewModel result = await orderRepo.AddDomesticOrder(orderToAdd, placeJob);
        return Json(result);
    }

    [Authorize(Roles = "Administrator,CS")]
    public async Task<IActionResult> EditContainerJob(string code)
    {
        var data = await orderRepo.GetOrderDataByCode(code);
        var customer = await orderRepo.GetCustomerCostByCode(data.CostCode);
        var jp = await orderRepo.GetOrderPlaceDataByCode(data.JobCode);
        var start = await orderRepo.GetYardPortDataByCode(jp.StartCode);
        var mission = await orderRepo.GetPlaceByCode(jp.MissionCode);
        var finish = await orderRepo.GetYardPortDataByCode(jp.FinishCode);

        var placeJob = new OrderPlaceModel();
        placeJob.txtCustomerName = customer.customer_name_th;
        placeJob.txtStartCode = jp.StartCode;
        placeJob.StartName = start.place_name;
        placeJob.txtMissionCode = jp.MissionCode;
        placeJob.MissionName = mission.place_name;
        placeJob.txtFinishCode = jp.FinishCode;
        placeJob.FinishName = finish.place_name;
        placeJob.expressway = jp.ExpresswayValue;
        placeJob.txtStartType = start.place_type;
        placeJob.txtMissionType = mission.place_type;
        placeJob.chkStartPortYard = jp.IsStartyardport;
        placeJob.chkFinishPortYard = jp.IsFinishyardport;

        ViewBag.Job = placeJob;
        ViewData["AgentCode"] = from a in await dropListRepo.GetAgent() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.AgentCode };
        ViewData["ContainerSizeCode"] = from a in await dropListRepo.GetContainerSize() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.ContainerSizeCode };
        ViewData["TruckTypegroupCode"] = from a in await dropListRepo.GetTruckGroup() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.TruckTypegroupCode };

        return View(data);
    }

    [Authorize(Roles = "Administrator,CS")]
    public async Task<IActionResult> EditFloorJob(string code)
    {
        var data = await orderRepo.GetOrderDataByCode(code);
        var customer = await orderRepo.GetCustomerCostByCode(data.CostCode);
        var jp = await orderRepo.GetOrderPlaceDataByCode(data.JobCode);
        var start = await orderRepo.GetYardPortDataByCode(jp.StartCode);
        var mission = await orderRepo.GetPlaceByCode(jp.MissionCode);
        var finish = await orderRepo.GetYardPortDataByCode(jp.FinishCode);

        var placeJob = new OrderPlaceModel();
        placeJob.txtCustomerName = customer.customer_name_th;
        placeJob.txtStartCode = jp.StartCode;
        placeJob.StartName = start.place_name;
        placeJob.txtMissionCode = jp.MissionCode;
        placeJob.MissionName = mission.place_name;
        placeJob.txtFinishCode = jp.FinishCode;
        placeJob.FinishName = finish.place_name;
        placeJob.txtStartType = start.place_type;
        placeJob.txtMissionType = mission.place_type;
        placeJob.expressway = jp.ExpresswayValue;
        placeJob.chkStartPortYard = jp.IsStartyardport;
        placeJob.chkFinishPortYard = jp.IsFinishyardport;

        ViewBag.Job = placeJob;
        ViewData["AgentCode"] = from a in await dropListRepo.GetAgent() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.AgentCode };
        ViewData["ContainerSizeCode"] = from a in await dropListRepo.GetContainerSize() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.ContainerSizeCode };
        ViewData["TruckTypegroupCode"] = from a in await dropListRepo.GetTruckGroup() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.TruckTypegroupCode };

        return View(data);
    }

    [Authorize(Roles = "Administrator,CS")]
    public async Task<IActionResult> EditOceanJob(string code)
    {
        var data = await orderRepo.GetOrderDataByCode(code);
        var customer = await orderRepo.GetCustomerCostByCode(data.CostCode);
        var jp = await orderRepo.GetJobPlaceDataByCode(data.JobCode);
        var start = await orderRepo.GetYardPortDataByCode(jp.StartCode);
        var mission = await orderRepo.GetPlaceByCode(jp.MissionCode);
        var finish = await orderRepo.GetYardPortDataByCode(jp.FinishCode);
        var ocean = await orderRepo.GetOceanJob(code);

        var placeJob = new OrderPlaceModel();
        placeJob.txtCustomerName = customer.customer_name_th;
        placeJob.txtStartCode = jp.StartCode;
        placeJob.StartName = start.place_name;
        placeJob.txtMissionCode = jp.MissionCode;
        placeJob.MissionName = mission.place_name;
        placeJob.txtFinishCode = jp.FinishCode;
        placeJob.FinishName = finish.place_name;
        placeJob.expressway = jp.ExpresswayValue;
        placeJob.txtStartType = start.place_type;
        placeJob.txtMissionType = mission.place_type;
        placeJob.chkStartPortYard = jp.IsStartyardport;
        placeJob.chkFinishPortYard = jp.IsFinishyardport;

        ViewBag.Job = placeJob;
        ViewBag.Ocean = ocean;
        ViewData["AgentCode"] = from a in await dropListRepo.GetAgent() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.AgentCode };
        ViewData["ContainerSizeCode"] = from a in await dropListRepo.GetContainerSize() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.ContainerSizeCode };
        ViewData["TruckTypegroupCode"] = from a in await dropListRepo.GetTruckGroup() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.TruckTypegroupCode };
        ViewData["consignee_name"] = from a in await dropListRepo.GetConsignee() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == ocean.FirstOrDefault().ConsigneeName };
        ViewData["shipper_name"] = from a in await dropListRepo.GetShipper() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == ocean.FirstOrDefault().ShipperName };
        ViewData["request_name"] = from a in await dropListRepo.GetRequestBy() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == ocean.FirstOrDefault().RequestName };
        ViewData["shipping_name"] = from a in await dropListRepo.GetShippingContact() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == ocean.FirstOrDefault().ShippingName };

        return View(data);
    }

    [Authorize(Roles = "Administrator,CS")]
    [HttpPost]
    public async Task<IActionResult> EditOrder(TblJobMaster orderModel, OrderPlaceModel placeModel, OrderOceanModel oceanModel, string txtOther)
    {
        ResponseViewModel result = new ResponseViewModel();
        string username = LoggedInUser;

        orderModel.UpdateDate = DateTime.Now;
        orderModel.UpdateBy = username;
        if (orderModel.JobType == "OCEAN")
        {
            orderModel.AgentCode = "-";
            orderModel.BookingNo = oceanModel.change_no;
            orderModel.ClosingTime = "";
            result = await orderRepo.EditOceanOrder(orderModel, placeModel, oceanModel, txtOther);
        }
        else if (orderModel.JobType == "FLOOR")
        {
            orderModel.AgentCode = "-";
            result = await orderRepo.EditFloorOrder(orderModel, placeModel, txtOther);
        }
        else
        {
            result = await orderRepo.EditContainerOrder(orderModel, placeModel, txtOther);
        }

        return Json(result);
    }

    [Authorize(Roles = "Administrator, Transport, Super")]
    public async Task<IActionResult> OrderTruck(string code, string detail_code)
    {
        var truck = new List<DropDownViewModel>();

        var data = await orderRepo.GetOrderDataByCode(code);
        var customer = await orderRepo.GetCustomerCostByCode(data.CostCode);
        var jp = await orderRepo.GetOrderPlaceDataByCode(data.JobCode);
        var start = await orderRepo.GetYardPortDataByCode(jp.StartCode);
        var mission = await orderRepo.GetPlaceByCode(jp.MissionCode);
        var finish = await orderRepo.GetYardPortDataByCode(jp.FinishCode);
        var job2Short = await orderRepo.Get2Short(detail_code);

        var model = new TblJobDetail();
        model.JobDetailCode = detail_code;
        model.JobCode = code;
        model.StartPlaceCode = start.place_code;
        model.MissionPlaceCode = mission.place_code;
        model.FinishPlaceCode = finish.place_code;
        model.CostValue = customer.cost_value;
        model.Is2short = job2Short.Is2short;

        var placeJob = new OrderPlaceModel();
        placeJob.txtCustomerName = customer.customer_name_th;
        placeJob.acronym = customer.customer_acronym;
        placeJob.txtStartCode = jp.StartCode;
        placeJob.StartName = start.place_name;
        placeJob.txtMissionCode = jp.MissionCode;
        placeJob.MissionName = mission.place_name;
        placeJob.txtFinishCode = jp.FinishCode;
        placeJob.FinishName = finish.place_name;

        ViewData["SupplierCode"] = from a in await dropListRepo.GetSupplierTransport() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == "SP18001" };
        ViewData["TruckCode"] = from a in await dropListRepo.GetTruck() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewData["DriverCode"] = from a in await dropListRepo.GetDriver() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewBag.PlaceJob = placeJob;
        ViewBag.Data = data;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> OrderTruck(TblJobDetail model)
    {
        ResponseViewModel response = new ResponseViewModel();
        string username = LoggedInUser;

        model.DriverCode = model.SupplierCode != "SP18001" ? "DV000000" : model.DriverCode;
        model.TruckCode = model.SupplierCode != "SP18001" ? "T000000" : model.TruckCode;
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = username;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = username;

        response = await orderRepo.OrderTruck(model);
        return Json(response);
    }

    [Authorize(Roles = "Administrator,CS")]
    public async Task<IActionResult> RemoveOrder(string code, string job_detail_code)
    {
        ResponseViewModel result = new ResponseViewModel();
        string username = LoggedInUser;

        result = await orderRepo.RemoveOrder(code, job_detail_code, username);
        return Json(result);
    }

    public async Task<IActionResult> GetCustomerCost(string code)
    {
        var data = await orderRepo.GetCustomerCostByCode(code);

        return Json(data);
    }

    public async Task<IActionResult> GetPlace(string code)
    {
        var data = await orderRepo.GetPlaceByCode(code);

        return Json(data);
    }

    public async Task<IActionResult> GetYardPort(string code)
    {
        var data = await orderRepo.GetYardPortDataByCode(code);

        return Json(data);
    }

    public async Task<IActionResult> GetDriverData()
    {
        var result = await orderRepo.GetDriver();

        return Json(new
        {
            result
        });
    }

    public async Task<IActionResult> GetTruckDriverByTruckCode(string code)
    {
        var data = await orderRepo.GetTruckDriverByTruckCode(code);

        return Json(data);
    }

    void SendNotification(string title, string body, string fcmToken)
    {
        try
        {
            // Get the server key from FCM console
            var serverKey = string.Format("key={0}", "AAAApGn5Bp8:APA91bG2nE2cUFxFUvx5MUTBdGmK9RBv0bfb0bdluf5qtfWbUSCklCyw6wrBCnGawQ98WXY1J7qY_P_Itgf5QvHBJYFP5mkwZgdCsZHfFYjrE0wruNKEQJPoRnQ4PE1YTX9oEnHSLPlt");

            // Get the sender id from FCM console
            var senderId = string.Format("id={0}", "706152564383");

            var data = new
            {
                to = fcmToken, // Recipient device token
                notification = new { title, body }
            };

            // Using Newtonsoft.Json
            var jsonBody = JsonConvert.SerializeObject(data);

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send"))
            {
                httpRequest.Headers.TryAddWithoutValidation("Authorization", serverKey);
                httpRequest.Headers.TryAddWithoutValidation("Sender", senderId);
                httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var result = httpClient.SendAsync(httpRequest).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        //return true;
                    }
                    else
                    {
                        // Use result.StatusCode to handle failure
                        // Your custom error handler here
                        //_logger.LogError($"Error sending notification. Status Code: {result.StatusCode}");
                    }
                }
            }
        }
        catch
        {
            //_logger.LogError($"Exception thrown in Notify Service: {ex}");
        }
    }
}