using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblPaymentSalaryDediction
{
    public int Id { get; set; }

    public string EmpCode { get; set; } = null!;

    public string PaymentDedictionName { get; set; } = null!;

    public decimal PaymentValue { get; set; }

    public string PaymentCode { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;

    public virtual TblPaymentSalary PaymentCodeNavigation { get; set; } = null!;
}
