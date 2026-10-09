using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class ViewHrOfficer
{
    public string EmpCode { get; set; } = null!;

    public string InitialName { get; set; } = null!;

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

    public bool IsActived { get; set; }

    public string DeptCode { get; set; } = null!;

    public string DeptName { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public bool IsEnabled { get; set; }
}
