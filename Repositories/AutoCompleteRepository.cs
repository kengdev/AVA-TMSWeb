using Microsoft.EntityFrameworkCore;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class AutoCompleteRepository
{
    private readonly DbavalogContext db;

    public AutoCompleteRepository(DbavalogContext context)
    {
        db = context;
    }

    // =====================================================
    // Driver
    // =====================================================
    public async Task<List<ViewDriverListActive>> GetDriver(
        string term,
        CancellationToken cancellationToken = default)
    {
        var termUpper = term.ToUpper();

        return await db.ViewDriverListActives
            .AsNoTracking()
            .Where(w =>
                w.DriverCode.Contains(termUpper) ||
                w.DriverFullname.Contains(term))
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Truck
    // =====================================================
    public async Task<List<TruckViewModel>> GetTruck(
        string term,
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblTrucks.AsNoTracking()

            join b in db.TblTruckDetails.AsNoTracking()
                on a.TruckCode equals b.TruckCode
                into truckDetails

            from p in truckDetails.DefaultIfEmpty()

            select new TruckViewModel
            {
                truck_code = a.TruckCode,
                truck_license = a.TruckLicense,
                truck_no =
                    p != null && p.TruckNo != null
                        ? a.TruckLicense + " [" + p.TruckNo + "]"
                        : a.TruckLicense
            })
            .Where(w =>
                w.truck_license.Contains(term) ||
                w.truck_no.Contains(term))
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Trailer
    // =====================================================
    public async Task<List<TblTrailer>> GetTrailer(
        string term,
        CancellationToken cancellationToken = default)
    {
        return await db.TblTrailers
            .AsNoTracking()
            .Where(w =>
                w.TrailerLicense.Contains(term) ||
                w.TrailerNo.Contains(term))
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Zone
    // =====================================================
    public async Task<List<TblZone>> GetZone(
        string term,
        CancellationToken cancellationToken = default)
    {
        return await db.TblZones
            .AsNoTracking()
            .Where(w =>
                w.ZoneCode.Contains(term) ||
                w.ZoneName.Contains(term))
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Place
    // =====================================================
    public async Task<List<TblPlace>> GetPlace(
        string term,
        CancellationToken cancellationToken = default)
    {
        return await db.TblPlaces
            .AsNoTracking()
            .Where(w =>
                w.PlaceCode.Contains(term) ||
                w.PlaceName.Contains(term))
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Customer Cost
    // =====================================================
    public async Task<List<CostModel>> GetCustomerCost(
        string term,
        CancellationToken cancellationToken = default)
    {
        return await db.ViewCostListActives
            .AsNoTracking()
            .Where(a =>
                a.IsActived == true &&
                (
                    a.CostCode.Contains(term) ||
                    a.CustomerNameTh.Contains(term) ||
                    a.MissionName.Contains(term)
                ))
            .Select(a => new CostModel
            {
                cost_code = a.CostCode,
                cost_name = a.MissionName,
                cost_remark = a.CostRemark,
                customer_code = a.CustomerCode,
                cost_value = a.CostValue.Value,
                customer_name_th = a.CustomerNameTh
            })
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Supplier
    // =====================================================
    public async Task<List<TblSupplier>> GetSupplier(
        string term,
        CancellationToken cancellationToken = default)
    {
        return await db.TblSuppliers
            .AsNoTracking()
            .Where(w =>
                w.SupplierCode.Contains(term) ||
                w.SupplierName.Contains(term) ||
                w.SupplierFullname.Contains(term))
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Customer
    // =====================================================
    public async Task<List<CustomerModel>> GetCustomer(
        string term,
        CancellationToken cancellationToken = default)
    {
        var termUpper = term.ToUpper();

        return await db.TblCustomers
            .AsNoTracking()
            .Where(a =>
                a.IsActived == true &&
                (
                    a.CustomerCode.Contains(term) ||
                    a.CustomerAcronym.ToUpper().Contains(termUpper) ||
                    a.CustomerNameEn.Contains(term)
                ))
            .Select(a => new CustomerModel
            {
                customer_code = a.CustomerCode,
                customer_name_th = a.CustomerNameTh
            })
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Booking
    // =====================================================
    public async Task<List<string>> GetBooking(
        string term,
        CancellationToken cancellationToken = default)
    {
        var termUpper = term.ToUpper();

        return await db.TblJobMasters
            .AsNoTracking()
            .Where(a =>
                a.BookingNo.Contains(termUpper))
            .Select(a => a.BookingNo)
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Place By Type
    // =====================================================
    public async Task<List<TblPlace>> GetPlaceByType(
        string term,
        string type,
        CancellationToken cancellationToken = default)
    {
        return await db.TblPlaces
            .AsNoTracking()
            .Where(w =>
                w.PlaceType == type &&
                (
                    w.PlaceCode.Contains(term) ||
                    w.PlaceName.Contains(term)
                ))
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Driver List
    // =====================================================
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
}
