using Microsoft.EntityFrameworkCore;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class SearchRepository
{
    private readonly DbavalogContext db;

    public SearchRepository(DbavalogContext context)
    {
        db = context;
    }

    // =====================================================
    // Yard / Port
    // =====================================================
    public async Task<List<YardPortViewModel>> GetYardPortDataList(
        CancellationToken cancellationToken = default)
    {
        return await db.TblPlaces
            .AsNoTracking()
            .Where(a => a.IsActived == true)
            .Select(a => new YardPortViewModel
            {
                yardport_code = a.PlaceCode,
                yardport_name = a.PlaceName,
                yardport_type = a.PlaceType
            })
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Port
    // =====================================================
    public async Task<List<YardPortViewModel>> GetPortDataList(
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblYardports.AsNoTracking()

            join b in db.TblPlaces.AsNoTracking()
                on a.YardportCode equals b.PlaceCode

            where
                b.IsActived == true &&
                b.PlaceType == "PORT"

            select new YardPortViewModel
            {
                yardport_code = a.YardportCode,
                yardport_name = b.PlaceName,
                yardport_type = b.PlaceType,
                import_fee = a.ImportFee,
                export_fee = a.ExportFee
            })
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Yard
    // =====================================================
    public async Task<List<YardPortViewModel>> GetYardDataList(
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblYardports.AsNoTracking()

            join b in db.TblPlaces.AsNoTracking()
                on a.YardportCode equals b.PlaceCode

            where
                b.IsActived == true &&
                b.PlaceType == "YARD"

            select new YardPortViewModel
            {
                yardport_code = a.YardportCode,
                yardport_name = b.PlaceName,
                yardport_type = b.PlaceType,
                import_fee = a.ImportFee,
                export_fee = a.ExportFee
            })
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Place
    // =====================================================
    public async Task<List<PlaceViewModel>> GetPlaceDataList(
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblZones.AsNoTracking()

            join b in db.TblIndustries.AsNoTracking()
                on a.ZoneCode equals b.ZoneCode

            join c in db.TblPlaces.AsNoTracking()
                on b.IndustryCode equals c.PlaceCode

            where c.IsEnabled == true

            select new PlaceViewModel
            {
                place_code = c.PlaceCode,
                place_port_code = b.PortCode,
                place_name = c.PlaceName,
                zone_name = a.ZoneName
            })
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Customer Cost
    // =====================================================
    public async Task<List<CostViewModel>> GetCustomerCostDataList(
        CancellationToken cancellationToken = default)
    {
        return await db.ViewCostListActives
            .AsNoTracking()
            .Where(a => a.IsActived == true)
            .Select(a => new CostViewModel
            {
                customer_code = a.CustomerCode,
                customer_acronym = a.CustomerAcronym,
                cost_code = a.CostCode,
                cost_remark = a.CostRemark,
                mission_code = a.MissionCode,
                mission_name = a.MissionName
            })
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Customer
    // =====================================================
    public async Task<List<CustomerViewModel>> GetCustomerDataList(
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblCustomers.AsNoTracking()

            join b in db.TblCustomerTypes.AsNoTracking()
                on a.CustomerTypeCode equals b.CustomerTypeCode

            where b.IsEnabled == true

            select new CustomerViewModel
            {
                customer_code = a.CustomerCode,
                customer_name_th = a.CustomerNameTh,
                customer_name_en = a.CustomerNameEn,
                customer_address = a.CustomerAddress
            })
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Supplier
    // =====================================================
    public async Task<List<SupplierViewModel>> GetSupplierDataList(
        CancellationToken cancellationToken = default)
    {
        return await db.TblSuppliers
            .AsNoTracking()
            .Where(a => a.IsActived == true)
            .Select(a => new SupplierViewModel
            {
                supplier_code = a.SupplierCode,
                supplier_name = a.SupplierName,
                supplier_type = a.FleetTypeCode
            })
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Zone
    // =====================================================
    public async Task<List<ZoneViewModel>> GetZoneDataList(
        CancellationToken cancellationToken = default)
    {
        return await db.TblZones
            .AsNoTracking()
            .Where(a => a.IsActived == true)
            .Select(a => new ZoneViewModel
            {
                zone_code = a.ZoneCode,
                zone_name = a.ZoneName
            })
            .ToListAsync(cancellationToken);
    }
}