using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class AttLeaveType
{
    public string LeaveTypeCode { get; set; } = null!;

    public string LeaveNameTh { get; set; } = null!;

    public decimal PaidDays { get; set; }

    public bool IsPaid { get; set; }

    public bool DeductBalance { get; set; }

    public bool RequiresDoc { get; set; }

    public virtual ICollection<AttLeaveRequestDay> AttLeaveRequestDays { get; set; } = new List<AttLeaveRequestDay>();

    public virtual ICollection<AttLeaveRequest> AttLeaveRequests { get; set; } = new List<AttLeaveRequest>();
}
