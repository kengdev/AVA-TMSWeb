using Microsoft.EntityFrameworkCore;
using System.Data;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class YardPortRepository
{
    private readonly DbavalogContext db;
    private readonly IAuditLogService auditLogService;
    private readonly CodeRepository codeRepo;

    public YardPortRepository(DbavalogContext context, IAuditLogService _auditLogService, CodeRepository _codeRepo)
    {
        db = context;
        auditLogService = _auditLogService;
        codeRepo = _codeRepo;
    }

    public async Task<List<YardPortViewModel>> GetYardPortDataList(
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblYardports.AsNoTracking()
            join b in db.TblPlaces.AsNoTracking()
                on a.YardportCode equals b.PlaceCode
            where b.IsEnabled == true &&
                  b.PlaceCode != "000000000"
            select new YardPortViewModel
            {
                yardport_code = a.YardportCode,
                yardport_name = b.PlaceName,
                yardport_type = b.PlaceType,
                is_actived = b.IsActived
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PortModel?> GetPortData(
        string yardport_code,
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblPlaces.AsNoTracking()
            join b in db.TblYardports.AsNoTracking()
                on a.PlaceCode equals b.YardportCode
            where a.PlaceCode == yardport_code
            select new PortModel
            {
                place_code = a.PlaceCode,
                place_name = a.PlaceName,
                place_type = a.PlaceType,
                place_remark = a.PlaceRemark,
                import_fee = b.ImportFee,
                export_fee = b.ExportFee
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<YardModel?> GetYardData(
        string yardport_code,
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblPlaces.AsNoTracking()
            join b in db.TblYardports.AsNoTracking()
                on a.PlaceCode equals b.YardportCode
            where a.PlaceCode == yardport_code
            select new YardModel
            {
                place_code = a.PlaceCode,
                place_name = a.PlaceName,
                place_type = a.PlaceType,
                place_remark = a.PlaceRemark,
                import_fee = b.ImportFee,
                export_fee = b.ExportFee
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TblYardCashing?> GetYardCashingData(
        string yardport_code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblYardCashings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.YardportCode == yardport_code,
                cancellationToken);
    }

    public async Task<List<PortViewModel>> GetPortCashingData(
        string yardport_code,
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblPortCashings.AsNoTracking()
            join b in db.TblContainerSizes.AsNoTracking()
                on a.ContainerSizeCode equals b.ContainerSizeCode
            where a.YardportCode == yardport_code
            select new PortViewModel
            {
                yardport_code = a.YardportCode,
                container_kind_code = b.ContainerSizeCode,
                container_size = b.ContainerSizeName,
                container_fee = a.ContainerFee,
                container_group = b.ContainerSizeGroup
            })
            .OrderByDescending(o => o.container_group)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TblContainerSize>> GetContainerData(
        CancellationToken cancellationToken = default)
    {
        return await db.TblContainerSizes
            .AsNoTracking()
            .Where(w => w.IsEnabled == true)
            .OrderByDescending(o => o.ContainerSizeGroup)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PortCashingModel>> GetListPortCashing(
        string code,
        CancellationToken cancellationToken = default)
    {
        var query =
            from a in db.TblContainerSizes.AsNoTracking()

            join b in db.TblPortCashings
                .AsNoTracking()
                .Where(w => w.YardportCode == code)

                on a.ContainerSizeCode equals b.ContainerSizeCode
                into joined

            from x in joined.DefaultIfEmpty()

            select new PortCashingModel
            {
                container_kind_code = a.ContainerSizeCode,
                container_kind_name = a.ContainerSizeName,
                container_group = a.ContainerSizeGroup,
                container_fee = x.ContainerFee
            };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ResponseViewModel> AddPortData(
        PortModel portToAdd,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var now = DateTime.Now;

            var placeCode =
                await codeRepo.GetPortCode(cancellationToken);

            var placeModel = new TblPlace
            {
                PlaceCode = placeCode,
                PlaceName = portToAdd.place_name.ToUpper(),
                PlaceType = "PORT",
                PlaceRemark = portToAdd.place_remark,
                IsActived = true,
                IsEnabled = true,
                CreateDate = now,
                CreateBy = portToAdd.create_by,
                UpdateDate = now,
                UpdateBy = portToAdd.create_by
            };

            var portModel = new TblYardport
            {
                YardportCode = placeCode,
                ImportFee = portToAdd.import_fee,
                ExportFee = portToAdd.export_fee
            };

            var containerCount =
                portToAdd.container_size.Count();

            var cashings =
                new List<TblPortCashing>(containerCount);

            for (var i = 0; i < containerCount; i++)
            {
                cashings.Add(
                    new TblPortCashing
                    {
                        YardportCode = placeCode,
                        ContainerSizeCode =
                            portToAdd.container_size[i],
                        ContainerFee =
                            portToAdd.container_fee[i]
                    });
            }

            portModel.TblPortCashings = cashings;

            db.TblYardports.Add(portModel);
            db.TblPlaces.Add(placeModel);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Port",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblPlace),
                    EntityId = portToAdd.place_code,
                    Description = $"เพิ่มข้อมูล Port {portToAdd.place_name}",
                    NewValues = new
                    {
                        portToAdd
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            response.result = true;
            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> AddYardData(
        YardModel yardToAdd,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var now = DateTime.Now;

            var placeCode = await codeRepo.GetYardCode(cancellationToken);

            var placeModel = new TblPlace
            {
                PlaceCode = placeCode,
                PlaceName = yardToAdd.place_name.ToUpper(),
                PlaceType = yardToAdd.place_type,
                PlaceRemark = yardToAdd.place_remark,
                IsActived = true,
                IsEnabled = true,
                CreateDate = now,
                CreateBy = yardToAdd.create_by,
                UpdateDate = now,
                UpdateBy = yardToAdd.create_by
            };

            var portModel = new TblYardport
            {
                YardportCode = placeCode,
                ImportFee = yardToAdd.import_fee
            };

            // คง logic เดิมจากไฟล์ต้นฉบับ
            portModel.ExportFee = portModel.ExportFee;

            var yardCashing = new TblYardCashing
            {
                YardportCode = placeCode,
                ShortliftonFee = yardToAdd.shortlifton,
                LongliftonFee = yardToAdd.longlifton,
                ShortliftoffFee = yardToAdd.shortliftoff,
                LongliftoffFee = yardToAdd.longliftoff
            };

            db.TblYardCashings.Add(yardCashing);

            placeModel.TblYardport = portModel;

            db.TblPlaces.Add(placeModel);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Yard",
                    ActionName = AuditActions.Create,
                    EntityName = nameof(TblPlace),
                    EntityId = yardToAdd.place_code,
                    Description = $"เพิ่มข้อมูล Yard {yardToAdd.place_name}",
                    NewValues = new
                    {
                        yardToAdd
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            response.result = true;
            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> EditPortData(
        PortModel portToEdit,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();
        try
        {
            var placeModel =
                await db.TblPlaces.FindAsync(
                    new object?[]
                    {
                        portToEdit.place_code
                    },
                    cancellationToken);

            var portModel =
                await db.TblYardports.FindAsync(
                    new object?[]
                    {
                        portToEdit.place_code
                    },
                    cancellationToken);

            if (placeModel == null ||
                portModel == null)
            {
                response.result = false;
                response.resultMessage =
                    "ไม่พบข้อมูล Port";

                return response;
            }

            var now = DateTime.Now;

            placeModel.PlaceName =
                portToEdit.place_name.ToUpper();

            placeModel.PlaceRemark =
                portToEdit.place_remark;

            placeModel.UpdateDate = now;
            placeModel.UpdateBy = portToEdit.update_by;

            portModel.ImportFee =
                portToEdit.import_fee;

            portModel.ExportFee =
                portToEdit.export_fee;

            // โหลดรายการเดิมครั้งเดียว
            // แทนการ Find ทีละรายการใน foreach
            var currentCashings =
                await db.TblPortCashings
                    .Where(w =>
                        w.YardportCode ==
                        portToEdit.place_code)
                    .ToListAsync(cancellationToken);

            db.TblPortCashings.RemoveRange(
                currentCashings);

            var containerCount =
                portToEdit.container_size.Count();

            var newCashings =
                new List<TblPortCashing>(containerCount);

            for (var i = 0; i < containerCount; i++)
            {
                newCashings.Add(
                    new TblPortCashing
                    {
                        YardportCode =
                            portToEdit.place_code,

                        ContainerSizeCode =
                            portToEdit.container_size[i],

                        ContainerFee =
                            portToEdit.container_fee[i]
                    });
            }

            db.TblPortCashings.AddRange(
                newCashings);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Port",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblYardport),
                    EntityId = portToEdit.place_code,
                    Description = $"แก้ไขข้อมูล Port {portToEdit.place_name}",
                    NewValues = new
                    {
                        portToEdit
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            response.result = true;
            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> EditYardData(
        YardModel yardToEdit,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();
        try
        {
            var placeModel =
                await db.TblPlaces.FindAsync(
                    new object?[]
                    {
                        yardToEdit.place_code
                    },
                    cancellationToken);

            var oldPlace = await db.TblPlaces
                .AsNoTracking()
                .Where(w => w.PlaceCode == yardToEdit.place_code)
                .Select(a => new
                {
                    a.PlaceCode,
                    a.PlaceName,
                    a.IsActived,
                    a.IsEnabled
                })
                .FirstOrDefaultAsync(cancellationToken);

            var yardModel =
                await db.TblYardports.FindAsync(
                    new object?[]
                    {
                        yardToEdit.place_code
                    },
                    cancellationToken);

            var yardCashing =
                await db.TblYardCashings.FindAsync(
                    new object?[]
                    {
                        yardToEdit.place_code
                    },
                    cancellationToken);

            if (placeModel == null ||
                yardModel == null ||
                yardCashing == null)
            {
                response.result = false;
                response.resultMessage =
                    "ไม่พบข้อมูล Yard";

                return response;
            }

            var now = DateTime.Now;

            placeModel.PlaceName =
                yardToEdit.place_name.ToUpper();

            placeModel.PlaceRemark =
                yardToEdit.place_remark;

            placeModel.UpdateDate = now;
            placeModel.UpdateBy = yardToEdit.update_by;

            yardModel.ImportFee =
                yardToEdit.import_fee;

            yardModel.ExportFee =
                yardToEdit.export_fee;

            yardCashing.ShortliftonFee =
                yardToEdit.shortlifton;

            yardCashing.LongliftonFee =
                yardToEdit.longlifton;

            yardCashing.ShortliftoffFee =
                yardToEdit.shortliftoff;

            yardCashing.LongliftoffFee =
                yardToEdit.longliftoff;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Yard",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblYardport),
                    EntityId = yardToEdit.place_code,
                    Description = $"แก้ไขข้อมูล Yard {yardToEdit.place_name}",
                    OldValues = oldPlace,
                    NewValues = new
                    {
                        yardToEdit
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            response.result = true;
            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> Remove(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var item =
                await db.TblPlaces.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            var oldYardPort = new
            {
                item.PlaceCode,
                item.PlaceName,
                item.IsActived,
                item.IsEnabled
            };

            if (item == null)
            {
                response.result = false;
                response.resultMessage =
                    "ไม่พบข้อมูล Place";

                return response;
            }

            var now = DateTime.Now;

            item.IsActived = false;
            item.IsEnabled = false;
            item.UpdateDate = now;
            item.UpdateBy = username;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "YardPort",
                    ActionName = AuditActions.Delete,
                    EntityName = nameof(TblYardport),
                    EntityId = code,
                    Description = $"ลบข้อมูล YardPort{oldYardPort.PlaceName}",
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            response.result = true;
            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> Active(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();
        try
        {
            var item =
                await db.TblPlaces.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            var oldYardPort = new
            {
                item.PlaceCode,
                item.PlaceName,
                item.IsActived,
                item.IsEnabled
            };

            if (item == null)
            {
                response.result = false;
                response.resultMessage =
                    "ไม่พบข้อมูล Place";

                return response;
            }

            item.IsActived = true;
            item.UpdateDate = DateTime.Now;
            item.UpdateBy = username;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "YardPort",
                    ActionName = AuditActions.Active,
                    EntityName = nameof(TblYardport),
                    EntityId = code,
                    Description = $"เปิดใช้งน YardPort{oldYardPort.PlaceName}",
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            response.result = true;
            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> InActive(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();
        try
        {
            var item =
                await db.TblPlaces.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            var oldYardPort = new
            {
                item.PlaceCode,
                item.PlaceName,
                item.IsActived,
                item.IsEnabled
            };

            if (item == null)
            {
                response.result = false;
                response.resultMessage =
                    "ไม่พบข้อมูล Place";

                return response;
            }

            item.IsActived = false;
            item.UpdateDate = DateTime.Now;
            item.UpdateBy = username;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "YardPort",
                    ActionName = AuditActions.Inactive,
                    EntityName = nameof(TblYardport),
                    EntityId = code,
                    Description = $"ปิดใช้งน YardPort{oldYardPort.PlaceName}",
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            response.result = true;
            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }
}