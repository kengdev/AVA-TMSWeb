using Microsoft.EntityFrameworkCore;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class DropListRepository
{
    private readonly DbavalogContext db;

    public DropListRepository(DbavalogContext context)
    {
        db = context;
    }

    public async Task<List<DropDownViewModel>> GetSupplierType(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblSupplierTypes
                    select new DropDownViewModel
                    {
                        key = a.SupplierTypeCode,
                        label = a.SupplierTypeName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetZone(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblZones
                    select new DropDownViewModel
                    {
                        key = a.ZoneCode,
                        label = a.ZoneName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetPaymentType(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblPaymentTypes
                    select new DropDownViewModel
                    {
                        key = a.PaymentTypeCode,
                        label = a.PaymentTypeName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetFleet(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblFleets
                    where a.IsEnabled == true
                    select new DropDownViewModel
                    {
                        key = a.FleetCode,
                        label = a.FleetName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetCustomerType(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblCustomerTypes
                    select new DropDownViewModel
                    {
                        key = a.CustomerTypeCode,
                        label = a.CustomerTypeName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetSupplierTransport(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblSuppliers
                    where (a.IsActived == true && a.FleetTypeCode == "F2") || a.SupplierCode == "SP18001"
                    orderby a.SupplierName ascending
                    select new DropDownViewModel
                    {
                        key = a.SupplierCode,
                        label = a.SupplierName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetSupplierFleet(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblSuppliers
                    join b in db.TblFleetInternals on a.SupplierCode equals b.SupplierCode
                    where a.IsActived == true
                    select new DropDownViewModel
                    {
                        key = a.SupplierCode,
                        label = a.SupplierName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetSupplierSupport(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblSuppliers
                    where a.FleetTypeCode == "F3" && a.IsActived == true
                    select new DropDownViewModel
                    {
                        key = a.SupplierCode,
                        label = a.SupplierName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetTruckBrand(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblBrands
                    select new DropDownViewModel
                    {
                        key = a.BrandCode,
                        label = a.BrandName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetTruckType(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblTruckTypes
                    select new DropDownViewModel
                    {
                        key = a.TruckTypeCode,
                        label = a.TruckTypeName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetFuelType(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblFuelTypes
                    select new DropDownViewModel
                    {
                        key = a.FuelTypeCode,
                        label = a.FuelTypeName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public List<DropDownViewModel> GetTruckYear()
    {
        int current = DateTime.Now.Year;
        List<DropDownViewModel> list = new List<DropDownViewModel>(current - 2005 + 1);
        for (int i = 2005; i <= current; i++)
        {
            DropDownViewModel item = new DropDownViewModel();
            item.key = i.ToString();
            item.label = i.ToString();
            list.Add(item);
        }

        return list;
    }

    public async Task<List<DropDownViewModel>> GetProvince(CancellationToken cancellationToken = default)
    {
        var query = from a in db.LcProvinces
                    orderby a.ProvinceNameTha ascending
                    select new DropDownViewModel
                    {
                        key = a.ProvinceCode,
                        label = a.ProvinceNameTha
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetInsurance(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblInsurances
                    select new DropDownViewModel
                    {
                        key = a.InsuranceCode,
                        label = a.InsuranceName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetInsuranceType(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblInsuranceTypes
                    select new DropDownViewModel
                    {
                        key = a.InsuranceTypeCode,
                        label = a.InsuranceTypeName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetTrailerType(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblTrailerTypes
                    select new DropDownViewModel
                    {
                        key = a.TrailerTypeCode,
                        label = a.TrailerTypeName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetTrailerFloor(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblTrailerFloors
                    select new DropDownViewModel
                    {
                        key = a.TrailerFloorCode,
                        label = a.TrailerFloorName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetDriver(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblDrivers
                    where a.IsEnabled == true
                    select new DropDownViewModel
                    {
                        key = a.DriverCode,
                        label = a.DriverFullname
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetTruck(CancellationToken cancellationToken = default)
    {
        var query = await (from a in db.TblTrucks
                           join b in db.TblTruckDetails on a.TruckCode equals b.TruckCode
                           where a.IsActived == true
                           orderby b.TruckNo
                           select new DropDownViewModel
                           {
                               key = a.TruckCode,
                               label = a.TruckLicense + " [" + b.TruckNo + "]"
                           }).ToListAsync(cancellationToken);

        return query;
    }

    public async Task<List<DropDownViewModel>> GetContainerTruck(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblTrucks
                    join b in db.TblTruckDetails on a.TruckCode equals b.TruckCode
                    where a.IsActived == true && b.TruckTypeCode != "03"
                    orderby b.TruckNo
                    select new DropDownViewModel
                    {
                        key = a.TruckCode,
                        label = a.TruckLicense + " [" + b.TruckNo + "]"
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> Get4WTruck(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblTrucks
                    join b in db.TblTruckDetails on a.TruckCode equals b.TruckCode
                    where a.IsActived == true && b.TruckTypeCode == "03" && b.TruckCode != "T000000"
                    orderby b.TruckNo
                    select new DropDownViewModel
                    {
                        key = a.TruckCode,
                        label = a.TruckLicense + " [" + b.TruckNo + "]"
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetTrailer(CancellationToken cancellationToken = default)
    {
        var query = from a in db.ViewTrailerListActives
                    where a.IsActived == true
                    orderby a.TrailerLicense
                    select new DropDownViewModel
                    {
                        key = a.TrailerCode,
                        label = a.TrailerLicense + " [" + a.TrailerNo + "]"
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public List<DropDownViewModel> GetMonth()
    {
        List<DropDownViewModel> list = new List<DropDownViewModel>();
        for (int i = 1; i <= 12; i++)
        {
            DropDownViewModel item = new DropDownViewModel();
            switch (i)
            {
                case 1:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "มกราคม";
                    break;
                case 2:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "กุมภาพันธ์";
                    break;
                case 3:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "มีนาคม";
                    break;
                case 4:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "เมษายน";
                    break;
                case 5:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "พฤษภาคม";
                    break;
                case 6:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "มิถุนายน";
                    break;
                case 7:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "กรกฎาคม";
                    break;
                case 8:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "สิงหาคม";
                    break;
                case 9:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "กันยายน";
                    break;
                case 10:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "ตุลาคม";
                    break;
                case 11:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "พฤศจิกายน";
                    break;
                case 12:
                    item.key = i.ToString().PadLeft(2, '0');
                    item.label = "ธันวาคม";
                    break;
            }

            list.Add(item);
        }

        return list;
    }

    public List<DropDownViewModel> GetBranchType()
    {
        List<DropDownViewModel> list = new List<DropDownViewModel>();
        DropDownViewModel item = new DropDownViewModel();
        item.key = "1";
        item.label = "สำนักงานใหญ่";
        list.Add(item);

        item = new DropDownViewModel();
        item.key = "2";
        item.label = "สาขา";
        list.Add(item);

        return list;
    }

    public List<DropDownViewModel> GetReceptType()
    {
        List<DropDownViewModel> list = new List<DropDownViewModel>();
        var item = new DropDownViewModel();
        item.key = "01";
        item.label = "เงินสด";
        list.Add(item);

        item = new DropDownViewModel();
        item.key = "02";
        item.label = "โอนเงิน";
        list.Add(item);

        item = new DropDownViewModel();
        item.key = "03";
        item.label = "เช็คธนาคาร";
        list.Add(item);

        return list;
    }

    public List<DropDownViewModel> GetYardPortType()
    {
        List<DropDownViewModel> list = new List<DropDownViewModel>();
        var item = new DropDownViewModel();
        item.key = "PORT";
        item.label = "ท่าเรือ";
        list.Add(item);

        item = new DropDownViewModel();
        item.key = "YARD";
        item.label = "ลานตู้";
        list.Add(item);
        return list;
    }

    public async Task<List<DropDownViewModel>> GetAgent(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblAgents
                    where a.IsActived == true
                    orderby a.AgentName
                    select new DropDownViewModel
                    {
                        key = a.AgentName,
                        label = a.AgentName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public List<DropDownViewModel> GetCostType()
    {
        List<DropDownViewModel> list = new List<DropDownViewModel>();
        var item = new DropDownViewModel();

        item.key = "A";
        item.label = "คิดเป็นยอดเงิน";
        list.Add(item);

        item = new DropDownViewModel();
        item.key = "B";
        item.label = "คิดเป็นเปอร์เซนต์";
        list.Add(item);

        return list;
    }

    public async Task<List<DropDownViewModel>> GetContainerSize(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblContainerSizes
                    select new DropDownViewModel
                    {
                        key = a.ContainerSizeCode,
                        label = a.ContainerSizeName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetBank(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblBanks
                    select new DropDownViewModel
                    {
                        key = a.BankCode,
                        label = a.BankName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public List<DropDownViewModel> GetYear()
    {
        int currentYear = DateTime.Now.Year;
        List<DropDownViewModel> list = new List<DropDownViewModel>(3);
        DropDownViewModel item = new DropDownViewModel();

        item.key = currentYear.ToString();
        item.label = currentYear.ToString();
        list.Add(item);

        item = new DropDownViewModel();
        item.key = (currentYear - 1).ToString();
        item.label = (currentYear - 1).ToString();
        list.Add(item);

        item = new DropDownViewModel();
        item.key = (currentYear - 2).ToString();
        item.label = (currentYear - 2).ToString();
        list.Add(item);

        return list;
    }

    public async Task<List<DropDownViewModel>> GetTruckGroup(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblTruckTypegroups
                    select new DropDownViewModel
                    {
                        key = a.TruckTypegroupCode,
                        label = a.TruckTypegroupName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public List<DropDownViewModel> GetMaintenanceStatus()
    {
        List<DropDownViewModel> list = new List<DropDownViewModel>();
        var item = new DropDownViewModel();
        item.key = "S";
        item.label = "หยุดงาน";
        list.Add(item);

        item = new DropDownViewModel();
        item.key = "R";
        item.label = "ปฏิบัติงานตามปกติ";
        list.Add(item);

        return list;
    }

    /*public List<DropDownViewModel> GetSpareparts()
    {
        var query = from a in db.ViewSparepartsStockeds
                    select new DropDownViewModel
                    {
                        key = a.SparepartCode,
                        label = a.SparepartName
                    };
        return query.ToList();
    }*/

    public async Task<List<DropDownViewModel>> GetConsignee(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblConsignees
                    select new DropDownViewModel
                    {
                        key = a.ConsigneeName,
                        label = a.ConsigneeName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetShipper(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblShippers
                    select new DropDownViewModel
                    {
                        key = a.ShipperName,
                        label = a.ShipperName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<DropDownViewModel>> GetRequestBy(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblRequests
                    select new DropDownViewModel
                    {
                        key = a.RequestName,
                        label = a.RequestName
                    };
        return await query.ToListAsync(cancellationToken);
    }


    public async Task<List<DropDownViewModel>> GetShippingContact(CancellationToken cancellationToken = default)
    {
        var query = from a in db.TblShippingContacts
                    select new DropDownViewModel
                    {
                        key = a.ShippingContactName,
                        label = a.ShippingContactName
                    };
        return await query.ToListAsync(cancellationToken);
    }

    public List<DropDownViewModel> GetTruckTrailer()
    {
        List<DropDownViewModel> list = new List<DropDownViewModel>();
        DropDownViewModel item = new DropDownViewModel();
        item.key = "1";
        item.label = "หัวลาก";
        list.Add(item);

        item = new DropDownViewModel();
        item.key = "2";
        item.label = "หางลาก";
        list.Add(item);

        return list;
    }

    public List<DropDownViewModel> GetTyreChangeType()
    {
        List<DropDownViewModel> list = new List<DropDownViewModel>();
        DropDownViewModel item = new DropDownViewModel();
        item.key = "1";
        item.label = "ปกติ";
        list.Add(item);

        item = new DropDownViewModel();
        item.key = "2";
        item.label = "หล่อ";
        list.Add(item);

        return list;
    }
}