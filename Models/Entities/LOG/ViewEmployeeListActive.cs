using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewEmployeeListActive
{
    public string EmpCode { get; set; } = null!;

    public string InitialName { get; set; } = null!;

    public string EmpFullname { get; set; } = null!;

    public string IdCard { get; set; } = null!;

    public string IdLine { get; set; } = null!;

    public string EmpPhone { get; set; } = null!;

    public string EmpMail { get; set; } = null!;

    public bool IsActived { get; set; }
}
