using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace TMSWeb.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly DashboardRepository dashboardRepo;
    private readonly BillingRepository billingRepo;
    private readonly DropListRepository dropListRepo;
    private readonly CustomerRepository customerRepo;

    private string? LoggedInUser =>
        User.Identity?.Name;

    public DashboardController(
        DashboardRepository _dashboardRepo, 
        BillingRepository _billingRepo, 
        DropListRepository _dropListRepo,
        CustomerRepository _customerRepo)
    {
        dashboardRepo = _dashboardRepo;
        billingRepo = _billingRepo;
        dropListRepo = _dropListRepo;
        customerRepo = _customerRepo;
    }

    public async Task<IActionResult> SummaryDetail(
        CancellationToken cancellationToken)
    {
        string current =
            DateTime.Now.Year.ToString();

        string lastyear =
            DateTime.Now
                .AddYears(-1)
                .Year
                .ToString();

        string last2year =
            DateTime.Now
                .AddYears(-2)
                .Year
                .ToString();

        /*
         * ทั้ง 9 method ด้านล่างใช้ Dapper + SqlConnection
         * คนละ connection ต่อ call จึงรันพร้อมกันได้
         * โดยไม่ชน EF Core DbContext
         */
        var currentDataTask =
            dashboardRepo.GetYearlyContainerCost(
                current,
                cancellationToken);

        var lastyearDataTask =
            dashboardRepo.GetYearlyContainerCost(
                lastyear,
                cancellationToken);

        var last2yearDataTask =
            dashboardRepo.GetYearlyContainerCost(
                last2year,
                cancellationToken);

        var currentOtherTask =
            dashboardRepo.GetYearlyContainerOther(
                current,
                cancellationToken);

        var lastyearOtherTask =
            dashboardRepo.GetYearlyContainerOther(
                lastyear,
                cancellationToken);

        var last2yearOtherTask =
            dashboardRepo.GetYearlyContainerOther(
                last2year,
                cancellationToken);

        var currentAdvanceTask =
            dashboardRepo.GetYearlyContainerAdvance(
                current,
                cancellationToken);

        var lastyearAdvanceTask =
            dashboardRepo.GetYearlyContainerAdvance(
                lastyear,
                cancellationToken);

        var last2yearAdvanceTask =
            dashboardRepo.GetYearlyContainerAdvance(
                last2year,
                cancellationToken);

        await Task.WhenAll(
            currentDataTask,
            lastyearDataTask,
            last2yearDataTask,
            currentOtherTask,
            lastyearOtherTask,
            last2yearOtherTask,
            currentAdvanceTask,
            lastyearAdvanceTask,
            last2yearAdvanceTask);

        var model =
            new List<SummaryDetailViewModel>
            {
                new()
                {
                    year = "current",
                    year_name = current,
                    data = await currentDataTask,
                    other = await currentOtherTask,
                    advance = await currentAdvanceTask
                },
                new()
                {
                    year = "lastyear",
                    year_name = lastyear,
                    data = await lastyearDataTask,
                    other = await lastyearOtherTask,
                    advance = await lastyearAdvanceTask
                },
                new()
                {
                    year = "last2year",
                    year_name = last2year,
                    data = await last2yearDataTask,
                    other = await last2yearOtherTask,
                    advance = await last2yearAdvanceTask
                }
            };

        return View(model);
    }

    public async Task<IActionResult> GetHistory(
        CancellationToken cancellationToken)
    {
        var dashview =
            new DashboardHistoryViewModel();

        DateTime second_month =
            DateTime.Now.AddMonths(-1);

        DateTime third_month =
            DateTime.Now.AddMonths(-2);

        DateTime forth_month =
            DateTime.Now.AddMonths(-3);

        DateTime fifth_month =
            DateTime.Now.AddMonths(-4);

        DateTime sixth_month =
            DateTime.Now.AddMonths(-5);

        dashview.month_2 =
            second_month.ToString("yyyy MMM");

        dashview.month_3 =
            third_month.ToString("yyyy MMM");

        dashview.month_4 =
            forth_month.ToString("yyyy MMM");

        dashview.month_5 =
            fifth_month.ToString("yyyy MMM");

        dashview.month_6 =
            sixth_month.ToString("yyyy MMM");

        /*
         * ห้าม Task.WhenAll ตรงชุดนี้
         * เพราะ method GetSum... ใช้ EF Core DbContext ตัวเดียวกัน
         * DbContext ไม่รองรับ parallel query
         */

        dashview.net_value2 =
            (double)await dashboardRepo.GetSumNetValue(
                second_month,
                cancellationToken);

        dashview.net_value3 =
            (double)await dashboardRepo.GetSumNetValue(
                third_month,
                cancellationToken);

        dashview.net_value4 =
            (double)await dashboardRepo.GetSumNetValue(
                forth_month,
                cancellationToken);

        dashview.net_value5 =
            (double)await dashboardRepo.GetSumNetValue(
                fifth_month,
                cancellationToken);

        dashview.net_value6 =
            (double)await dashboardRepo.GetSumNetValue(
                sixth_month,
                cancellationToken);

        dashview.bill_value2 =
            (double)await dashboardRepo.GetSumBillingValue(
                second_month,
                cancellationToken);

        dashview.bill_value3 =
            (double)await dashboardRepo.GetSumBillingValue(
                third_month,
                cancellationToken);

        dashview.bill_value4 =
            (double)await dashboardRepo.GetSumBillingValue(
                forth_month,
                cancellationToken);

        dashview.bill_value5 =
            (double)await dashboardRepo.GetSumBillingValue(
                fifth_month,
                cancellationToken);

        dashview.bill_value6 =
            (double)await dashboardRepo.GetSumBillingValue(
                sixth_month,
                cancellationToken);

        dashview.unbill_value2 =
            (double)await dashboardRepo.GetSumUnBillingValue(
                second_month,
                cancellationToken);

        dashview.unbill_value3 =
            (double)await dashboardRepo.GetSumUnBillingValue(
                third_month,
                cancellationToken);

        dashview.unbill_value4 =
            (double)await dashboardRepo.GetSumUnBillingValue(
                forth_month,
                cancellationToken);

        dashview.unbill_value5 =
            (double)await dashboardRepo.GetSumUnBillingValue(
                fifth_month,
                cancellationToken);

        dashview.unbill_value6 =
            (double)await dashboardRepo.GetSumUnBillingValue(
                sixth_month,
                cancellationToken);

        dashview.unpay_value2 =
            (double)await dashboardRepo.GetSumUnCashingValue(
                second_month,
                cancellationToken);

        dashview.unpay_value3 =
            (double)await dashboardRepo.GetSumUnCashingValue(
                third_month,
                cancellationToken);

        dashview.unpay_value4 =
            (double)await dashboardRepo.GetSumUnCashingValue(
                forth_month,
                cancellationToken);

        dashview.unpay_value5 =
            (double)await dashboardRepo.GetSumUnCashingValue(
                fifth_month,
                cancellationToken);

        dashview.unpay_value6 =
            (double)await dashboardRepo.GetSumUnCashingValue(
                sixth_month,
                cancellationToken);

        return Json(dashview);
    }

    public IActionResult HistoryDetail()
    {
        return View();
    }

    public async Task<IActionResult> GetSum(
        CancellationToken cancellationToken)
    {
        var dashview =
            new DashboardCounterViewModel();

        DateTime current =
            DateTime.Now;

        /*
         * ใช้ DbContext ตัวเดียวกัน จึง await ทีละ query
         */
        dashview.sum_netvalue =
            (double)await dashboardRepo.GetSumNetValue(
                current,
                cancellationToken);

        dashview.sum_billingvalue =
            (double)await dashboardRepo.GetSumBillingValue(
                current,
                cancellationToken);

        dashview.sum_unbillingvalue =
            (double)await dashboardRepo.GetSumUnBillingValue(
                current,
                cancellationToken);

        dashview.sum_uncashingvalue =
            (double)await dashboardRepo.GetSumUnCashingValue(
                current,
                cancellationToken);

        return Json(dashview);
    }

    public async Task<JsonResult> _GraphDailyContainer(
        CancellationToken cancellationToken)
    {
        var fc =
            new FusionMSLine();

        var list_ds =
            new List<FCDataset>();

        DateTime current_month =
            DateTime.Now;

        var currentmonthData =
            await dashboardRepo.GetDailyContainerData(
                current_month.Year,
                current_month.Month,
                cancellationToken);

        var dsValues =
            new List<FCDatasetValue>(
                currentmonthData.Count);

        foreach (var item in currentmonthData)
        {
            dsValues.Add(
                new FCDatasetValue
                {
                    value =
                        item.id_count.ToString()
                });
        }

        var ds =
            new FCDataset
            {
                seriesname =
                    current_month.ToString("MMM yyyy"),

                data =
                    dsValues.ToArray()
            };

        list_ds.Add(ds);

        var chart =
            new FCMSLineAttribute
            {
                theme = "fint",
                caption = "",
                subCaption = "",
                xAxisName = "",
                yAxisName = "",
                showXAxisLine = "0",
                xAxisLineColor = "#cccccc"
            };

        int daysInMonth =
            DateTime.DaysInMonth(
                current_month.Year,
                current_month.Month);

        var list_label =
            new List<CategoryValue>(
                daysInMonth);

        for (int i = 1;
             i <= daysInMonth;
             i++)
        {
            list_label.Add(
                new CategoryValue
                {
                    label = i.ToString()
                });
        }

        var category =
            new FCCategory
            {
                category =
                    list_label.ToArray()
            };

        fc.chart = chart;
        fc.categories = category;
        fc.dataset = list_ds.ToArray();

        return Json(fc);
    }

    public async Task<JsonResult> _GraphYearlyContainer(
        CancellationToken cancellationToken)
    {
        var cultureEnInfo =
            new CultureInfo("en-US");

        var fc =
            new FusionMSLine();

        string startyear =
            DateTime.Now.Year.ToString();

        string endyear =
            DateTime.Now
                .AddYears(-2)
                .Year
                .ToString();

        int e =
            int.Parse(startyear);

        int s =
            int.Parse(endyear);

        var years =
            Enumerable
                .Range(
                    s,
                    e - s + 1)
                .ToArray();

        /*
         * GetYearlyContainerData ใช้ Dapper + connection ใหม่ต่อ query
         * จึงเรียกทั้ง 3 ปีพร้อมกันได้
         */
        var queryTasks =
            years.Select(
                async year =>
                    new
                    {
                        year,
                        data =
                            await dashboardRepo
                                .GetYearlyContainerData(
                                    year.ToString(),
                                    cancellationToken)
                    })
                .ToArray();

        var yearlyData =
            await Task.WhenAll(
                queryTasks);

        var list_ds =
            new List<FCDataset>(
                yearlyData.Length);

        foreach (var result in yearlyData)
        {
            var dsValues =
                new List<FCDatasetValue>(
                    result.data.Count);

            foreach (var item in result.data)
            {
                dsValues.Add(
                    new FCDatasetValue
                    {
                        value =
                            item.id_count.ToString()
                    });
            }

            list_ds.Add(
                new FCDataset
                {
                    seriesname =
                        result.year.ToString(),

                    data =
                        dsValues.ToArray()
                });
        }

        var chart =
            new FCMSLineAttribute
            {
                theme = "fint",
                caption = "",
                subCaption = "",
                xAxisName = "",
                yAxisName = "",
                showXAxisLine = "0",
                xAxisLineColor = "#cccccc"
            };

        var list_label =
            new List<CategoryValue>(12);

        for (int i = 1;
             i <= 12;
             i++)
        {
            DateTime dt =
                new(
                    e,
                    i,
                    1);

            string sMonth =
                dt.ToString(
                    "MMM",
                    cultureEnInfo)
                .ToUpper();

            list_label.Add(
                new CategoryValue
                {
                    label = sMonth
                });
        }

        var category =
            new FCCategory
            {
                category =
                    list_label.ToArray()
            };

        fc.chart = chart;
        fc.categories = category;
        fc.dataset = list_ds.ToArray();

        return Json(fc);
    }

    public async Task<IActionResult> _GraphMonthlyContainerByPlace(
        CancellationToken cancellationToken)
    {
        var fc =
            new FusionBarChart();

        var query =
            await dashboardRepo
                .GetMonthlyContainerByPlaceData(
                    cancellationToken);

        var list =
            new List<FCData>(
                query.Count);

        foreach (var item in query)
        {
            list.Add(
                new FCData
                {
                    label = item.name,
                    value = item.id_count.ToString()
                });
        }

        var chart =
            new FCBarAttribute
            {
                plotgradientcolor = "",
                showalternatehgridcolor = "0",
                showShadow = "0",
                showplotborder = "0",
                showvalues = "0",
                theme = "fint",
                divlinecolor = "CCCCCC",
                palettecolors =
                    "#b2d9f9,#f7c018,#94bf13,#ff9049,#069191,#d74a4a,#914b91,#5c882b,#0c93d8",
                toolTipBorderColor = "7d7d7d",
                toolTipBgColor = "7d7d7d",
                toolTipColor = "ffffff",
                showcanvasborder = "0",
                canvasborderalpha = "0",
                numberSuffix = " trip",
                anchorRadius = "4",
                anchorBorderThickness = "2",
                yaxisvaluespadding = "10",
                showborder = "0",
                outcnvbasefontsize = "12",
                showXAxisLine = "0"
            };

        fc.chart = chart;
        fc.data = list.ToArray();

        return Json(fc);
    }

    public async Task<IActionResult> TotalDetail(
        string code,
        string month,
        string year,
        CancellationToken cancellationToken)
    {
        string current_month =
            DateTime.Now.ToString("MM");

        string current_year =
            DateTime.Now.ToString("yyyy");

        month =
            month != null
                ? month
                : current_month;

        year =
            year != null
                ? year
                : current_year;

        var model =
            await dashboardRepo.GetTotalDetailData(
                code,
                month,
                year,
                cancellationToken);

        /*
         * CustomerRepository และ DropListLogRepository
         * ยังเป็น synchronous ตามไฟล์เดิม
         * จึงไม่สร้าง async method ที่ไม่มีอยู่จริง
         */
        var customer =
            await customerRepo.GetCustomerDataByCode(
                code);

        ViewBag.Customer =
            customer;

        ViewData["month"] =
            from a in dropListRepo.GetMonth()
            select new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label,
                Selected = a.key == month
            };

        ViewData["year"] =
            from a in dropListRepo.GetYear()
            select new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label,
                Selected = a.key == year
            };

        return View(model);
    }

    public async Task<IActionResult> OtherDetail(
        string month,
        string year,
        CancellationToken cancellationToken)
    {
        int d = 1;
        int m = Convert.ToInt32(month);
        int y = Convert.ToInt32(year);

        DateTime date =
            new(
                y,
                m,
                d);

        var model =
            await dashboardRepo
                .GetJobCostCustomerBySupplier(
                    month,
                    year,
                    cancellationToken);

        ViewBag.Date =
            date;

        return View(model);
    }

    public async Task<IActionResult> ListOtherDetail(
        string code,
        int month,
        int year,
        CancellationToken cancellationToken)
    {
        int d = 1;
        int m = Convert.ToInt32(month);
        int y = Convert.ToInt32(year);

        DateTime date =
            new(
                y,
                m,
                d);

        var customer =
            await customerRepo
                .GetCustomerByCode(
                    code);

        var data =
            await dashboardRepo
                .GetAdvanceOtherList(
                    code,
                    month.ToString(),
                    year.ToString(),
                    cancellationToken);

        ViewBag.Date =
            date;

        ViewBag.Customer =
            customer;

        return View(data);
    }

    public async Task<IActionResult> WorkingDetail(
        string code,
        CancellationToken cancellationToken)
    {
        var model =
            await dashboardRepo
                .GetWorkingDetailData(
                    code,
                    cancellationToken);

        return View(model);
    }

    public IActionResult _PendingPayment()
    {
        string year =
            DateTime.Now.ToString("yyyy");

        ViewData["year"] =
            from a in dropListRepo.GetYear()
            select new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label,
                Selected = a.key == year
            };

        return View();
    }

    public async Task<IActionResult> _PendingData(
        string year)
    {
        /*
         * ส่วนนี้ยังคง synchronous ตามเดิม
         * เพราะ BillingRepository ที่ส่งมายังไม่ได้มี async API
         * จึงไม่เปลี่ยน business logic / method contract เอง
         */
        var customer =
            await billingRepo.GetListCustomer(
                year);

        var list =
            new List<PendingBillViewModel>();

        foreach (var cs in customer)
        {
            var item =
                new PendingBillViewModel();

            var data = await billingRepo.GetListPending(
                    cs.customer_code,
                    year);

            item.customer_code =
                cs.customer_code;

            item.customer_name =
                cs.customer_name;

            foreach (var pd in data)
            {
                switch (pd.month)
                {
                    case "01":
                        item.month_01 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;

                    case "02":
                        item.month_02 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;

                    case "03":
                        item.month_03 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;

                    case "04":
                        item.month_04 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;

                    case "05":
                        item.month_05 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;

                    case "06":
                        item.month_06 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;

                    case "07":
                        item.month_07 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;

                    case "08":
                        item.month_08 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;

                    case "09":
                        item.month_09 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;

                    case "10":
                        item.month_10 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;

                    case "11":
                        item.month_11 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;

                    case "12":
                        item.month_12 =
                            pd.count_pending == null
                                ? 0
                                : pd.count_pending.Value;
                        break;
                }
            }

            list.Add(item);
        }

        return PartialView(list);
    }
}