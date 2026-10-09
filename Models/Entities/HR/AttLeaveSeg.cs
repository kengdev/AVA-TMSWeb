using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class AttLeaveSeg
{
    public string SegCode { get; set; } = null!;

    public string SegName { get; set; } = null!;

    public virtual ICollection<AttLeaveRequestDay> AttLeaveRequestDays { get; set; } = new List<AttLeaveRequestDay>();

    public virtual ICollection<AttLeaveRequest> AttLeaveRequestEndSegNavigations { get; set; } = new List<AttLeaveRequest>();

    public virtual ICollection<AttLeaveRequest> AttLeaveRequestStartSegNavigations { get; set; } = new List<AttLeaveRequest>();
}
