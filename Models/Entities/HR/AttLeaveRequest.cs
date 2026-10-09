using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class AttLeaveRequest
{
    public long LeaveId { get; set; }

    public string EmpCode { get; set; } = null!;

    public string LeaveTypeCode { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public string StartSeg { get; set; } = null!;

    public DateOnly EndDate { get; set; }

    public string EndSeg { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public virtual ICollection<AttLeaveRequestDay> AttLeaveRequestDays { get; set; } = new List<AttLeaveRequestDay>();

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;

    public virtual AttLeaveSeg EndSegNavigation { get; set; } = null!;

    public virtual AttLeaveType LeaveTypeCodeNavigation { get; set; } = null!;

    public virtual AttLeaveSeg StartSegNavigation { get; set; } = null!;
}
