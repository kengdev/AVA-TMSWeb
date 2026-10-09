using TMSWeb.Models;
namespace TMSWeb.Services;

// DEMO ONLY: process-local storage, fixed demo lifecycle, no SQL/GPS integration.
public sealed class DemoDispatchService : IDispatchService
{
    private const int PreparationMinutes = 30;
    private const int TurnaroundMinutes = 45;
    private static readonly TimeSpan BangkokOffset = TimeSpan.FromHours(7);
    private readonly object gate = new();
    private readonly List<Assignment> assignments = [];
    private readonly List<DispatchJob> jobs = [];
    private readonly List<Vehicle> vehicles = [
        new("V01","70-2481","COMPANY","บริษัท","หัวลาก","สมชาย ใจดี","แหลมฉบัง","READY"),
        new("V02","71-5932","COMPANY","บริษัท","หัวลาก","วิชัย มั่นคง","บางนา","READY"),
        new("V03","70-1826","COMPANY","บริษัท","สิบล้อ","ธนา พูลสุข","ศรีราชา","READY"),
        new("V04","72-6094","COMPANY","บริษัท","หัวลาก","อนุชา แสงทอง","อู่ชลบุรี","MAINTENANCE"),
        new("V05","71-8270","COMPANY","บริษัท","สิบล้อ","พิชัย รุ่งเรือง","แหลมฉบัง","READY"),
        new("V06","70-9913","PARTNER","รุ่งเรืองขนส่ง","หัวลาก","เอกชัย สุขใจ","แหลมฉบัง","UNCONFIRMED"),
        new("V07","71-4308","PARTNER","สยามทรานสปอร์ต","หัวลาก","อาทิตย์ วงศ์ดี","บางปะกง","READY"),
        new("V08","72-3167","PARTNER","รุ่งเรืองขนส่ง","สิบล้อ","วุฒิชัย พรดี","ระยอง","UNCONFIRMED")
    ];
    public DemoDispatchService()
    {
        var today = DateTimeOffset.UtcNow.ToOffset(BangkokOffset).Date;
        DateTimeOffset At(int day, double hour) => new(today.AddDays(day).AddHours(hour), BangkokOffset);
        for (int day = 0; day < 3; day++)
        {
            var prefix = day == 0 ? "JOB" : $"D{day}";
            jobs.AddRange([
                new($"{prefix}-001","ABC Logistics","ท่าเรือแหลมฉบัง","บางนา","หัวลาก",At(day,14),At(day,17),true),
                new($"{prefix}-002","Siam Export","ศรีราชา","ลาดกระบัง","หัวลาก",At(day,18.5),At(day,21),false),
                new($"{prefix}-003","Matang Cosmetic","ชลบุรี","บางปะกง","สิบล้อ",At(day,15),At(day,17),false),
                new($"{prefix}-004","Eastern Cargo","มาบตาพุด","แหลมฉบัง","หัวลาก",At(day,16),At(day,19),false),
                new($"{prefix}-005","Thai Distribution","บางนา","อยุธยา","สิบล้อ",At(day,22),At(day+1,2),false)
            ]);
        }
        Seed("TRIP-101","V01","ABC Logistics","แหลมฉบัง","บางนา",At(0,7),At(0,10),"COMPLETED");
        Seed("TRIP-102","V01","Siam Export","บางนา","ศรีราชา",At(0,11),At(0,12.5),"RUNNING");
        Seed("TRIP-201","V02","Eastern Cargo","มาบตาพุด","ลาดกระบัง",At(0,9),At(0,12),"COMPLETED");
        Seed("TRIP-202","V02","Siam Export","ลาดกระบัง","แหลมฉบัง",At(0,15),At(0,18),"PLANNED");
        Seed("TRIP-301","V03","Matang Cosmetic","ศรีราชา","บางปะกง",At(0,10),At(0,15),"RUNNING");
        Seed("TRIP-701","V07","Eastern Cargo","บางปะกง","ระยอง",At(0,8),At(0,11),"COMPLETED");
        Seed("TRIP-702","V07","ABC Logistics","ระยอง","แหลมฉบัง",At(0,16),At(0,19),"PLANNED");
        Seed("TRIP-N01","V05","Thai Distribution","อยุธยา","บางนา",At(-1,23),At(0,3),"COMPLETED");
        Seed("TRIP-N02","V05","Thai Distribution","บางนา","อยุธยา",At(0,22),At(1,2),"PLANNED");
    }
    private void Seed(string id, string vehicleId, string customer, string origin, string destination,
        DateTimeOffset pickup, DateTimeOffset delivery, string status)
    {
        var vehicle = vehicles.Single(x => x.Id == vehicleId);
        jobs.Add(new(id,customer,origin,destination,vehicle.Kind,pickup,delivery,false));
        assignments.Add(new(id,id,vehicleId,vehicle.Driver,pickup.AddMinutes(-PreparationMinutes),
            pickup,delivery,delivery.AddMinutes(TurnaroundMinutes),status,DateTimeOffset.UtcNow));
    }
    public DispatchSnapshot Snapshot()
    {
        lock (gate)
            return new(vehicles.ToArray(),jobs.ToArray(),assignments.ToArray(),
                PreparationMinutes,TurnaroundMinutes,"Asia/Bangkok");
    }
    public DispatchResult Assign(AssignRequest request)
    {
        lock (gate)
        {
            var job = jobs.Find(x => x.Id == request.JobId);
            var vehicle = vehicles.Find(x => x.Id == request.VehicleId);
            if (job is null || vehicle is null) return new(false,"ไม่พบงานหรือรถ กรุณารีเฟรช");
            if (assignments.Any(x => x.JobId == job.Id)) return new(false,"งานนี้ถูกจัดรถแล้ว");
            var error = Validate(job,vehicle,request.PickupAt,request.DeliveryAt,null);
            if (error is not null) return new(false,error);
            assignments.Add(Create(Guid.NewGuid().ToString("N"),job.Id,vehicle,request.PickupAt,request.DeliveryAt));
            return new(true,"จัดรถสำเร็จ และจองช่วงเวลารวมเตรียมรถ/พักก่อนเที่ยวถัดไปแล้ว");
        }
    }
    public DispatchResult Move(MoveRequest request)
    {
        lock (gate)
        {
            var item = assignments.Find(x => x.Id == request.AssignmentId);
            var vehicle = vehicles.Find(x => x.Id == request.VehicleId);
            if (item is null || vehicle is null) return new(false,"ไม่พบเที่ยวงานหรือรถ กรุณารีเฟรช");
            if (item.Status != "PLANNED") return new(false,"เปลี่ยนได้เฉพาะเที่ยวที่ยังไม่เริ่มงาน");
            var job = jobs.Single(x => x.Id == item.JobId);
            var error = Validate(job,vehicle,request.PickupAt,request.DeliveryAt,item.Id);
            if (error is not null) return new(false,error);
            assignments[assignments.IndexOf(item)] = Create(item.Id,item.JobId,vehicle,request.PickupAt,request.DeliveryAt)
                with { CreatedAt = item.CreatedAt };
            return new(true,"เปลี่ยนรถ/เวลาเที่ยวงานแล้ว");
        }
    }
    private Assignment Create(string id, string jobId, Vehicle vehicle, DateTimeOffset pickup, DateTimeOffset delivery)
        => new(id,jobId,vehicle.Id,vehicle.Driver,pickup.AddMinutes(-PreparationMinutes),pickup,
            delivery,delivery.AddMinutes(TurnaroundMinutes),"PLANNED",DateTimeOffset.UtcNow);

    // Half-open intervals: [from, until). Equality at the next start is allowed.
    private string? Validate(DispatchJob job, Vehicle vehicle, DateTimeOffset pickup, DateTimeOffset delivery, string? ignoreId)
    {
        if (pickup == default || delivery == default || delivery <= pickup)
            return "เวลาจบงานต้องอยู่หลังเวลารับสินค้า";
        if ((delivery - pickup).TotalHours > 48) return "ตัวอย่างนี้รองรับเที่ยวงานไม่เกิน 48 ชั่วโมง";
        if (vehicle.Status == "MAINTENANCE") return "รถอยู่ระหว่างซ่อมบำรุง";
        if (vehicle.Status == "UNCONFIRMED") return "ต้องยืนยันรถร่วมพร้อมใช้งานก่อน";
        if (vehicle.Kind != job.Kind) return "ประเภทรถไม่ตรงกับงาน";
        if (string.IsNullOrWhiteSpace(vehicle.Driver)) return "รถยังไม่มีคนขับ";
        var from = pickup.AddMinutes(-PreparationMinutes);
        var until = delivery.AddMinutes(TurnaroundMinutes);
        var conflict = assignments.FirstOrDefault(x => x.Id != ignoreId &&
            (x.VehicleId == vehicle.Id || x.Driver == vehicle.Driver) &&
            from < x.AvailableAt && until > x.OccupiedFrom);
        return conflict is null ? null : $"เวลารถหรือคนขับซ้อนกับ {conflict.JobId} (รวมเตรียมรถและพักก่อนเที่ยวถัดไป)";
    }
    public DispatchResult Cancel(string assignmentId)
    {
        lock (gate)
        {
            var item = assignments.Find(x => x.Id == assignmentId);
            if (item is null) return new(false,"ไม่พบรายการจัดรถ");
            if (item.Status != "PLANNED") return new(false,"ยกเลิกได้เฉพาะเที่ยวที่ยังไม่เริ่มงาน");
            assignments.Remove(item);
            return new(true,"ยกเลิกเที่ยวแล้ว ช่วงเวลาจองถูกคืนให้อัตโนมัติ");
        }
    }
    public DispatchResult ConfirmPartner(string vehicleId)
    {
        lock (gate)
        {
            var vehicle = vehicles.Find(x => x.Id == vehicleId);
            if (vehicle is null || vehicle.FleetType != "PARTNER" || vehicle.Status != "UNCONFIRMED")
                return new(false,"รถนี้ไม่อยู่ในสถานะรอยืนยัน");
            vehicles[vehicles.IndexOf(vehicle)] = vehicle with { Status = "READY" };
            return new(true,"ยืนยันรถร่วมพร้อมใช้งานแล้ว");
        }
    }
}
