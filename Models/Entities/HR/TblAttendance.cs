using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblAttendance
{
    public string AttendanceCode { get; set; } = null!;

    public string AttendanceTypeCode { get; set; } = null!;

    public string EmpCode { get; set; } = null!;

    public DateTime WorkedInDate { get; set; }

    public DateTime? WorkedOutDate { get; set; }

    public decimal? Lat { get; set; }

    public decimal? Lon { get; set; }

    public decimal? Distance { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblAttendanceType AttendanceTypeCodeNavigation { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;
}
