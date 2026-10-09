using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblEmployee
{
    public string EmpCode { get; set; } = null!;

    public string InitialName { get; set; } = null!;

    public string EmpFullname { get; set; } = null!;

    public string StatusCode { get; set; } = null!;

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<AttLeaveRequest> AttLeaveRequests { get; set; } = new List<AttLeaveRequest>();

    public virtual ICollection<AttPunchtime> AttPunchtimes { get; set; } = new List<AttPunchtime>();

    public virtual TblEmployeeStatus StatusCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblAttendance> TblAttendances { get; set; } = new List<TblAttendance>();

    public virtual ICollection<TblContract> TblContracts { get; set; } = new List<TblContract>();

    public virtual ICollection<TblEmployeeDepartment> TblEmployeeDepartments { get; set; } = new List<TblEmployeeDepartment>();

    public virtual TblEmployeeDetail? TblEmployeeDetail { get; set; }

    public virtual ICollection<TblEmployeeDocument> TblEmployeeDocuments { get; set; } = new List<TblEmployeeDocument>();

    public virtual ICollection<TblEmployeeDriver> TblEmployeeDrivers { get; set; } = new List<TblEmployeeDriver>();

    public virtual TblEmployeeFund? TblEmployeeFund { get; set; }

    public virtual TblEmployeeResign? TblEmployeeResign { get; set; }

    public virtual ICollection<TblEmployeeSalary> TblEmployeeSalaries { get; set; } = new List<TblEmployeeSalary>();

    public virtual ICollection<TblLeave> TblLeaves { get; set; } = new List<TblLeave>();

    public virtual ICollection<TblLoan> TblLoans { get; set; } = new List<TblLoan>();

    public virtual ICollection<TblPaymentSalaryAddition> TblPaymentSalaryAdditions { get; set; } = new List<TblPaymentSalaryAddition>();

    public virtual ICollection<TblPaymentSalaryAttendance> TblPaymentSalaryAttendances { get; set; } = new List<TblPaymentSalaryAttendance>();

    public virtual ICollection<TblPaymentSalaryDediction> TblPaymentSalaryDedictions { get; set; } = new List<TblPaymentSalaryDediction>();

    public virtual ICollection<TblPaymentSalaryDetail> TblPaymentSalaryDetails { get; set; } = new List<TblPaymentSalaryDetail>();
}
