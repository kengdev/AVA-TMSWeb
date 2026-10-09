using System.Data;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TMSWeb.Repositories;

public class OrderRepository
{
    private readonly DbavalogContext db;
    private readonly CodeRepository codeRepo;
    private readonly AuditLogService auditLogService;
    private readonly ILogger<OrderRepository> logger;

    public OrderRepository(
        DbavalogContext context,
        CodeRepository _codeRepo,
        AuditLogService _auditLogService,
        ILogger<OrderRepository> _logger)
    {
        db = context;
        codeRepo = _codeRepo;
        auditLogService = _auditLogService;
        logger = _logger;
    }

    public async Task<ViewJobListActive> GetOrderByHash(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.ViewJobListActives
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.HashCode == code, cancellationToken);

        return data!;
    }

    public async Task<ViewJobListActive> GetOrderByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.ViewJobListActives
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.JobDetailCode == code, cancellationToken);

        return data!;
    }

    public async Task<TblBudgetTransaction> GetLastestTransactionByJob(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await (
            from a in db.TblJobBudgetTransactions.AsNoTracking()
            join b in db.TblBudgetTransactions.AsNoTracking()
                on a.TransactionCode equals b.TransactionCode
            where a.JobDetailCode == code
            orderby b.CreateDate descending
            select b)
            .FirstOrDefaultAsync(cancellationToken);

        return data!;
    }

    public async Task<ViewJobListActive> GetOrderBySearchDetail(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.ViewJobListActives
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.JobDetailCode == code, cancellationToken);

        return data!;
    }

    public async Task<TblJobMaster> GetOrderDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.TblJobMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.JobCode == code, cancellationToken);

        return data!;
    }

    public async Task<TblJobPlace> GetOrderPlaceDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.TblJobPlaces
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.JobCode == code, cancellationToken);

        return data!;
    }

    public async Task<TblJobPlace> GetJobPlaceDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.TblJobPlaces
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.JobCode == code, cancellationToken);

        return data!;
    }

    public async Task<OrderPlaceViewModel> GetPlaceByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.TblPlaces
            .AsNoTracking()
            .Where(a => a.PlaceCode == code)
            .Select(a => new OrderPlaceViewModel
            {
                place_code = a.PlaceCode,
                place_name = a.PlaceName,
                place_type = a.PlaceType
            })
            .FirstOrDefaultAsync(cancellationToken);

        return data!;
    }

    public async Task<List<OrderModel>> GetOrderBookingList(
        CancellationToken cancellationToken = default)
    {
        var rawData = await (
            from a in db.TblJobPlaces.AsNoTracking()
            join b in db.TblPlaces.AsNoTracking()
                on a.MissionCode equals b.PlaceCode
            join c in db.TblJobMasters.AsNoTracking()
                on a.JobCode equals c.JobCode
            join d in db.TblCustomers.AsNoTracking()
                on c.CustomerCode equals d.CustomerCode
            join e in db.TblBindingGenerates.AsNoTracking()
                on c.JobCode equals e.JobCode
            where e.IsEnabled == false
            select new
            {
                c.JobCode,
                e.JobDetailCode,
                c.BookingNo,
                c.JobInvoice,
                d.CustomerAcronym,
                c.ContainerSizeName,
                c.ContainerType,
                c.ContainerAmount,
                c.JobDate,
                MissionName = b.PlaceName,
                c.JobType
            })
            .ToListAsync(cancellationToken);

        return rawData
            .Select(x => new OrderModel
            {
                job_code = x.JobCode,
                job_detail_code = x.JobDetailCode,
                booking_no = x.BookingNo,
                job_invoice = x.JobInvoice,
                customer_name = x.CustomerAcronym,
                container_size = x.ContainerSizeName,
                container_type = x.ContainerType,
                container_amount = x.ContainerAmount,
                job_date = x.JobDate.ToString("yyyy-MM-dd HH:mm"),
                job_date_value = x.JobDate,
                mission_name = x.MissionName,
                job_type = x.JobType
            })
            .ToList();
    }

    public async Task<CostModel> GetCustomerCostByCode(
        string cost_code,
        CancellationToken cancellationToken = default)
    {
        var query = await db.ViewCostListActives
            .AsNoTracking()
            .Where(a => a.CostCode == cost_code)
            .Select(a => new CostModel
            {
                cost_code = a.CostCode,
                cost_value = a.CostValue ?? 0,
                cost_remark = a.CostRemark,
                customer_code = a.CustomerCode,
                customer_name_th = a.CustomerNameTh,
                customer_acronym = a.CustomerAcronym,
                mission_code = a.MissionCode,
                mission_name = a.MissionName,
                mission_type = a.PlaceType,
                expressway_value = a.ExpresswayValue ?? 0
            })
            .FirstOrDefaultAsync(cancellationToken);

        return query!;
    }

    public async Task<List<TblJobOcean>> GetOceanJob(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblJobOceans
            .AsNoTracking()
            .Where(w => w.JobCode == code)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderPlaceViewModel> GetYardPortDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.TblPlaces
            .AsNoTracking()
            .Where(a => a.PlaceCode == code)
            .Select(a => new OrderPlaceViewModel
            {
                place_code = a.PlaceCode,
                place_name = a.PlaceName,
                place_type = a.PlaceType
            })
            .FirstOrDefaultAsync(cancellationToken);

        return data!;
    }

    public async Task<List<DriverViewModel>> GetDriver(
        CancellationToken cancellationToken = default)
    {
        return await db.ViewDriverListActives
            .AsNoTracking()
            .Select(a => new DriverViewModel
            {
                driver_code = a.DriverCode,
                fullname = a.DriverFullname
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DriverViewModel> GetTruckDriverByTruckCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.ViewDriverListActives
            .AsNoTracking()
            .Where(a => a.TruckCode == code)
            .Select(a => new DriverViewModel
            {
                driver_code = a.DriverCode,
                fullname = a.DriverFullname,
                truck_license = a.TruckLicense,
                truck_code = a.TruckCode
            })
            .FirstOrDefaultAsync(cancellationToken);

        return data!;
    }

    public async Task<List<TblJobImg>> GetOrderImagesByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblJobImgs
            .AsNoTracking()
            .Where(w => w.JobDetailCode == code)
            .ToListAsync(cancellationToken);
    }

    public async Task<TblBindingGenerate> Get2Short(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.TblBindingGenerates
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.JobDetailCode == code, cancellationToken);

        return data!;
    }

    // Explicit transaction retained here because this method performs
    // read-modify-write and custom code generation before SaveChanges.
    public async Task<ResponseViewModel> OrderTruck(
    TblJobDetail model,
    CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        await using var tr =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            string supplier_code = "";

            // =====================================================
            // Generate Codes
            // =====================================================
            var advanceCode =
                await codeRepo.GetJobAdvanceCode(
                    cancellationToken);

            var cashingCode =
                await codeRepo.GetJobCashingCode(
                    cancellationToken);

            // =====================================================
            // Truck Data
            // =====================================================
            var truck =
                await (
                    from a in db.TblTrucks.AsNoTracking()

                    join b in db.TblTruckDetails.AsNoTracking()
                        on a.TruckCode equals b.TruckCode

                    where a.TruckCode == model.TruckCode

                    select new
                    {
                        supplier = a.SupplierCode,
                        truck_type = b.TruckTypegroupCode
                    })
                    .FirstOrDefaultAsync(
                        cancellationToken);

            // =====================================================
            // Job Master
            // =====================================================
            var job_master =
                await db.TblJobMasters.FindAsync(
                    new object?[]
                    {
                    model.JobCode
                    },
                    cancellationToken);

            if (job_master == null)
            {
                throw new InvalidOperationException(
                    $"Job master '{model.JobCode}' not found.");
            }

            job_master.LiftedAmount =
                job_master.LiftedAmount + 1;

            job_master.TruckTypegroupCode =
                truck != null
                    ? truck.truck_type
                    : "04";

            // =====================================================
            // Job Detail
            // =====================================================
            model.SupplierInvoice =
                model.SupplierCode != "SP18001"
                    ? "-"
                    : model.JobDetailCode;

            model.ContainerNo = "-";
            model.SealNo = "-";
            model.TareWeight = 0;
            model.JobStatus = "W";
            model.TrackingStatus = "PN";
            model.TrailerLicense = "-";

            model.IsCanceled = false;
            model.IsDocument = false;
            model.IsBilling = false;
            model.IsPayment = false;
            model.IsAdvancePaid = false;

            db.TblJobDetails.Add(model);

            // =====================================================
            // Job Place
            // ใช้ tracked entity ตัวเดียว
            // =====================================================
            var jobPlace =
                await db.TblJobPlaces
                    .FirstOrDefaultAsync(
                        w => w.JobCode == model.JobCode,
                        cancellationToken);

            if (jobPlace == null)
            {
                throw new InvalidOperationException(
                    $"Job place for '{model.JobCode}' not found.");
            }

            // =====================================================
            // Fuel
            // =====================================================
            int fuel = 0;

            if (truck != null)
            {
                fuel =
                    await GetFuelRate(
                        jobPlace.MissionCode,
                        truck.truck_type,
                        cancellationToken);
            }

            jobPlace.FuelValue =
                fuel;

            var now =
                DateTime.Now;

            // =====================================================
            // Job Advance
            // =====================================================
            var item_advance =
                new TblJobadvance
                {
                    AdvanceCode =
                        advanceCode,

                    JobDetailCode =
                        model.JobDetailCode,

                    AdvanceLifton =
                        jobPlace.LiftonValue,

                    AdvanceLiftoff =
                        jobPlace.LiftoffValue,

                    AdvanceYard =
                        jobPlace.YardValue,

                    AdvancePort =
                        jobPlace.PortValue,

                    AdvanceTotal =
                        jobPlace.LiftonValue +
                        jobPlace.LiftoffValue +
                        jobPlace.YardValue +
                        jobPlace.PortValue,

                    AdvanceLiftonup = 0,
                    AdvanceLiftoffup = 0,
                    AdvanceYardup = 0,
                    AdvancePortup = 0,
                    AdvanceTotalup = 0,

                    AdvanceRemark = "",

                    IsBilling = false,
                    IsClearing = false,
                    ClearingDate = null,

                    IsEnabled = true,

                    CreateDate = now,
                    CreateBy = model.CreateBy,

                    UpdateDate = now,
                    UpdateBy = model.CreateBy
                };

            db.TblJobadvances.Add(
                item_advance);

            // =====================================================
            // Job Cashing
            // =====================================================
            var item_cashing =
                new TblJobcashing
                {
                    CashingCode =
                        cashingCode,

                    JobDetailCode =
                        model.JobDetailCode,

                    CashingFuel =
                        fuel,

                    CashingFuelup = 0,

                    CashingExpressway =
                        jobPlace.ExpresswayValue,

                    CashingExpresswayup = 0,

                    CashingWaste = 0,

                    CashingTotal =
                        fuel + jobPlace.ExpresswayValue,

                    CashingRemark = "-",

                    IsEnabled =
                        model.SupplierCode == "SP18001",

                    CreateDate = now,
                    CreateBy = model.CreateBy,

                    UpdateDate = now,
                    UpdateBy = model.CreateBy
                };

            db.TblJobcashings.Add(
                item_cashing);

            // =====================================================
            // Driver Cost
            // =====================================================
            if (model.SupplierCode == "SP18001")
            {
                if (truck == null)
                {
                    throw new InvalidOperationException(
                        $"Truck '{model.TruckCode}' not found.");
                }

                supplier_code =
                    truck.supplier;

                decimal wage =
                    await GetDriverCost(
                        jobPlace.MissionCode,
                        cancellationToken);

                var driverCost =
                    new TblDriverCost
                    {
                        JobDetailCode =
                            model.JobDetailCode,

                        DriverCode =
                            model.DriverCode,

                        TripValue =
                            wage,

                        FuelValue = 0,
                        ExpresswayValue = 0,

                        CostType = "A"
                    };

                db.TblDriverCosts.Add(
                    driverCost);
            }
            else
            {
                supplier_code =
                    model.SupplierCode;
            }

            // =====================================================
            // Supplier Allowance
            // =====================================================
            decimal supplyer_trip =
                await GetSupplierCost(
                    supplier_code,
                    job_master.TruckTypegroupCode,
                    model.MissionPlaceCode,
                    cancellationToken);

            var supplierAllowance =
                new TblSupplierAllowance
                {
                    SupplierCode =
                        model.SupplierCode,

                    JobDetailCode =
                        model.JobDetailCode,

                    CostValue =
                        supplyer_trip
                };

            db.TblSupplierAllowances.Add(
                supplierAllowance);

            // =====================================================
            // Binding
            // =====================================================
            var binding =
                await db.TblBindingGenerates.FindAsync(
                    new object?[]
                    {
                    model.JobCode,
                    model.JobDetailCode
                    },
                    cancellationToken);

            if (binding == null)
            {
                throw new InvalidOperationException(
                    $"Binding '{model.JobCode}/{model.JobDetailCode}' not found.");
            }

            binding.IsEnabled =
                true;

            binding.UpdateDate =
                now;

            binding.UpdateBy =
                model.CreateBy;

            // =====================================================
            // Audit Values
            //
            // สำคัญ:
            // ห้ามส่ง EF Entity ทั้ง model เข้า JSON ตรง ๆ
            // เพราะ Navigation Property อาจวนเป็น Object Cycle
            //
            // ดึงเฉพาะ Scalar Property ที่ map กับ Column
            // =====================================================
            var newValues =
                db.Entry(model)
                    .Properties
                    .ToDictionary(
                        p => p.Metadata.Name,
                        p => p.CurrentValue);

            // =====================================================
            // Audit Log
            // =====================================================
            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Job",

                    ActionName =
                        AuditActions.Create,

                    EntityName =
                        nameof(TblJobDetail),

                    EntityId =
                        model.JobDetailCode,

                    Description =
                        "Add truck to job.",

                    NewValues =
                        newValues,

                    Result =
                        AuditResults.Success
                },
                cancellationToken);

            // =====================================================
            // Save
            // =====================================================
            await db.SaveChangesAsync(
                cancellationToken);

            // =====================================================
            // Commit
            // =====================================================
            await tr.CommitAsync(
                cancellationToken);

            response.result =
                true;

            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            // =====================================================
            // Rollback
            // =====================================================
            await tr.RollbackAsync(
                CancellationToken.None);

            response.result =
                false;

            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            logger.LogError(
                ex,
                "Error ordering truck for job {JobCode}",
                model.JobCode);
        }

        return response;
    }

    // Transaction retained because JobCode/JobDetailCode are generated
    // from CodeRepository before the insert is committed.
    public async Task<ResponseViewModel> AddContainerOrder(
        TblJobMaster orderToAdd,
        OrderPlaceModel placeJob,
        string txtOther,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        if (orderToAdd.ContainerAmount <= 0)
        {
            response.result = false;
            response.resultMessage = "จำนวนตู้ต้องมากกว่า 0";
            return response;
        }

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var container = await db.TblContainerSizes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.ContainerSizeCode == orderToAdd.ContainerSizeCode,
                    cancellationToken);

            if (container == null)
            {
                throw new InvalidOperationException(
                    $"Container size '{orderToAdd.ContainerSizeCode}' not found.");
            }

            var customerCost = await db.TblCustomerCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.CostCode == orderToAdd.CostCode,
                    cancellationToken);

            if (customerCost == null)
            {
                throw new InvalidOperationException(
                    $"Customer cost '{orderToAdd.CostCode}' not found.");
            }

            var payment = await db.TblCustomerRules
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.CustomerCode == customerCost.CustomerCode,
                    cancellationToken);

            if (payment == null)
            {
                throw new InvalidOperationException(
                    $"Customer rule for '{customerCost.CustomerCode}' not found.");
            }

            orderToAdd.JobCode = await codeRepo.GetJobCode(cancellationToken);

            var jobDetailCodes = await codeRepo.GetJobDetailCodes(
                placeJob.acronym,
                orderToAdd.ContainerAmount,
                cancellationToken);

            if (jobDetailCodes.Count() < orderToAdd.ContainerAmount)
            {
                throw new InvalidOperationException(
                    "Generated job-detail codes are fewer than ContainerAmount.");
            }

            orderToAdd.JobFinish = orderToAdd.JobDate;
            orderToAdd.CustomerCode = customerCost.CustomerCode;
            orderToAdd.ContainerSizeName =
                orderToAdd.ContainerSizeCode == "06"
                    ? "20 GP"
                    : orderToAdd.ContainerSizeCode == "99"
                        ? txtOther
                        : container.ContainerSizeName;
            orderToAdd.JobRemark = orderToAdd.JobRemark ?? "-";
            orderToAdd.ContainerType =
                orderToAdd.JobType == "IMPORT" ? "LADEN" : "EMPTY";
            orderToAdd.JobInvoice = NormalizeUpper(orderToAdd.JobInvoice);
            orderToAdd.BookingNo = NormalizeUpper(orderToAdd.BookingNo);

            db.TblJobMasters.Add(orderToAdd);

            var place = new TblJobPlace
            {
                StartCode = placeJob.txtStartCode,
                MissionCode = customerCost.MissionCode,
                FinishCode = placeJob.txtFinishCode ?? "000000000",
                JobCode = orderToAdd.JobCode
            };

            var cashing = await GetPlaceCashing(
                place.MissionCode,
                orderToAdd.TruckTypegroupCode,
                cancellationToken);

            if (orderToAdd.JobType == "IMPORT")
            {
                var startYardPort = await GetYardPort(
                    place.StartCode,
                    cancellationToken);

                if (place.FinishCode == "000000000")
                {
                    place.LiftoffValue = 0;
                    place.LiftonValue = 0;
                    place.YardValue = 0;
                    place.PortValue = payment.IsPort == true &&
                                      placeJob.chkStartPortYard == true
                        ? startYardPort.ImportFee
                        : 0;
                }
                else
                {
                    var finishYardPort = await GetYardPort(
                        place.FinishCode,
                        cancellationToken);

                    var lift = await GetYardLift(
                        place.FinishCode,
                        cancellationToken);

                    place.LiftoffValue = 0;
                    place.LiftonValue = payment.IsLifton == true
                        ? container.ContainerSizeGroup == "S"
                            ? lift.ShortliftoffFee
                            : lift.LongliftoffFee
                        : 0;
                    place.YardValue = payment.IsYard == true
                        ? finishYardPort.ImportFee
                        : 0;
                    place.PortValue = payment.IsPort == true &&
                                      placeJob.chkStartPortYard == true
                        ? startYardPort.ImportFee
                        : 0;
                }
            }
            else
            {
                var startYardPort = await GetYardPort(
                    place.StartCode,
                    cancellationToken);

                var finishYardPort = await GetYardPort(
                    place.FinishCode,
                    cancellationToken);

                var lifton = await GetYardLift(
                    place.StartCode,
                    cancellationToken);

                var liftoff = await GetPortLift(
                    place.FinishCode,
                    orderToAdd.ContainerSizeCode,
                    cancellationToken);

                place.LiftonValue = payment.IsLifton == true
                    ? container.ContainerSizeGroup == "S"
                        ? lifton.ShortliftonFee
                        : lifton.LongliftonFee
                    : 0;
                place.LiftoffValue = payment.IsLiftoff == true
                    ? liftoff.ContainerFee
                    : 0;
                place.YardValue = payment.IsYard == true &&
                                  placeJob.chkStartPortYard == true
                    ? startYardPort.ImportFee
                    : 0;
                place.PortValue = payment.IsPort == true
                    ? finishYardPort.ExportFee
                    : 0;
            }

            place.IsStartyardport = placeJob.chkStartPortYard;
            place.IsFinishyardport = placeJob.chkFinishPortYard;
            place.ExpresswayValue = placeJob.expressway;
            place.FuelValue = cashing.fuel_value;

            db.TblJobPlaces.Add(place);

            var now = DateTime.Now;
            var bindings = new List<TblBindingGenerate>(orderToAdd.ContainerAmount);

            for (var i = 1; i <= orderToAdd.ContainerAmount; i++)
            {
                bindings.Add(new TblBindingGenerate
                {
                    IndexCode = i,
                    Is2short =
                        orderToAdd.ContainerSizeCode == "06" &&
                        i % 2 == 0,
                    JobCode = orderToAdd.JobCode,
                    JobDetailCode = jobDetailCodes[i - 1],
                    CreateDate = now,
                    CreateBy = orderToAdd.CreateBy,
                    UpdateDate = now,
                    UpdateBy = orderToAdd.CreateBy
                });
            }

            db.TblBindingGenerates.AddRange(bindings);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (OperationCanceledException)
        {
            await tr.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            await tr.RollbackAsync(CancellationToken.None);

            logger.LogError(
                ex,
                "Error adding container order for cost {CostCode}",
                orderToAdd.CostCode);

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
        }

        return response;
    }

    public async Task<ResponseViewModel> AddFloorOrder(
        TblJobMaster orderToAdd,
        OrderPlaceModel placeJob,
        string txtOther,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        if (orderToAdd.ContainerAmount <= 0)
        {
            response.result = false;
            response.resultMessage = "จำนวนเที่ยวต้องมากกว่า 0";
            return response;
        }

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var customerCost = await db.TblCustomerCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.CostCode == orderToAdd.CostCode,
                    cancellationToken);

            if (customerCost == null)
            {
                throw new InvalidOperationException(
                    $"Customer cost '{orderToAdd.CostCode}' not found.");
            }

            orderToAdd.JobCode = await codeRepo.GetJobCode(cancellationToken);

            var jobDetailCodes = await codeRepo.GetJobDetailCodes(
                placeJob.flooracronym,
                orderToAdd.ContainerAmount,
                cancellationToken);

            if (jobDetailCodes.Count() < orderToAdd.ContainerAmount)
            {
                throw new InvalidOperationException(
                    "Generated job-detail codes are fewer than ContainerAmount.");
            }

            orderToAdd.CustomerCode = customerCost.CustomerCode;
            orderToAdd.JobFinish = orderToAdd.JobDate;
            orderToAdd.JobRemark = orderToAdd.JobRemark ?? "-";
            orderToAdd.ContainerSizeCode = "11";
            orderToAdd.ContainerSizeName = "วางพื้น";
            orderToAdd.AgentCode = "1900";
            orderToAdd.JobInvoice = NormalizeUpper(orderToAdd.JobInvoice);
            orderToAdd.BookingNo = NormalizeUpper(orderToAdd.BookingNo);

            db.TblJobMasters.Add(orderToAdd);

            var startYardPort = await GetYardPort(
                placeJob.txtFloorStartCode,
                cancellationToken);

            var missionYardPort = await GetYardPort(
                placeJob.txtFloorMissionCode,
                cancellationToken);

            var startPort = placeJob.txtStartType == "PORT"
                ? startYardPort.ExportFee
                : 0;

            var missionPort = placeJob.txtMissionType == "PORT"
                ? missionYardPort.ExportFee
                : 0;

            var place = new TblJobPlace
            {
                StartCode = placeJob.txtFloorStartCode,
                MissionCode = customerCost.MissionCode,
                FinishCode = "000000000",
                JobCode = orderToAdd.JobCode,
                ExpresswayValue = placeJob.expressway,
                FuelValue = 0,
                LiftonValue = 0,
                LiftoffValue = 0,
                YardValue = 0,
                PortValue = startPort + missionPort,
                IsStartyardport = true,
                IsFinishyardport = true
            };

            db.TblJobPlaces.Add(place);

            var now = DateTime.Now;
            var bindings = new List<TblBindingGenerate>(orderToAdd.ContainerAmount);

            for (var i = 1; i <= orderToAdd.ContainerAmount; i++)
            {
                bindings.Add(new TblBindingGenerate
                {
                    IndexCode = i,
                    Is2short = false,
                    JobCode = orderToAdd.JobCode,
                    JobDetailCode = jobDetailCodes[i - 1],
                    CreateDate = now,
                    CreateBy = orderToAdd.CreateBy,
                    UpdateDate = now,
                    UpdateBy = orderToAdd.UpdateBy
                });
            }

            db.TblBindingGenerates.AddRange(bindings);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (OperationCanceledException)
        {
            await tr.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            await tr.RollbackAsync(CancellationToken.None);

            logger.LogError(
                ex,
                "Error adding floor order for cost {CostCode}",
                orderToAdd.CostCode);

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
        }

        return response;
    }

    public async Task<ResponseViewModel> AddOceanOrder(
        TblJobMaster orderToAdd,
        OrderPlaceModel placeJob,
        OrderOceanModel oceanJob,
        string txtOther,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        if (orderToAdd.ContainerAmount <= 0)
        {
            response.result = false;
            response.resultMessage = "จำนวนตู้ต้องมากกว่า 0";
            return response;
        }

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            ValidateOceanData(oceanJob);

            var container = await db.TblContainerSizes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.ContainerSizeCode == orderToAdd.ContainerSizeCode,
                    cancellationToken);

            if (container == null)
            {
                throw new InvalidOperationException(
                    $"Container size '{orderToAdd.ContainerSizeCode}' not found.");
            }

            var customerCost = await db.TblCustomerCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.CostCode == orderToAdd.CostCode,
                    cancellationToken);

            if (customerCost == null)
            {
                throw new InvalidOperationException(
                    $"Customer cost '{orderToAdd.CostCode}' not found.");
            }

            var payment = await db.TblCustomerRules
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.CustomerCode == customerCost.CustomerCode,
                    cancellationToken);

            if (payment == null)
            {
                throw new InvalidOperationException(
                    $"Customer rule for '{customerCost.CustomerCode}' not found.");
            }

            orderToAdd.JobCode = await codeRepo.GetJobCode(cancellationToken);

            var jobDetailCodes = await codeRepo.GetJobDetailCodes(
                placeJob.oceanacronym,
                orderToAdd.ContainerAmount,
                cancellationToken);

            if (jobDetailCodes.Count() < orderToAdd.ContainerAmount)
            {
                throw new InvalidOperationException(
                    "Generated job-detail codes are fewer than ContainerAmount.");
            }

            orderToAdd.CustomerCode = customerCost.CustomerCode;
            orderToAdd.ContainerSizeName =
                orderToAdd.ContainerSizeCode == "06"
                    ? "20 GP"
                    : orderToAdd.ContainerSizeCode == "99"
                        ? txtOther
                        : container.ContainerSizeName;
            orderToAdd.JobRemark = orderToAdd.JobRemark ?? "-";
            orderToAdd.ContainerType =
                orderToAdd.JobType == "IMPORT" ? "LADEN" : "EMPTY";

            var place = new TblJobPlace
            {
                StartCode = placeJob.txtOceanStartCode,
                MissionCode = customerCost.MissionCode,
                FinishCode = placeJob.txtOceanFinishCode ?? "000000000",
                JobCode = orderToAdd.JobCode
            };

            var cashing = await GetPlaceCashing(
                place.MissionCode,
                orderToAdd.TruckTypegroupCode,
                cancellationToken);

            if (orderToAdd.JobType == "IMPORT" || orderToAdd.JobType == "OCEAN")
            {
                var startYardPort = await GetYardPort(
                    place.StartCode,
                    cancellationToken);

                if (place.FinishCode == "000000000")
                {
                    place.LiftoffValue = 0;
                    place.LiftonValue = 0;
                    place.YardValue = 0;
                    place.PortValue = payment.IsPort == true &&
                                      placeJob.chkStartPortYard == true
                        ? startYardPort.ImportFee
                        : 0;
                }
                else
                {
                    var finishYardPort = await GetYardPort(
                        place.FinishCode,
                        cancellationToken);

                    var lift = await GetYardLift(
                        place.FinishCode,
                        cancellationToken);

                    place.LiftoffValue = 0;
                    place.LiftonValue = payment.IsLifton == true
                        ? container.ContainerSizeGroup == "S"
                            ? lift.ShortliftoffFee
                            : lift.LongliftoffFee
                        : 0;
                    place.YardValue = payment.IsYard == true
                        ? finishYardPort.ImportFee
                        : 0;
                    place.PortValue = payment.IsPort == true &&
                                      placeJob.chkStartPortYard == true
                        ? startYardPort.ImportFee
                        : 0;
                }
            }
            else
            {
                var startYardPort = await GetYardPort(
                    place.StartCode,
                    cancellationToken);

                var finishYardPort = await GetYardPort(
                    place.FinishCode,
                    cancellationToken);

                var lifton = await GetYardLift(
                    place.StartCode,
                    cancellationToken);

                var liftoff = await GetPortLift(
                    place.FinishCode,
                    orderToAdd.ContainerSizeCode,
                    cancellationToken);

                place.LiftonValue = payment.IsLifton == true
                    ? container.ContainerSizeGroup == "S"
                        ? lifton.ShortliftonFee
                        : lifton.LongliftonFee
                    : 0;
                place.LiftoffValue = payment.IsLiftoff == true
                    ? liftoff.ContainerFee
                    : 0;
                place.YardValue = payment.IsYard == true &&
                                  placeJob.chkStartPortYard == true
                    ? startYardPort.ImportFee
                    : 0;
                place.PortValue = payment.IsPort == true
                    ? finishYardPort.ExportFee
                    : 0;
            }

            place.IsStartyardport = placeJob.chkStartPortYard;
            place.IsFinishyardport = placeJob.chkFinishPortYard;
            place.ExpresswayValue = placeJob.expressway;
            place.FuelValue = cashing.fuel_value;

            db.TblJobPlaces.Add(place);

            TblJobOcean? lastOcean = null;
            var oceanCount = oceanJob.iContainer.Count();

            for (var i = 0; i < oceanCount; i++)
            {
                if (string.IsNullOrWhiteSpace(oceanJob.iContainer[i]))
                {
                    continue;
                }

                var deliveryDate = oceanJob.iDelivery[i]
                    ?? throw new InvalidOperationException(
                        $"Delivery date at ocean row {i + 1} is required.");

                lastOcean = new TblJobOcean
                {
                    JobCode = orderToAdd.JobCode,
                    BlNo = NormalizeUpper(oceanJob.iBL[i]),
                    ContainerNo = NormalizeUpper(oceanJob.iContainer[i]),
                    OrderQuantity = oceanJob.iQuantity[i],
                    OrderWeight = oceanJob.iWeight[i],
                    OrderCbm = oceanJob.iCBM[i],
                    DeliveryDate = deliveryDate,
                    EtaDate = oceanJob.eta_date,
                    VesselName = oceanJob.vessel,
                    ChangeNo = NormalizeUpper(oceanJob.change_no),
                    PlaceCode = oceanJob.place_code,
                    PickupDate = oceanJob.pickup_date,
                    ConsigneeName = oceanJob.consignee_name,
                    ShipperName = oceanJob.shipper_name,
                    ShippingName = oceanJob.shipping_name,
                    GoodsDeclarationNo =
                        string.IsNullOrWhiteSpace(oceanJob.goods_declaration)
                            ? "-"
                            : oceanJob.goods_declaration.Trim(),
                    RequestName = oceanJob.request_name,
                    Description = oceanJob.description
                };

                db.TblJobOceans.Add(lastOcean);
            }

            if (lastOcean == null)
            {
                throw new InvalidOperationException(
                    "At least one ocean container is required.");
            }

            orderToAdd.JobInvoice = NormalizeUpper(orderToAdd.JobInvoice);
            // Preserve the original business rule: master BookingNo/JobDate
            // are taken from the last valid ocean row.
            orderToAdd.BookingNo = lastOcean.BlNo;
            orderToAdd.JobDate = lastOcean.DeliveryDate;
            orderToAdd.JobFinish = orderToAdd.JobDate;

            db.TblJobMasters.Add(orderToAdd);

            var now = DateTime.Now;
            var bindings = new List<TblBindingGenerate>(orderToAdd.ContainerAmount);

            for (var i = 1; i <= orderToAdd.ContainerAmount; i++)
            {
                bindings.Add(new TblBindingGenerate
                {
                    IndexCode = i,
                    Is2short =
                        orderToAdd.ContainerSizeCode == "06" &&
                        i % 2 == 0,
                    JobCode = orderToAdd.JobCode,
                    JobDetailCode = jobDetailCodes[i - 1],
                    CreateDate = now,
                    CreateBy = orderToAdd.CreateBy,
                    UpdateDate = now,
                    UpdateBy = orderToAdd.CreateBy
                });
            }

            db.TblBindingGenerates.AddRange(bindings);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (OperationCanceledException)
        {
            await tr.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            await tr.RollbackAsync(CancellationToken.None);

            logger.LogError(
                ex,
                "Error adding ocean order for cost {CostCode}",
                orderToAdd.CostCode);

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
        }

        return response;
    }

    public async Task<ResponseViewModel> AddDomesticOrder(
        TblJobMaster orderToAdd,
        OrderPlaceModel placeJob,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        if (orderToAdd.ContainerAmount <= 0)
        {
            response.result = false;
            response.resultMessage = "จำนวนเที่ยวต้องมากกว่า 0";
            return response;
        }

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            orderToAdd.JobCode = await codeRepo.GetJobCode(cancellationToken);

            var jobDetailCodes = await codeRepo.GetJobDetailCodes(
                placeJob.domesticacronym,
                orderToAdd.ContainerAmount,
                cancellationToken);

            if (jobDetailCodes.Count() < orderToAdd.ContainerAmount)
            {
                throw new InvalidOperationException(
                    "Generated job-detail codes are fewer than ContainerAmount.");
            }

            orderToAdd.JobFinish = orderToAdd.JobDate;
            orderToAdd.JobRemark = orderToAdd.JobRemark ?? "-";
            orderToAdd.ContainerSizeCode = "99";
            orderToAdd.ContainerSizeName = "Domestic";
            orderToAdd.AgentCode = "1900";
            orderToAdd.JobInvoice = NormalizeUpper(orderToAdd.JobInvoice);
            orderToAdd.BookingNo = "-";

            db.TblJobMasters.Add(orderToAdd);

            const int startPort = 35;
            const int missionPort = 0;

            db.TblJobPlaces.Add(new TblJobPlace
            {
                StartCode = placeJob.txtDomesticStartCode,
                MissionCode = placeJob.route_code,
                FinishCode = "000000000",
                JobCode = orderToAdd.JobCode,
                ExpresswayValue = placeJob.expressway,
                FuelValue = placeJob.fuel,
                LiftonValue = 0,
                LiftoffValue = 0,
                YardValue = 0,
                PortValue = startPort + missionPort,
                IsStartyardport = true,
                IsFinishyardport = true
            });

            var now = DateTime.Now;
            var bindings = new List<TblBindingGenerate>(orderToAdd.ContainerAmount);

            for (var i = 1; i <= orderToAdd.ContainerAmount; i++)
            {
                bindings.Add(new TblBindingGenerate
                {
                    IndexCode = i,
                    Is2short = false,
                    JobCode = orderToAdd.JobCode,
                    JobDetailCode = jobDetailCodes[i - 1],
                    CreateDate = now,
                    CreateBy = orderToAdd.CreateBy,
                    UpdateDate = now,
                    UpdateBy = orderToAdd.UpdateBy
                });
            }

            db.TblBindingGenerates.AddRange(bindings);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (OperationCanceledException)
        {
            await tr.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            await tr.RollbackAsync(CancellationToken.None);

            logger.LogError(
                ex,
                "Error adding domestic order");

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
        }

        return response;
    }

    // No explicit transaction: all entity changes are persisted by one SaveChangesAsync.
    public async Task<ResponseViewModel> EditContainerOrder(
        TblJobMaster orderToEdit,
        OrderPlaceModel placeJob,
        string txtOther,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var jobCode = orderToEdit.JobCode;

            var currentJob = await db.TblJobMasters
                .FirstOrDefaultAsync(
                    w => w.JobCode == jobCode,
                    cancellationToken);

            if (currentJob == null)
            {
                throw new InvalidOperationException(
                    $"Job master '{jobCode}' not found.");
            }

            // Snapshot only scalar EF properties. This prevents object-cycle
            // problems from navigation properties in AuditLog serialization.
            var oldMasterValues = GetScalarValues(currentJob);

            var customerCost = await db.TblCustomerCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.CostCode == orderToEdit.CostCode,
                    cancellationToken);

            if (customerCost == null)
            {
                throw new InvalidOperationException(
                    $"Customer cost '{orderToEdit.CostCode}' not found.");
            }

            // CustomerCode must come from the selected cost record so that
            // the payment rule cannot accidentally come from another customer.
            var payment = await db.TblCustomerRules
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.CustomerCode == customerCost.CustomerCode,
                    cancellationToken);

            if (payment == null)
            {
                throw new InvalidOperationException(
                    $"Customer rule for '{customerCost.CustomerCode}' not found.");
            }

            var container = await db.TblContainerSizes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.ContainerSizeCode == orderToEdit.ContainerSizeCode,
                    cancellationToken);

            if (container == null)
            {
                throw new InvalidOperationException(
                    $"Container size '{orderToEdit.ContainerSizeCode}' not found.");
            }

            var place = await db.TblJobPlaces
                .FirstOrDefaultAsync(
                    w => w.JobCode == jobCode,
                    cancellationToken);

            if (place == null)
            {
                throw new InvalidOperationException(
                    $"Job place for '{jobCode}' not found.");
            }

            place.StartCode = placeJob.txtStartCode;
            place.MissionCode = customerCost.MissionCode;
            place.FinishCode = placeJob.txtFinishCode ?? "000000000";
            place.JobCode = jobCode;

            var cashing = await GetPlaceCashing(
                place.MissionCode,
                orderToEdit.TruckTypegroupCode,
                cancellationToken);

            if (orderToEdit.JobType == "IMPORT")
            {
                var startYardPort = await GetYardPort(
                    place.StartCode,
                    cancellationToken);

                if (place.FinishCode == "000000000")
                {
                    place.LiftoffValue = 0;
                    place.LiftonValue = 0;
                    place.YardValue = 0;
                    place.PortValue = payment.IsPort == true &&
                                      placeJob.chkStartPortYard == true
                        ? startYardPort.ImportFee
                        : 0;
                }
                else
                {
                    var finishYardPort = await GetYardPort(
                        place.FinishCode,
                        cancellationToken);

                    var lift = await GetYardLift(
                        place.FinishCode,
                        cancellationToken);

                    place.LiftoffValue = 0;
                    place.LiftonValue = payment.IsLifton == true
                        ? container != null && container.ContainerSizeGroup == "S"
                            ? lift.ShortliftoffFee
                            : lift.LongliftoffFee
                        : 0;

                    place.YardValue = payment.IsYard == true
                        ? finishYardPort.ImportFee
                        : 0;

                    place.PortValue = payment.IsPort == true &&
                                      placeJob.chkStartPortYard == true
                        ? startYardPort.ImportFee
                        : 0;
                }
            }
            else
            {
                var startYardPort = await GetYardPort(
                    place.StartCode,
                    cancellationToken);

                var finishYardPort = await GetYardPort(
                    place.FinishCode,
                    cancellationToken);

                var lifton = await GetYardLift(
                    place.StartCode,
                    cancellationToken);

                var liftoff = await GetPortLift(
                    place.FinishCode,
                    orderToEdit.ContainerSizeCode,
                    cancellationToken);

                place.LiftonValue = payment.IsLifton == true
                    ? container != null && container.ContainerSizeGroup == "S"
                        ? lifton.ShortliftonFee
                        : lifton.LongliftonFee
                    : 0;

                place.LiftoffValue = payment.IsLiftoff == true
                    ? liftoff.ContainerFee
                    : 0;

                place.YardValue = payment.IsYard == true &&
                                  placeJob.chkStartPortYard == true
                    ? startYardPort.ImportFee
                    : 0;

                place.PortValue = payment.IsPort == true
                    ? finishYardPort.ExportFee
                    : 0;
            }

            place.IsStartyardport = placeJob.chkStartPortYard;
            place.IsFinishyardport = placeJob.chkFinishPortYard;
            place.ExpresswayValue = placeJob.expressway;
            place.FuelValue = cashing.fuel_value;

            var details = await db.TblJobDetails
                .Where(w => w.JobCode == jobCode)
                .ToListAsync(cancellationToken);

            var oldDetailValues = details
                .Select(GetScalarValues)
                .ToList();

            var detailCodes = details
                .Select(x => x.JobDetailCode)
                .ToList();

            var advances = await db.TblJobadvances
                .Where(w => detailCodes.Contains(w.JobDetailCode))
                .ToListAsync(cancellationToken);

            var jobCashings = await db.TblJobcashings
                .Where(w => detailCodes.Contains(w.JobDetailCode))
                .ToListAsync(cancellationToken);

            var advanceMap = advances.ToDictionary(
                x => x.JobDetailCode,
                StringComparer.OrdinalIgnoreCase);

            var cashingMap = jobCashings.ToDictionary(
                x => x.JobDetailCode,
                StringComparer.OrdinalIgnoreCase);

            var now = DateTime.Now;

            foreach (var item in details)
            {
                item.StartPlaceCode = placeJob.txtStartCode;
                item.MissionPlaceCode = placeJob.txtMissionCode;
                item.FinishPlaceCode = placeJob.txtFinishCode;
                item.CostValue = customerCost.CostValue;
                item.UpdateDate = now;
                item.UpdateBy = orderToEdit.UpdateBy;

                if (!advanceMap.TryGetValue(item.JobDetailCode, out var advance))
                {
                    throw new InvalidOperationException(
                        $"Advance for job detail '{item.JobDetailCode}' not found.");
                }

                advance.AdvanceLifton = place.LiftonValue;
                advance.AdvanceLiftoff = place.LiftoffValue;
                advance.AdvancePort = place.PortValue;
                advance.AdvanceYard = place.YardValue;
                advance.AdvanceTotal =
                    advance.AdvanceLifton +
                    advance.AdvanceLiftoff +
                    advance.AdvancePort +
                    advance.AdvanceYard;
                advance.UpdateDate = now;
                advance.UpdateBy = orderToEdit.UpdateBy;

                if (!cashingMap.TryGetValue(item.JobDetailCode, out var jobCashing))
                {
                    throw new InvalidOperationException(
                        $"Cashing for job detail '{item.JobDetailCode}' not found.");
                }

                jobCashing.CashingFuel = place.FuelValue;
                jobCashing.CashingExpressway = place.ExpresswayValue;
                jobCashing.CashingTotal =
                    place.FuelValue + place.ExpresswayValue;
                jobCashing.IsEnabled = item.SupplierCode == "SP18001";
                jobCashing.UpdateDate = now;
                jobCashing.UpdateBy = orderToEdit.UpdateBy;
            }

            orderToEdit.CustomerCode = customerCost.CustomerCode;
            orderToEdit.ContainerType =
                orderToEdit.JobType == "IMPORT" ? "LADEN" : "EMPTY";
            orderToEdit.JobFinish = orderToEdit.JobDate;

            if (orderToEdit.ContainerSizeCode == "06")
            {
                orderToEdit.ContainerSizeName = "20 GP";
            }
            else
            {
                orderToEdit.ContainerSizeName =
                    orderToEdit.ContainerSizeCode == "99"
                        ? txtOther
                        : container.ContainerSizeName;
            }

            orderToEdit.JobInvoice = NormalizeUpper(orderToEdit.JobInvoice);
            orderToEdit.BookingNo = NormalizeUpper(orderToEdit.BookingNo);

            // Keep a tracked master instead of attaching the posted entity with Update().
            // SetValues copies scalar values only; navigation properties are not copied.
            ApplyJobMasterValues(currentJob, orderToEdit);
            currentJob.UpdateDate = now;
            currentJob.UpdateBy = orderToEdit.UpdateBy;

            var bindings = await db.TblBindingGenerates
                .Where(w => w.JobCode == jobCode)
                .ToListAsync(cancellationToken);

            foreach (var item in bindings)
            {
                item.Is2short =
                    orderToEdit.ContainerSizeCode == "06" &&
                    item.IndexCode % 2 == 0;
            }

            var newMasterValues = GetScalarValues(currentJob);
            var newDetailValues = details
                .Select(GetScalarValues)
                .ToList();

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Job",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblJobMaster),
                    EntityId = jobCode,
                    Description = "Update job data.",
                    OldValues = new
                    {
                        Master = oldMasterValues,
                        Details = oldDetailValues
                    },
                    NewValues = new
                    {
                        Master = newMasterValues,
                        Details = newDetailValues
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error editing container order {JobCode}",
                orderToEdit.JobCode);

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
        }

        return response;
    }

    public async Task<ResponseViewModel> EditFloorOrder(
        TblJobMaster orderToEdit,
        OrderPlaceModel placeJob,
        string txtOther,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var jobCode = orderToEdit.JobCode;

            var currentJob = await db.TblJobMasters
                .FirstOrDefaultAsync(
                    w => w.JobCode == jobCode,
                    cancellationToken);

            if (currentJob == null)
            {
                throw new InvalidOperationException(
                    $"Job master '{jobCode}' not found.");
            }

            var oldMasterValues = GetScalarValues(currentJob);

            var customerCost = await db.TblCustomerCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.CostCode == orderToEdit.CostCode,
                    cancellationToken);

            if (customerCost == null)
            {
                throw new InvalidOperationException(
                    $"Customer cost '{orderToEdit.CostCode}' not found.");
            }

            var startYardPort = await GetYardPort(
                placeJob.txtFloorStartCode,
                cancellationToken);

            var missionYardPort = await GetYardPort(
                placeJob.txtFloorMissionCode,
                cancellationToken);

            var startPort = placeJob.txtStartType == "PORT"
                ? startYardPort.ExportFee
                : 0;

            var missionPort = placeJob.txtMissionType == "PORT"
                ? missionYardPort.ExportFee
                : 0;

            var place = await db.TblJobPlaces
                .FirstOrDefaultAsync(
                    w => w.JobCode == jobCode,
                    cancellationToken);

            if (place == null)
            {
                throw new InvalidOperationException(
                    $"Job place for '{jobCode}' not found.");
            }

            place.StartCode = placeJob.txtFloorStartCode;
            place.MissionCode = customerCost.MissionCode;
            place.FinishCode = "000000000";
            place.JobCode = jobCode;
            place.ExpresswayValue = placeJob.expressway;
            place.FuelValue = 0;
            place.LiftonValue = 0;
            place.LiftoffValue = 0;
            place.YardValue = 0;
            place.PortValue = startPort + missionPort;
            place.IsStartyardport = true;
            place.IsFinishyardport = true;

            var details = await db.TblJobDetails
                .Where(w => w.JobCode == jobCode)
                .ToListAsync(cancellationToken);

            var oldDetailValues = details
                .Select(GetScalarValues)
                .ToList();

            var detailCodes = details
                .Select(x => x.JobDetailCode)
                .ToList();

            var advances = await db.TblJobadvances
                .Where(w => detailCodes.Contains(w.JobDetailCode))
                .ToListAsync(cancellationToken);

            var jobCashings = await db.TblJobcashings
                .Where(w => detailCodes.Contains(w.JobDetailCode))
                .ToListAsync(cancellationToken);

            var advanceMap = advances.ToDictionary(
                x => x.JobDetailCode,
                StringComparer.OrdinalIgnoreCase);

            var cashingMap = jobCashings.ToDictionary(
                x => x.JobDetailCode,
                StringComparer.OrdinalIgnoreCase);

            var now = DateTime.Now;

            foreach (var item in details)
            {
                item.StartPlaceCode = placeJob.txtStartCode;
                item.MissionPlaceCode = placeJob.txtMissionCode;
                item.FinishPlaceCode = placeJob.txtFinishCode;
                item.CostValue = customerCost.CostValue;
                item.UpdateDate = now;
                item.UpdateBy = orderToEdit.UpdateBy;

                if (!advanceMap.TryGetValue(item.JobDetailCode, out var advance))
                {
                    throw new InvalidOperationException(
                        $"Advance for job detail '{item.JobDetailCode}' not found.");
                }

                advance.AdvanceLifton = place.LiftonValue;
                advance.AdvanceLiftoff = place.LiftoffValue;
                advance.AdvancePort = place.PortValue;
                advance.AdvanceYard = place.YardValue;
                advance.AdvanceTotal =
                    advance.AdvanceLifton +
                    advance.AdvanceLiftoff +
                    advance.AdvancePort +
                    advance.AdvanceYard;
                advance.UpdateDate = now;
                advance.UpdateBy = orderToEdit.UpdateBy;

                if (!cashingMap.TryGetValue(item.JobDetailCode, out var jobCashing))
                {
                    throw new InvalidOperationException(
                        $"Cashing for job detail '{item.JobDetailCode}' not found.");
                }

                jobCashing.CashingFuel = place.FuelValue;
                jobCashing.CashingExpressway = place.ExpresswayValue;
                jobCashing.CashingTotal =
                    place.FuelValue + place.ExpresswayValue;
                jobCashing.IsEnabled = item.SupplierCode == "SP18001";
                jobCashing.UpdateDate = now;
                jobCashing.UpdateBy = orderToEdit.UpdateBy;
            }

            orderToEdit.CustomerCode = customerCost.CustomerCode;
            orderToEdit.JobFinish = orderToEdit.JobDate;
            orderToEdit.JobInvoice = NormalizeUpper(orderToEdit.JobInvoice);
            orderToEdit.BookingNo = NormalizeUpper(orderToEdit.BookingNo);

            ApplyJobMasterValues(currentJob, orderToEdit);
            currentJob.UpdateDate = now;
            currentJob.UpdateBy = orderToEdit.UpdateBy;

            var newMasterValues = GetScalarValues(currentJob);
            var newDetailValues = details
                .Select(GetScalarValues)
                .ToList();

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Job",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblJobMaster),
                    EntityId = jobCode,
                    Description = "Update job data.",
                    OldValues = new
                    {
                        Master = oldMasterValues,
                        Details = oldDetailValues
                    },
                    NewValues = new
                    {
                        Master = newMasterValues,
                        Details = newDetailValues
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error editing floor order {JobCode}",
                orderToEdit.JobCode);

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
        }

        return response;
    }

    public async Task<ResponseViewModel> EditOceanOrder(
        TblJobMaster orderToEdit,
        OrderPlaceModel placeJob,
        OrderOceanModel oceanJob,
        string txtOther,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            ValidateOceanData(oceanJob);

            var jobCode = orderToEdit.JobCode;

            var currentJob = await db.TblJobMasters
                .FirstOrDefaultAsync(
                    w => w.JobCode == jobCode,
                    cancellationToken);

            if (currentJob == null)
            {
                throw new InvalidOperationException(
                    $"Job master '{jobCode}' not found.");
            }

            var oldMasterValues = GetScalarValues(currentJob);

            var customerCost = await db.TblCustomerCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.CostCode == orderToEdit.CostCode,
                    cancellationToken);

            if (customerCost == null)
            {
                throw new InvalidOperationException(
                    $"Customer cost '{orderToEdit.CostCode}' not found.");
            }

            var payment = await db.TblCustomerRules
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.CustomerCode == customerCost.CustomerCode,
                    cancellationToken);

            if (payment == null)
            {
                throw new InvalidOperationException(
                    $"Customer rule for '{customerCost.CustomerCode}' not found.");
            }

            var container = await db.TblContainerSizes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.ContainerSizeCode == orderToEdit.ContainerSizeCode,
                    cancellationToken);

            if (container == null)
            {
                throw new InvalidOperationException(
                    $"Container size '{orderToEdit.ContainerSizeCode}' not found.");
            }

            var place = await db.TblJobPlaces
                .FirstOrDefaultAsync(
                    w => w.JobCode == jobCode,
                    cancellationToken);

            if (place == null)
            {
                throw new InvalidOperationException(
                    $"Job place for '{jobCode}' not found.");
            }

            place.StartCode = placeJob.txtStartCode;
            place.MissionCode = customerCost.MissionCode;
            place.FinishCode = placeJob.txtFinishCode ?? "000000000";
            place.JobCode = jobCode;

            var cashing = await GetPlaceCashing(
                place.MissionCode,
                orderToEdit.TruckTypegroupCode,
                cancellationToken);

            var startYardPort = await GetYardPort(
                place.StartCode,
                cancellationToken);

            if (place.FinishCode == "000000000")
            {
                place.LiftoffValue = 0;
                place.LiftonValue = 0;
                place.YardValue = 0;
                place.PortValue = payment.IsPort == true &&
                                  placeJob.chkStartPortYard == true
                    ? startYardPort.ImportFee
                    : 0;
            }
            else
            {
                var finishYardPort = await GetYardPort(
                    place.FinishCode,
                    cancellationToken);

                var lift = await GetYardLift(
                    place.FinishCode,
                    cancellationToken);

                place.LiftoffValue = 0;
                place.LiftonValue = payment.IsLifton == true
                    ? container != null && container.ContainerSizeGroup == "S"
                        ? lift.ShortliftoffFee
                        : lift.LongliftoffFee
                    : 0;

                place.YardValue = payment.IsYard == true
                    ? finishYardPort.ImportFee
                    : 0;

                place.PortValue = payment.IsPort == true &&
                                  placeJob.chkStartPortYard == true
                    ? startYardPort.ImportFee
                    : 0;
            }

            place.IsStartyardport = placeJob.chkStartPortYard;
            place.IsFinishyardport = placeJob.chkFinishPortYard;
            place.ExpresswayValue = placeJob.expressway;
            place.FuelValue = cashing.fuel_value;

            var details = await db.TblJobDetails
                .Where(w => w.JobCode == jobCode)
                .ToListAsync(cancellationToken);

            var oldDetailValues = details
                .Select(GetScalarValues)
                .ToList();

            var detailCodes = details
                .Select(x => x.JobDetailCode)
                .ToList();

            var advances = await db.TblJobadvances
                .Where(w => detailCodes.Contains(w.JobDetailCode))
                .ToListAsync(cancellationToken);

            var jobCashings = await db.TblJobcashings
                .Where(w => detailCodes.Contains(w.JobDetailCode))
                .ToListAsync(cancellationToken);

            var advanceMap = advances.ToDictionary(
                x => x.JobDetailCode,
                StringComparer.OrdinalIgnoreCase);

            var cashingMap = jobCashings.ToDictionary(
                x => x.JobDetailCode,
                StringComparer.OrdinalIgnoreCase);

            var now = DateTime.Now;

            foreach (var item in details)
            {
                item.StartPlaceCode = placeJob.txtStartCode;
                item.MissionPlaceCode = placeJob.txtMissionCode;
                item.FinishPlaceCode = placeJob.txtFinishCode;
                item.CostValue = customerCost.CostValue;
                item.UpdateDate = now;
                item.UpdateBy = orderToEdit.UpdateBy;

                if (!advanceMap.TryGetValue(item.JobDetailCode, out var advance))
                {
                    throw new InvalidOperationException(
                        $"Advance for job detail '{item.JobDetailCode}' not found.");
                }

                advance.AdvanceLifton = place.LiftonValue;
                advance.AdvanceLiftoff = place.LiftoffValue;
                advance.AdvancePort = place.PortValue;
                advance.AdvanceYard = place.YardValue;
                advance.AdvanceTotal =
                    advance.AdvanceLifton +
                    advance.AdvanceLiftoff +
                    advance.AdvancePort +
                    advance.AdvanceYard;
                advance.UpdateDate = now;
                advance.UpdateBy = orderToEdit.UpdateBy;

                if (!cashingMap.TryGetValue(item.JobDetailCode, out var jobCashing))
                {
                    throw new InvalidOperationException(
                        $"Cashing for job detail '{item.JobDetailCode}' not found.");
                }

                jobCashing.CashingFuel = place.FuelValue;
                jobCashing.CashingExpressway = place.ExpresswayValue;
                jobCashing.CashingTotal =
                    place.FuelValue + place.ExpresswayValue;
                jobCashing.IsEnabled = item.SupplierCode == "SP18001";
                jobCashing.UpdateDate = now;
                jobCashing.UpdateBy = orderToEdit.UpdateBy;
            }

            orderToEdit.CustomerCode = customerCost.CustomerCode;
            orderToEdit.ContainerType =
                orderToEdit.JobType == "IMPORT" ? "LADEN" : "EMPTY";
            orderToEdit.JobFinish = orderToEdit.JobDate;

            if (orderToEdit.ContainerSizeCode == "06")
            {
                orderToEdit.ContainerSizeName = "20 GP";
            }
            else
            {
                orderToEdit.ContainerSizeName =
                    orderToEdit.ContainerSizeCode == "99"
                        ? txtOther
                        : container.ContainerSizeName;
            }

            var removeOcean = await db.TblJobOceans
                .Where(w => w.JobCode == jobCode)
                .ToListAsync(cancellationToken);

            db.TblJobOceans.RemoveRange(removeOcean);

            TblJobOcean? lastOcean = null;
            var oceanCount = oceanJob.iContainer.Count();

            for (var i = 0; i < oceanCount; i++)
            {
                if (string.IsNullOrWhiteSpace(oceanJob.iContainer[i]))
                {
                    continue;
                }

                var deliveryDate = oceanJob.iDelivery[i]
                    ?? throw new InvalidOperationException(
                        $"Delivery date at ocean row {i + 1} is required.");

                lastOcean = new TblJobOcean
                {
                    JobCode = jobCode,
                    BlNo = NormalizeUpper(oceanJob.iBL[i]),
                    ContainerNo = NormalizeUpper(oceanJob.iContainer[i]),
                    OrderQuantity = oceanJob.iQuantity[i],
                    OrderWeight = oceanJob.iWeight[i],
                    OrderCbm = oceanJob.iCBM[i],
                    DeliveryDate = deliveryDate,
                    EtaDate = oceanJob.eta_date,
                    VesselName = oceanJob.vessel,
                    ChangeNo = NormalizeUpper(oceanJob.change_no),
                    PlaceCode = oceanJob.place_code,
                    PickupDate = oceanJob.pickup_date,
                    ConsigneeName = oceanJob.consignee_name,
                    ShipperName = oceanJob.shipper_name,
                    ShippingName = oceanJob.shipping_name,
                    RequestName = oceanJob.request_name,
                    Description = oceanJob.description,
                    GoodsDeclarationNo =
                        string.IsNullOrWhiteSpace(oceanJob.goods_declaration)
                            ? "-"
                            : oceanJob.goods_declaration.Trim()
                };

                db.TblJobOceans.Add(lastOcean);
            }

            if (lastOcean == null)
            {
                throw new InvalidOperationException(
                    "At least one ocean container is required.");
            }

            orderToEdit.JobInvoice = NormalizeUpper(orderToEdit.JobInvoice);
            // Preserve the original business rule: master BookingNo/JobDate
            // are taken from the last valid ocean row.
            orderToEdit.BookingNo = lastOcean.BlNo;
            orderToEdit.JobDate = lastOcean.DeliveryDate;
            orderToEdit.JobFinish = orderToEdit.JobDate;

            ApplyJobMasterValues(currentJob, orderToEdit);
            currentJob.UpdateDate = now;
            currentJob.UpdateBy = orderToEdit.UpdateBy;

            var bindings = await db.TblBindingGenerates
                .Where(w => w.JobCode == jobCode)
                .ToListAsync(cancellationToken);

            foreach (var item in bindings)
            {
                item.Is2short =
                    orderToEdit.ContainerSizeCode == "06" &&
                    item.IndexCode % 2 == 0;
            }

            var newMasterValues = GetScalarValues(currentJob);
            var newDetailValues = details
                .Select(GetScalarValues)
                .ToList();

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Job",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblJobMaster),
                    EntityId = jobCode,
                    Description = "Update job data.",
                    OldValues = new
                    {
                        Master = oldMasterValues,
                        Details = oldDetailValues
                    },
                    NewValues = new
                    {
                        Master = newMasterValues,
                        Details = newDetailValues
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error editing ocean order {JobCode}",
                orderToEdit.JobCode);

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
        }

        return response;
    }

    // Transaction retained because the count determines which records are changed.
    // Concurrent removals can otherwise corrupt ContainerAmount / job state.
    public async Task<ResponseViewModel> RemoveOrder(
        string code,
        string job_detail_code,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var binding = await db.TblBindingGenerates.FindAsync(
                new object?[] { code, job_detail_code },
                cancellationToken);

            if (binding == null)
            {
                throw new InvalidOperationException(
                    $"Binding '{code}/{job_detail_code}' not found.");
            }

            var count = await db.TblBindingGenerates
                .CountAsync(
                    w => w.JobCode == code,
                    cancellationToken);

            if (count <= 0)
            {
                throw new InvalidOperationException(
                    $"No binding found for job '{code}'.");
            }

            var job = await db.TblJobMasters.FindAsync(
                new object?[] { code },
                cancellationToken);

            if (job == null)
            {
                throw new InvalidOperationException(
                    $"Job master '{code}' not found.");
            }

            db.TblBindingGenerates.Remove(binding);

            var now = DateTime.Now;

            if (count == 1)
            {
                var place = await db.TblJobPlaces
                    .FirstOrDefaultAsync(
                        w => w.JobCode == code,
                        cancellationToken);

                if (place == null)
                {
                    throw new InvalidOperationException(
                        $"Job place for '{code}' not found.");
                }

                db.TblJobPlaces.Remove(place);

                job.IsEnabled = false;
                job.UpdateBy = username;
                job.UpdateDate = now;
            }
            else
            {
                job.ContainerAmount = Math.Max(0, job.ContainerAmount - 1);
                job.UpdateBy = username;
                job.UpdateDate = now;
            }

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Job",
                    ActionName = AuditActions.Delete,
                    EntityName = nameof(TblJobMaster),
                    EntityId = job_detail_code,
                    Description =
                        $"ลบข้อมูล Job {job_detail_code} โดย {username}",
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "จัดการข้อมูลเรียบร้อยแล้ว";
        }
        catch (OperationCanceledException)
        {
            await tr.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            await tr.RollbackAsync(CancellationToken.None);

            logger.LogError(
                ex,
                "Error removing job detail {JobDetailCode} from job {JobCode}",
                job_detail_code,
                code);

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
        }

        return response;
    }

    public async Task<decimal> GetDriverCost(
        string place_code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblDriverRates
            .AsNoTracking()
            .Where(w => w.PlaceCode == place_code)
            .Select(w => (decimal?)w.RateValue)
            .FirstOrDefaultAsync(cancellationToken)
            ?? 0;
    }

    public async Task<decimal> GetSupplierCost(
        string supplyer_code,
        string truck_type,
        string place_code,
        CancellationToken cancellationToken = default)
    {
        var fleetTypeCode = await db.TblSuppliers
            .AsNoTracking()
            .Where(w => w.SupplierCode == supplyer_code)
            .Select(w => w.FleetTypeCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (fleetTypeCode == null)
        {
            throw new InvalidOperationException(
                $"Supplier '{supplyer_code}' not found.");
        }

        if (fleetTypeCode == "F1")
        {
            var fleet = await (
                from a in db.TblFleetInternals.AsNoTracking()
                join b in db.TblFleetRates.AsNoTracking()
                    on a.FleetCode equals b.FleetCode
                where a.SupplierCode == supplyer_code &&
                      b.PlaceCode == place_code
                select b)
                .FirstOrDefaultAsync(cancellationToken);

            if (fleet == null)
            {
                return 0;
            }

            return truck_type switch
            {
                "01" => fleet.Truck4w,
                "02" => fleet.Truck6w,
                "03" => fleet.Truck10w,
                _ => 0
            };
        }

        return await db.TblSupplierRates
            .AsNoTracking()
            .Where(w =>
                w.SupplierCode == supplyer_code &&
                w.PlaceCode == place_code)
            .Select(w => (decimal?)w.CostRate)
            .FirstOrDefaultAsync(cancellationToken)
            ?? 0;
    }

    public async Task<ViewDriverListActive> GetDriverCodeByTruck(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.ViewDriverListActives
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.TruckCode == code, cancellationToken);

        return data!;
    }

    private async Task<TblCustomerCost> GetCustomerCostDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.TblCustomerCosts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.CostCode == code,
                cancellationToken);

        return data ?? throw new InvalidOperationException(
            $"Customer cost '{code}' not found.");
    }

    private async Task<TblPortCashing> GetPortLift(
        string yardport_code,
        string container_kind_code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.TblPortCashings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.YardportCode == yardport_code &&
                     w.ContainerSizeCode == container_kind_code,
                cancellationToken);

        return data ?? throw new InvalidOperationException(
            $"Port cashing for yard/port '{yardport_code}' and container '{container_kind_code}' not found.");
    }

    private async Task<TblYardCashing> GetYardLift(
        string yardport_code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.TblYardCashings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.YardportCode == yardport_code,
                cancellationToken);

        return data ?? throw new InvalidOperationException(
            $"Yard cashing for '{yardport_code}' not found.");
    }

    private async Task<TblYardport> GetYardPort(
        string yardport_code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.TblYardports
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.YardportCode == yardport_code,
                cancellationToken);

        return data ?? throw new InvalidOperationException(
            $"Yard/port '{yardport_code}' not found.");
    }

    private async Task<PlaceCashingViewModel> GetPlaceCashing(
        string code,
        string truck_type,
        CancellationToken cancellationToken = default)
    {
        var data = await (
            from a in db.TblIndustries.AsNoTracking()
            join b in db.TblFuelRates.AsNoTracking()
                on a.ZoneCode equals b.ZoneCode
            where a.IndustryCode == code &&
                  b.TruckTypegroupCode == truck_type
            select new PlaceCashingViewModel
            {
                fuel_value = b.FuelRateValue
            })
            .FirstOrDefaultAsync(cancellationToken);

        return data ?? new PlaceCashingViewModel
        {
            fuel_value = 0
        };
    }

    private async Task<int> GetFuelRate(
        string place_code,
        string truck_type,
        CancellationToken cancellationToken = default)
    {
        var fuelRate = await (
            from industry in db.TblIndustries.AsNoTracking()
            join fuel in db.TblFuelRates.AsNoTracking()
                on industry.ZoneCode equals fuel.ZoneCode
            where industry.IndustryCode == place_code &&
                  fuel.TruckTypegroupCode == truck_type
            select (int?)fuel.FuelRateValue)
            .FirstOrDefaultAsync(cancellationToken);

        return fuelRate ?? 0;
    }

    private Dictionary<string, object?> GetScalarValues<TEntity>(TEntity entity)
        where TEntity : class
    {
        return db.Entry(entity)
            .Properties
            .ToDictionary(
                p => p.Metadata.Name,
                p => p.CurrentValue);
    }

    private void ApplyJobMasterValues(
        TblJobMaster trackedEntity,
        TblJobMaster postedEntity)
    {
        var entry = db.Entry(trackedEntity);

        // Preserve server-owned identity/audit fields even if the MVC form
        // does not post them back.
        var protectedValues = entry.Properties
            .Where(p =>
                p.Metadata.Name == "JobCode" ||
                p.Metadata.Name == "CreateDate" ||
                p.Metadata.Name == "CreateBy" ||
                p.Metadata.Name == "IsEnabled" ||
                p.Metadata.Name == "LiftedAmount" ||
                p.Metadata.Name == "ContainerAmount")
            .ToDictionary(
                p => p.Metadata.Name,
                p => p.CurrentValue);

        entry.CurrentValues.SetValues(postedEntity);

        foreach (var item in protectedValues)
        {
            entry.Property(item.Key).CurrentValue = item.Value;
        }
    }

    private static string NormalizeUpper(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "-"
            : value.Trim().ToUpperInvariant();
    }

    private static void ValidateOceanData(OrderOceanModel oceanJob)
    {
        var count = oceanJob.iContainer.Count();

        if (count == 0)
        {
            throw new InvalidOperationException(
                "Ocean container data is required.");
        }

        if (oceanJob.iBL.Count() != count ||
            oceanJob.iQuantity.Count() != count ||
            oceanJob.iWeight.Count() != count ||
            oceanJob.iCBM.Count() != count ||
            oceanJob.iDelivery.Count() != count)
        {
            throw new InvalidOperationException(
                "Ocean container data count mismatch.");
        }
    }

}