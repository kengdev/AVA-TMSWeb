using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class ViewHrDriverListActive
{
    public string? TruckNo { get; set; }

    public string? TruckLicense { get; set; }

    public string EmpCode { get; set; } = null!;

    public string DriverCode { get; set; } = null!;

    public string EmpFullname { get; set; } = null!;

    public string IdCard { get; set; } = null!;

    public string IdLine { get; set; } = null!;

    public string EmpPhone { get; set; } = null!;

    public string EmpMail { get; set; } = null!;

    public string? EmpAddress { get; set; }

    public string? OriginalAddress { get; set; }

    public string EmpGender { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public DateOnly StartDate { get; set; }

    public string EmpImg { get; set; } = null!;

    public string EmpAvatar { get; set; } = null!;

    public string? EmpIdcard { get; set; }

    public string StatusCode { get; set; } = null!;

    public string? TruckCode { get; set; }

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public string StatusName { get; set; } = null!;
}
