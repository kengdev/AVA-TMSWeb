using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class AttTransactionRequest
{
    public int Id { get; set; }

    public string EmpCode { get; set; } = null!;

    public DateOnly WorkDate { get; set; }

    public bool IsTimein { get; set; }

    public bool IsTimeout { get; set; }

    public TimeOnly? TimeIn { get; set; }

    public TimeOnly? TimeOut { get; set; }

    public string? Remark { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;
}
