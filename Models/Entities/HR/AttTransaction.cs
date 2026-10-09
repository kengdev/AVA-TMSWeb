using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class AttTransaction
{
    public long Id { get; set; }

    public string EmpCode { get; set; } = null!;

    public DateOnly WorkDate { get; set; }

    public DateTime? TimeIn { get; set; }

    public DateTime? TimeOut { get; set; }

    public decimal WorkHour { get; set; }

    public string AttOntime { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }
}
