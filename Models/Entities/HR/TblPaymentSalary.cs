using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblPaymentSalary
{
    public string PaymentCode { get; set; } = null!;

    public string PaymentType { get; set; } = null!;

    public string PaymentMonth { get; set; } = null!;

    public string PaymentYear { get; set; } = null!;

    public decimal PaymentTotal { get; set; }

    public int PaymentAmount { get; set; }

    public DateOnly PaymentDate { get; set; }

    public DateOnly PaymentStart { get; set; }

    public DateOnly PaymentEnd { get; set; }

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblPaymentSalaryAddition> TblPaymentSalaryAdditions { get; set; } = new List<TblPaymentSalaryAddition>();

    public virtual ICollection<TblPaymentSalaryAttendance> TblPaymentSalaryAttendances { get; set; } = new List<TblPaymentSalaryAttendance>();

    public virtual ICollection<TblPaymentSalaryDediction> TblPaymentSalaryDedictions { get; set; } = new List<TblPaymentSalaryDediction>();

    public virtual ICollection<TblPaymentSalaryDetail> TblPaymentSalaryDetails { get; set; } = new List<TblPaymentSalaryDetail>();
}
