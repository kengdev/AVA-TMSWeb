using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblPaymentSalaryDetail
{
    public string PaymentCode { get; set; } = null!;

    public string EmpCode { get; set; } = null!;

    public string SalaryType { get; set; } = null!;

    public decimal WorkedTotal { get; set; }

    public decimal AbsentTotal { get; set; }

    public decimal LeaveTotal { get; set; }

    public decimal LateTotal { get; set; }

    public decimal IncomingSalary { get; set; }

    public decimal IncomingDiligence { get; set; }

    public decimal IncomingTrip { get; set; }

    public decimal IncomingTotal { get; set; }

    public decimal IncomingOt { get; set; }

    public decimal IncomingPosition { get; set; }

    public decimal DedictionSso { get; set; }

    public decimal DedictionLate { get; set; }

    public decimal DedictionAbsent { get; set; }

    public decimal DedictionSecure { get; set; }

    public decimal DedictionTotal { get; set; }

    public decimal PaymentNetTotal { get; set; }

    public decimal PaymentTransfer { get; set; }

    public string? Remark { get; set; }

    public bool IsApprove { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;

    public virtual TblPaymentSalary PaymentCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblPaymentSalaryAttendance> TblPaymentSalaryAttendances { get; set; } = new List<TblPaymentSalaryAttendance>();
}
