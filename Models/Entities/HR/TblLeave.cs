using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblLeave
{
    public string LeaveCode { get; set; } = null!;

    public string EmpCode { get; set; } = null!;

    public string AttendanceTypeCode { get; set; } = null!;

    public DateOnly LeaveStartdate { get; set; }

    public DateOnly LeaveEnddate { get; set; }

    public string? Remark { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblAttendanceType AttendanceTypeCodeNavigation { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;
}
