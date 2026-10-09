using Microsoft.AspNetCore.Mvc;
using TMSWeb.Repositories;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;

public class DriverJobController : Controller
{
    private readonly DriverJobRepository jobRepo;
    private readonly JobRepository jobMasterRepo;
    private string LoggedInUser => User.Identity.Name;

    public DriverJobController(DriverJobRepository _jobRepo, JobRepository _jobMasterRepo)
    {
        jobRepo = _jobRepo;
        jobMasterRepo = _jobMasterRepo;
    }

    public async Task<IActionResult> FloorInvoice(string code)
    {
        var model = new OrderFormViewModel();
        var data = await jobRepo.GetOrderByCode(code);
        if (data != null)
        {
            model.driver_fullname = data.DriverFullname;
            model.truck_license = data.TruckLicense;
            model.job_type = data.JobType;
            model.job_detail_code = data.JobDetailCode;
            model.booking_no = data.BookingNo;
            model.mission_name = data.MissionName;
            model.job_date = data.JobDate;
            model.container_no = data.ContainerNo;
            model.seal_no = data.SealNo;
            model.trailer_license = data.TrailerLicense;
            model.tracking_status = data.TrackingStatus;
        }
        else
        {
            var job = await jobRepo.GetJobOrderByCode(code);
            model.driver_fullname = "";
            model.truck_license = "";
            model.job_type = job.JobType;
            model.job_detail_code = job.JobDetailCode;
            model.booking_no = job.BookingNo;
            model.mission_name = job.MissionName;
            model.job_date = job.JobDate;
            model.tracking_status = "-";
        }

        if (model.job_type == "FLOOR")
        {
            var tracking = this.GetTracking(data.JobDetailCode, model.job_type);
            model.container_no = model.container_no != "-" ? model.container_no : "";
            model.seal_no = model.seal_no != "-" ? model.seal_no : "";
            model.trailer_license = model.trailer_license != "-" ? model.trailer_license : "";

            ViewBag.Tracking = tracking;
            return View(model);
        }
        else
        {
            return RedirectToAction("importinvoice", "driverjob", new { @code = code });
        }
    }

    public async Task<IActionResult> ImportInvoice(string code)
    {
        var model = new OrderFormViewModel();
        var data = await jobRepo.GetOrderByCode(code);
        if (data != null)
        {
            model.driver_fullname = data.DriverFullname;
            model.truck_license = data.TruckLicense;
            model.job_type = data.JobType;
            model.job_detail_code = data.JobDetailCode;
            model.booking_no = data.BookingNo;
            model.mission_name = data.MissionName;
            model.job_date = data.JobDate;
            model.container_no = data.ContainerNo;
            model.seal_no = data.SealNo;
            model.trailer_license = data.TrailerLicense;
            model.tracking_status = data.TrackingStatus;
        }
        else
        {
            var job = await jobRepo.GetJobOrderByCode(code);
            model.driver_fullname = "";
            model.truck_license = "";
            model.job_type = job.JobType;
            model.job_detail_code = job.JobDetailCode;
            model.booking_no = job.BookingNo;
            model.mission_name = job.MissionName;
            model.job_date = job.JobDate;
            model.tracking_status = "-";
        }

        if (model.job_type == "IMPORT")
        {
            var tracking = this.GetTracking(data.JobDetailCode, model.job_type);
            model.container_no = model.container_no != "-" ? model.container_no : "";
            model.seal_no = model.seal_no != "-" ? model.seal_no : "";
            model.trailer_license = model.trailer_license != "-" ? model.trailer_license : "";

            ViewBag.Tracking = tracking;
            return View(model);
        }
        else
        {
            return RedirectToAction("exportinvoice", "driverjob", new { @code = code });
        }
    }

    public async Task<IActionResult> ExportInvoice(string code)
    {
        var model = new OrderFormViewModel();
        var data = await jobRepo.GetOrderByCode(code);
        if (data != null)
        {
            model.driver_code = data.DriverCode;
            model.driver_fullname = data.DriverFullname;
            model.truck_license = data.TruckLicense;
            model.job_type = data.JobType;
            model.job_detail_code = data.JobDetailCode;
            model.booking_no = data.BookingNo;
            model.mission_name = data.MissionName;
            model.job_date = data.JobDate;
            model.container_no = data.ContainerNo;
            model.seal_no = data.SealNo;
            model.trailer_license = data.TrailerLicense;
            model.tracking_status = data.TrackingStatus;
        }
        else
        {
            var job = await jobRepo.GetJobOrderByCode(code);

            model.driver_fullname = "";
            model.truck_license = "";
            model.job_type = job.JobType;
            model.job_detail_code = job.JobDetailCode;
            model.booking_no = job.BookingNo;
            model.mission_name = job.MissionName;
            model.job_date = job.JobDate;
            model.tracking_status = "-";
        }

        if (model.job_type == "EXPORT")
        {
            var tracking = this.GetTracking(data.JobDetailCode, model.job_type);
            model.container_no = model.container_no != "-" ? model.container_no : "";
            model.seal_no = model.seal_no != "-" ? model.seal_no : "";
            model.trailer_license = model.trailer_license != "-" ? model.trailer_license : "";

            ViewBag.Tracking = tracking;
            return View(model);
        }
        else
        {
            return RedirectToAction("oceaninvoice", "driverjob", new { @code = code });
        }
    }

    public async Task<IActionResult> OceanInvoice(string code)
    {
        var model = new OrderFormViewModel();
        var data = await jobRepo.GetOrderByCode(code);
        if (data != null)
        {
            model.driver_code = data.DriverCode;
            model.driver_fullname = data.DriverFullname;
            model.truck_license = data.TruckLicense;
            model.job_type = data.JobType;
            model.job_detail_code = data.JobDetailCode;
            model.booking_no = data.BookingNo;
            model.mission_name = data.MissionName;
            model.job_date = data.JobDate;
            model.container_no = data.ContainerNo;
            model.seal_no = data.SealNo;
            model.trailer_license = data.TrailerLicense;
            model.tracking_status = data.TrackingStatus;
        }
        else
        {
            var job = await jobRepo.GetJobOrderByCode(code);

            model.driver_fullname = "";
            model.truck_license = "";
            model.job_type = job.JobType;
            model.job_detail_code = job.JobDetailCode;
            model.booking_no = job.BookingNo;
            model.mission_name = job.MissionName;
            model.job_date = job.JobDate;
            model.tracking_status = "-";
        }

        if (model.job_type == "OCEAN")
        {
            var tracking = this.GetTracking(data.JobDetailCode, model.job_type);
            model.container_no = model.container_no != "-" ? model.container_no : "";
            model.seal_no = model.seal_no != "-" ? model.seal_no : "";
            model.trailer_license = model.trailer_license != "-" ? model.trailer_license : "";

            ViewBag.Tracking = tracking;
            return View(model);
        }
        else
        {
            return RedirectToAction("floorinvoice", "driverjob", new { @code = code });
        }
    }


    #region Sharing
    public async Task<IActionResult> UpdateST(string job_detail_code, string driver_code)
    {
        ResponseViewModel result = await jobRepo.UpdateST(job_detail_code, driver_code);

        return Json(result);
    }

    public async Task<IActionResult> UpdateWP(UpdateDriverJobModel model, IFormFile container, IFormFile seal)
    {
        model.container_pic = this.fileToBase64(container);
        model.seal_pic = this.fileToBase64(seal);

        ResponseViewModel result = await jobRepo.UpdateWP(model);

        return Json(result);
    }
    #endregion

    #region Import
    public async Task<IActionResult> UpdateImportAR(UpdateDriverJobModel model)
    {
        ResponseViewModel result = await jobRepo.UpdateImportAR(model);

        return Json(result);
    }

    public async Task<IActionResult> UpdateImportFN(UpdateDriverJobModel model)
    {
        ResponseViewModel result = await jobRepo.UpdateImportFN(model);

        return Json(result);
    }

    public async Task<IActionResult> UpdateImportDO(UpdateDriverJobModel model)
    {
        ResponseViewModel result = await jobRepo.UpdateImportDO(model);

        return Json(result);
    }
    #endregion

    #region Export
    public async Task<IActionResult> UpdateExportAR(UpdateDriverJobModel model)
    {
        ResponseViewModel result = await jobRepo.UpdateExportAR(model);

        return Json(result);
    }

    public async Task<IActionResult> UpdateExportDO(UpdateDriverJobModel model)
    {
        ResponseViewModel result = await jobRepo.UpdateExportDO(model);

        return Json(result);
    }
    #endregion

    #region Floor
    public async Task<IActionResult> UpdateFloorAR(UpdateDriverJobModel model, List<IFormFile> file)
    {
        List<byte[]> list = new List<byte[]>();

        foreach (var item in file)
        {
            byte[] img = this.fileToBase64(item);
            list.Add(img);
        }

        model.floor_images = list;
        ResponseViewModel result = await jobRepo.UpdateFloorAR(model);
        return Json(result);
    }

    public async Task<IActionResult> UpdateFloorDO(UpdateDriverJobModel model)
    {
        ResponseViewModel result = await jobRepo.UpdateFloorDO(model);

        return Json(result);
    }
    #endregion

    public IActionResult Test()
    {
        return View();
    }

    private byte[] fileToBase64(IFormFile file)
    {
        using (var ms = new MemoryStream())
        {
            file.CopyTo(ms);
            var fileBytes = ms.ToArray();
            return fileBytes;
        }
    }

    private async Task<List<StepWizardViewModels>> GetTracking(string code, string type)
    {
        var list = new List<StepWizardViewModels>();
        var tracking = await jobMasterRepo.GetListTrackingByJob(code);
        var count_tracking = tracking.Count;
        if (type != "FLOOR")
        {
            for (int i = 0; i < 4; i++)
            {
                var item = new StepWizardViewModels();
                var i_track = new TrackingModel();
                switch (i)
                {
                    case 0:
                        i_track = tracking.Where(w => w.tracking_status == "ST").FirstOrDefault();
                        item.action = "รับงาน";
                        item.action_time = (i_track != null ? i_track.tracking_date.ToString("dd-MM-yyyy HH:mm") : "");
                        item.progress = (i_track != null ? "progress-bar" : "progress-bar-active");
                        item.wizard = (i_track != null ? "bs-wizard-dot-active" : "bs-wizard-dot");
                        item.icon = "fad fa-user-hard-hat";
                        list.Add(item);
                        break;
                    case 1:
                        i_track = tracking.Where(w => w.tracking_status == "WP").FirstOrDefault();
                        item.action = "รับตู้";
                        item.action_time = (i_track != null ? i_track.tracking_date.ToString("dd-MM-yyyy HH:mm") : "");
                        item.progress = (i_track != null ? "progress-bar" : "progress-bar-active");
                        item.wizard = (i_track != null ? "bs-wizard-dot-active" : "bs-wizard-dot");
                        item.icon = "fad fa-container-storage";
                        list.Add(item);
                        break;
                    case 2:
                        i_track = tracking.Where(w => w.tracking_status == "AR").FirstOrDefault();
                        item.action = "รอบรรจุ/รอเปิด";
                        item.action_time = (i_track != null ? i_track.tracking_date.ToString("dd-MM-yyyy HH:mm") : "");
                        item.progress = (i_track != null ? "progress-bar" : "progress-bar-active");
                        item.wizard = (i_track != null ? "bs-wizard-dot-active" : "bs-wizard-dot");
                        item.icon = "fad fa-clipboard-list-check";
                        list.Add(item);
                        break;
                    case 3:
                        i_track = tracking.Where(w => w.tracking_status == "DO").FirstOrDefault();
                        item.action = "คืนตู้";
                        item.action_time = (i_track != null ? i_track.tracking_date.ToString("dd-MM-yyyy HH:mm") : "");
                        item.progress = (i_track != null ? "progress-bar" : "progress-bar-active");
                        item.wizard = (i_track != null ? "bs-wizard-dot-active" : "bs-wizard-dot");
                        item.icon = "fad fa-truck-container";
                        list.Add(item);
                        break;
                    default:
                        item.action_time = "";
                        item.progress = "";
                        item.wizard = "";
                        break;
                }
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                var item = new StepWizardViewModels();
                var i_track = new TrackingModel();
                switch (i)
                {
                    case 0:
                        i_track = tracking.Where(w => w.tracking_status == "ST").FirstOrDefault();
                        item.action = "รับงาน";
                        item.action_time = (i_track != null ? i_track.tracking_date.ToString("dd-MM-yyyy HH:mm") : "");
                        item.progress = (i_track != null ? "progress-bar" : "progress-bar-active");
                        item.wizard = (i_track != null ? "bs-wizard-dot-active" : "bs-wizard-dot");
                        item.icon = "fad fa-user-hard-hat";
                        list.Add(item);
                        break;
                    case 1:
                        i_track = tracking.Where(w => w.tracking_status == "AR").FirstOrDefault();
                        item.action = "รับสินค้า/ลงสินค้า";
                        item.action_time = (i_track != null ? i_track.tracking_date.ToString("dd-MM-yyyy HH:mm") : "");
                        item.progress = (i_track != null ? "progress-bar" : "progress-bar-active");
                        item.wizard = (i_track != null ? "bs-wizard-dot-active" : "bs-wizard-dot");
                        item.icon = "fad fa-truck-loading";
                        list.Add(item);
                        break;
                    case 2:
                        i_track = tracking.Where(w => w.tracking_status == "DO").FirstOrDefault();
                        item.action = "จบงาน";
                        item.action_time = (i_track != null ? i_track.tracking_date.ToString("dd-MM-yyyy HH:mm") : "");
                        item.progress = (i_track != null ? "progress-bar" : "progress-bar-active");
                        item.wizard = (i_track != null ? "bs-wizard-dot-active" : "bs-wizard-dot");
                        item.icon = "fad fa-truck-container";
                        list.Add(item);
                        break;
                    default:
                        item.action_time = "";
                        item.progress = "";
                        item.wizard = "";
                        break;
                }
            }
        }

        return list;
    }
}