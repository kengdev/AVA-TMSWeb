namespace TMSWeb.Models
{
    public class TimeAttendanceViewModel
    {
        public DateTime punchtime { get; set; }
        public string terminal_alias { get; set; }
        public string area_alias { get; set; }
        public string emp_code { get; set; }
        public string emp_name { get; set; }
        public string emp_img { get; set; }
        public string status { get; set; }
    }
    public class AttendanceViewModel
    {
        public int total { get; set; }
        public int absent { get; set; }
        public int late { get; set; }
        public int leave { get; set; }
        public List<EmployeeAttendanceViewModel> employee { get; set; }

        public AttendanceViewModel()
        {
            employee = new List<EmployeeAttendanceViewModel>();
        }
    }

    public class EmployeeAttendanceViewModel
    {
        public string emp_code { get; set; }
        public string emp_fullname { get; set; }
        public Dictionary<string, DailyAttendanceViewModel> Days { get; set; } = new();
    }

    public class DailyAttendanceViewModel
    {
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }// เช่น "08:00-17:00" หรือ "ขาด"
        public string Status { get; set; }     // เช่น "normal", "late", "absent"
        public bool IsHoliday { get; set; }
        public string HolidayName { get; set; }
    }

    public class EmployeeLeaveViewModel()
    {
        public long leave_id { get; set; }
        public string emp_code { get; set; }
        public string emp_fullname { get; set; }
        public string dept_name { get; set; }
        public string section_name { get; set; }
        public string leave_type_code { get; set; }
        public string leave_type_name { get; set; }
        public string start_leave_date { get; set; }
        public string end_leave_date { get; set; }
        public string leave_status { get; set; }
    }

    public enum LeaveSegment
    {
        None = 0,
        Full = 1,
        HalfMorning = 2, // M
        HalfEvening = 3  // E
    }

    public class LeaveDay
    {
        public DateOnly Date { get; set; }
        public LeaveSegment Segment { get; set; }
        public string LeaveType { get; set; } // เผื่อใช้คิดเงินลาตามประเภท
    }
}
