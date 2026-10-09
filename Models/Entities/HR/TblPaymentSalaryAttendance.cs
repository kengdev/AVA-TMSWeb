using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblPaymentSalaryAttendance
{
    public long Id { get; set; }

    public DateOnly WorkDate { get; set; }

    public TimeOnly WorkStart { get; set; }

    public TimeOnly WorkEnd { get; set; }

    public string DailyType { get; set; } = null!;

    public decimal Worked { get; set; }

    public decimal HolidayWorked { get; set; }

    public decimal Hours { get; set; }

    public decimal Ot { get; set; }

    public decimal OtHoliday { get; set; }

    public int Late { get; set; }

    public string? Note { get; set; }

    public string? LeaveInfo { get; set; }

    public bool IsHoliday { get; set; }

    public bool IsSunday { get; set; }

    public string EmpCode { get; set; } = null!;

    public string PaymentCode { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;

    public virtual TblPaymentSalary PaymentCodeNavigation { get; set; } = null!;

    public virtual TblPaymentSalaryDetail TblPaymentSalaryDetail { get; set; } = null!;
}
