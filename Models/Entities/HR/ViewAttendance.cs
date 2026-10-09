using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class ViewAttendance
{
    public string EmpCode { get; set; } = null!;

    public string EmpFullname { get; set; } = null!;

    public DateOnly PunchDate { get; set; }

    public DateTime PunchIn { get; set; }

    public DateTime? PunchOut { get; set; }

    public decimal WorkHour { get; set; }

    public string AttOntime { get; set; } = null!;
}
