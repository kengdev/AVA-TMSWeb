using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class AttLeaveRequestDay
{
    public long LeaveDayId { get; set; }

    public long LeaveId { get; set; }

    public string EmpCode { get; set; } = null!;

    public DateOnly WorkDate { get; set; }

    public string SegCode { get; set; } = null!;

    public decimal DayEqiv { get; set; }

    public string LeaveTypeCode { get; set; } = null!;

    public virtual AttLeaveRequest Leave { get; set; } = null!;

    public virtual AttLeaveType LeaveTypeCodeNavigation { get; set; } = null!;

    public virtual AttLeaveSeg SegCodeNavigation { get; set; } = null!;
}
