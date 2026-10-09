using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblAttendanceDetail
{
    public int Id { get; set; }

    public string Remark { get; set; } = null!;

    public string AttendanceCode { get; set; } = null!;
}
