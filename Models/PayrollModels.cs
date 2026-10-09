namespace TMSWeb.Models;

public class PayrollViewModel
{
    public string code { get; set; }
    public string start { get; set; }
    public string end { get; set; }
    public decimal net_total { get; set; }
    public bool is_approved { get; set; }
    public List<PayrollDetailViewModel> detail { get; set; }
}
public class PayrollDetailViewModel
{
    public string code { get; set; }
    public string emp_code { get; set; }
    public string emp_name { get; set; }
    public string nickname { get; set; }
    public string division { get; set; }
    public string department { get; set; }
    public string section { get; set; }
    public string position { get; set; }
    public string company { get; set; }
    public string emp_type { get; set; }
    public decimal total_worked { get; set; }
    public decimal work_days { get; set;  }
    public decimal ot_hours { get; set;  }

    // Income
    public decimal salary_base { get; set; }
    public decimal ot_base { get; set; }
    public decimal ot_wage { get; set; }
    public decimal ot_holiday { get; set; }
    public decimal salary_wage { get; set; }
    public decimal shift_allowance { get; set; }
    public decimal meal_allowance { get; set; }
    public decimal fuel_allowance { get; set; }
    public decimal dilg_allowance { get; set; }
    public decimal housing_allowance { get; set; }
    public decimal position_allowance { get; set; }
    public decimal OtherIncome { get; set; }      // รายได้อื่น ๆ
    public decimal TotalIncome { get; set; }       // รายได้รวม (เฉพาะช่องที่มีค่า)
    public decimal NetTotal { get; set; }

    // Deduction
    public int sso { get; set; }
    public decimal slf { get; set; }
    public decimal loan { get; set; }
    public decimal vat { get; set; }
    public decimal OtherDeduct { get; set; }
    public decimal TotalDeduct { get; set; }
    public int TotalAbsent { get; set; }
    public decimal TotalLeave { get; set; }
    public int TotalLate { get; set; }
    public List<EmpAttendanceViewModel> empAttendances { get; set; }
    public virtual ICollection<OtherPayment> OtherPayment { get; set; } = new List<OtherPayment>();
}

public class EmpAttendanceViewModel
{
    public string EmpCode { get; set; }
    public DateOnly WorkDate { get; set; }
    public string DailyType { get; set; }
    public decimal Worked { get; set; }
    public decimal HolidayWorked { get; set; }
    public decimal Hours { get; set; }
    public decimal OT { get; set; }
    public decimal OTHoliday { get; set; }
    public string Note { get; set; }
    public string LeaveInfo { get; set; }
    public bool IsHoliday { get; set; }
    public bool IsSunday { get; set; }
}

public class OtherPayment
{
    public long Id { get; set; }
    public string OtherType { get; set; }
    public string OtherName { get; set; }
    public decimal OtherValue { get; set; }
    public virtual PayrollDetailViewModel PayrollDetailViewModel { get; set; } = null!;
}