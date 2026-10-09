using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblAttendanceType
{
    public string AttendanceTypeCode { get; set; } = null!;

    public string AttendanceTypeName { get; set; } = null!;

    public string Color { get; set; } = null!;

    public virtual ICollection<TblAttendance> TblAttendances { get; set; } = new List<TblAttendance>();

    public virtual ICollection<TblLeave> TblLeaves { get; set; } = new List<TblLeave>();
}
