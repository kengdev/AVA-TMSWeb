using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TMSWeb.Models.Entities.HR;

public partial class DbavahrContext : DbContext
{
    public DbavahrContext(DbContextOptions<DbavahrContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AttLeaveRequest> AttLeaveRequests { get; set; }

    public virtual DbSet<AttLeaveRequestDay> AttLeaveRequestDays { get; set; }

    public virtual DbSet<AttLeaveSeg> AttLeaveSegs { get; set; }

    public virtual DbSet<AttLeaveType> AttLeaveTypes { get; set; }

    public virtual DbSet<AttPunchtime> AttPunchtimes { get; set; }

    public virtual DbSet<AttTransaction> AttTransactions { get; set; }

    public virtual DbSet<AttTransactionRequest> AttTransactionRequests { get; set; }

    public virtual DbSet<IclockTerminal> IclockTerminals { get; set; }

    public virtual DbSet<IclockTransaction> IclockTransactions { get; set; }

    public virtual DbSet<SeGroup> SeGroups { get; set; }

    public virtual DbSet<SeGroupPriviledge01> SeGroupPriviledge01s { get; set; }

    public virtual DbSet<SeGroupPriviledge02> SeGroupPriviledge02s { get; set; }

    public virtual DbSet<SeGroupPriviledge03> SeGroupPriviledge03s { get; set; }

    public virtual DbSet<SeLevel01> SeLevel01s { get; set; }

    public virtual DbSet<SeLevel02> SeLevel02s { get; set; }

    public virtual DbSet<SeLevel03> SeLevel03s { get; set; }

    public virtual DbSet<TblAbsentType> TblAbsentTypes { get; set; }

    public virtual DbSet<TblAccount> TblAccounts { get; set; }

    public virtual DbSet<TblAttendance> TblAttendances { get; set; }

    public virtual DbSet<TblAttendanceDetail> TblAttendanceDetails { get; set; }

    public virtual DbSet<TblAttendanceType> TblAttendanceTypes { get; set; }

    public virtual DbSet<TblContract> TblContracts { get; set; }

    public virtual DbSet<TblContractType> TblContractTypes { get; set; }

    public virtual DbSet<TblDepartment> TblDepartments { get; set; }

    public virtual DbSet<TblEmployee> TblEmployees { get; set; }

    public virtual DbSet<TblEmployeeDepartment> TblEmployeeDepartments { get; set; }

    public virtual DbSet<TblEmployeeDetail> TblEmployeeDetails { get; set; }

    public virtual DbSet<TblEmployeeDocument> TblEmployeeDocuments { get; set; }

    public virtual DbSet<TblEmployeeDriver> TblEmployeeDrivers { get; set; }

    public virtual DbSet<TblEmployeeFund> TblEmployeeFunds { get; set; }

    public virtual DbSet<TblEmployeeIclock> TblEmployeeIclocks { get; set; }

    public virtual DbSet<TblEmployeeProbation> TblEmployeeProbations { get; set; }

    public virtual DbSet<TblEmployeeResign> TblEmployeeResigns { get; set; }

    public virtual DbSet<TblEmployeeSalary> TblEmployeeSalaries { get; set; }

    public virtual DbSet<TblEmployeeStatus> TblEmployeeStatuses { get; set; }

    public virtual DbSet<TblGroup> TblGroups { get; set; }

    public virtual DbSet<TblInitial> TblInitials { get; set; }

    public virtual DbSet<TblLeave> TblLeaves { get; set; }

    public virtual DbSet<TblLicenseType> TblLicenseTypes { get; set; }

    public virtual DbSet<TblLoan> TblLoans { get; set; }

    public virtual DbSet<TblLoanDetail> TblLoanDetails { get; set; }

    public virtual DbSet<TblPaymentSalary> TblPaymentSalaries { get; set; }

    public virtual DbSet<TblPaymentSalaryAddition> TblPaymentSalaryAdditions { get; set; }

    public virtual DbSet<TblPaymentSalaryAttendance> TblPaymentSalaryAttendances { get; set; }

    public virtual DbSet<TblPaymentSalaryDediction> TblPaymentSalaryDedictions { get; set; }

    public virtual DbSet<TblPaymentSalaryDetail> TblPaymentSalaryDetails { get; set; }

    public virtual DbSet<TblPaymentType> TblPaymentTypes { get; set; }

    public virtual DbSet<TblResignType> TblResignTypes { get; set; }

    public virtual DbSet<TblRestday> TblRestdays { get; set; }

    public virtual DbSet<TblSalaryType> TblSalaryTypes { get; set; }

    public virtual DbSet<TmpDriver> TmpDrivers { get; set; }

    public virtual DbSet<ViewAttendance> ViewAttendances { get; set; }

    public virtual DbSet<ViewEmployeeList> ViewEmployeeLists { get; set; }

    public virtual DbSet<ViewEmployeeListActive> ViewEmployeeListActives { get; set; }

    public virtual DbSet<ViewGroupPriviledgeLevel1> ViewGroupPriviledgeLevel1s { get; set; }

    public virtual DbSet<ViewGroupPriviledgeLevel2> ViewGroupPriviledgeLevel2s { get; set; }

    public virtual DbSet<ViewGroupPriviledgeLevel3> ViewGroupPriviledgeLevel3s { get; set; }

    public virtual DbSet<ViewHrDriverList> ViewHrDriverLists { get; set; }

    public virtual DbSet<ViewHrDriverListActive> ViewHrDriverListActives { get; set; }

    public virtual DbSet<ViewHrOfficer> ViewHrOfficers { get; set; }

    public virtual DbSet<ViewTruckListActive> ViewTruckListActives { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttLeaveRequest>(entity =>
        {
            entity.HasKey(e => e.LeaveId);

            entity.ToTable("att_leave_request");

            entity.Property(e => e.LeaveId).HasColumnName("leave_id");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.EndSeg)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("end_seg");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.LeaveTypeCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("leave_type_code");
            entity.Property(e => e.Reason)
                .HasMaxLength(200)
                .HasColumnName("reason");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.StartSeg)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("start_seg");
            entity.Property(e => e.Status)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("status");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.AttLeaveRequests)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_att_leave_request_tbl_employee");

            entity.HasOne(d => d.EndSegNavigation).WithMany(p => p.AttLeaveRequestEndSegNavigations)
                .HasForeignKey(d => d.EndSeg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_att_leave_request_att_leave_seg_end");

            entity.HasOne(d => d.LeaveTypeCodeNavigation).WithMany(p => p.AttLeaveRequests)
                .HasForeignKey(d => d.LeaveTypeCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_att_leave_request_att_leave_type");

            entity.HasOne(d => d.StartSegNavigation).WithMany(p => p.AttLeaveRequestStartSegNavigations)
                .HasForeignKey(d => d.StartSeg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_att_leave_request_att_leave_seg_start");
        });

        modelBuilder.Entity<AttLeaveRequestDay>(entity =>
        {
            entity.HasKey(e => e.LeaveDayId);

            entity.ToTable("att_leave_request_day");

            entity.Property(e => e.LeaveDayId).HasColumnName("leave_day_id");
            entity.Property(e => e.DayEqiv)
                .HasColumnType("decimal(10, 1)")
                .HasColumnName("day_eqiv");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.LeaveId).HasColumnName("leave_id");
            entity.Property(e => e.LeaveTypeCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("leave_type_code");
            entity.Property(e => e.SegCode)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("seg_code");
            entity.Property(e => e.WorkDate).HasColumnName("work_date");

            entity.HasOne(d => d.Leave).WithMany(p => p.AttLeaveRequestDays)
                .HasForeignKey(d => d.LeaveId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_att_leave_request_day_att_leave_request");

            entity.HasOne(d => d.LeaveTypeCodeNavigation).WithMany(p => p.AttLeaveRequestDays)
                .HasForeignKey(d => d.LeaveTypeCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_att_leave_request_day_att_leave_type");

            entity.HasOne(d => d.SegCodeNavigation).WithMany(p => p.AttLeaveRequestDays)
                .HasForeignKey(d => d.SegCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_att_leave_request_day_att_leave_seg");
        });

        modelBuilder.Entity<AttLeaveSeg>(entity =>
        {
            entity.HasKey(e => e.SegCode);

            entity.ToTable("att_leave_seg");

            entity.Property(e => e.SegCode)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("seg_code");
            entity.Property(e => e.SegName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("seg_name");
        });

        modelBuilder.Entity<AttLeaveType>(entity =>
        {
            entity.HasKey(e => e.LeaveTypeCode);

            entity.ToTable("att_leave_type");

            entity.Property(e => e.LeaveTypeCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("leave_type_code");
            entity.Property(e => e.DeductBalance).HasColumnName("deduct_balance");
            entity.Property(e => e.IsPaid).HasColumnName("is_paid");
            entity.Property(e => e.LeaveNameTh)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("leave_name_th");
            entity.Property(e => e.PaidDays)
                .HasColumnType("decimal(10, 1)")
                .HasColumnName("paid_days");
            entity.Property(e => e.RequiresDoc).HasColumnName("requires_doc");
        });

        modelBuilder.Entity<AttPunchtime>(entity =>
        {
            entity.ToTable("att_punchtime");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttOntime)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("att_ontime");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.TimeIn)
                .HasColumnType("datetime")
                .HasColumnName("time_in");
            entity.Property(e => e.TimeOut)
                .HasColumnType("datetime")
                .HasColumnName("time_out");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");
            entity.Property(e => e.WorkDate).HasColumnName("work_date");
            entity.Property(e => e.WorkHour)
                .HasColumnType("decimal(5, 1)")
                .HasColumnName("work_hour");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.AttPunchtimes)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_att_punchtime_tbl_employee");
        });

        modelBuilder.Entity<AttTransaction>(entity =>
        {
            entity.ToTable("att_transaction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttOntime)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("att_ontime");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.TimeIn)
                .HasColumnType("datetime")
                .HasColumnName("time_in");
            entity.Property(e => e.TimeOut)
                .HasColumnType("datetime")
                .HasColumnName("time_out");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");
            entity.Property(e => e.WorkDate).HasColumnName("work_date");
            entity.Property(e => e.WorkHour)
                .HasColumnType("decimal(5, 1)")
                .HasColumnName("work_hour");
        });

        modelBuilder.Entity<AttTransactionRequest>(entity =>
        {
            entity.ToTable("att_transaction_request");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.IsTimein).HasColumnName("is_timein");
            entity.Property(e => e.IsTimeout).HasColumnName("is_timeout");
            entity.Property(e => e.Remark)
                .HasColumnType("text")
                .HasColumnName("remark");
            entity.Property(e => e.TimeIn).HasColumnName("time_in");
            entity.Property(e => e.TimeOut).HasColumnName("time_out");
            entity.Property(e => e.WorkDate).HasColumnName("work_date");
        });

        modelBuilder.Entity<IclockTerminal>(entity =>
        {
            entity.HasKey(e => e.TerminalSn);

            entity.ToTable("iclock_terminal");

            entity.Property(e => e.TerminalSn)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("terminal_sn");
            entity.Property(e => e.AreaAlias)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("area_alias");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.TerminalAddress)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("terminal_address");
            entity.Property(e => e.TerminalAlias)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("terminal_alias");
        });

        modelBuilder.Entity<IclockTransaction>(entity =>
        {
            entity.ToTable("iclock_transaction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("emp_code");
            entity.Property(e => e.PunchTime)
                .HasColumnType("datetime")
                .HasColumnName("punch_time");
            entity.Property(e => e.TerminalAlias)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("terminal_alias");
            entity.Property(e => e.TerminalSn)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("terminal_sn");
            entity.Property(e => e.VerifyType).HasColumnName("verify_type");
        });

        modelBuilder.Entity<SeGroup>(entity =>
        {
            entity.HasKey(e => e.GroupCode);

            entity.ToTable("se_group");

            entity.Property(e => e.GroupCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("group_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.GroupName)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("group_name");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");
        });

        modelBuilder.Entity<SeGroupPriviledge01>(entity =>
        {
            entity.HasKey(e => new { e.GroupCode, e.Level01Code });

            entity.ToTable("se_group_priviledge_01");

            entity.Property(e => e.GroupCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("group_code");
            entity.Property(e => e.Level01Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_01_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");

            entity.HasOne(d => d.GroupCodeNavigation).WithMany(p => p.SeGroupPriviledge01s)
                .HasForeignKey(d => d.GroupCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_se_group_priviledge_01_tbl_group");

            entity.HasOne(d => d.Level01CodeNavigation).WithMany(p => p.SeGroupPriviledge01s)
                .HasForeignKey(d => d.Level01Code)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_se_group_priviledge_01_se_level_01");
        });

        modelBuilder.Entity<SeGroupPriviledge02>(entity =>
        {
            entity.HasKey(e => new { e.GroupCode, e.Level02Code });

            entity.ToTable("se_group_priviledge_02");

            entity.Property(e => e.GroupCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("group_code");
            entity.Property(e => e.Level02Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_02_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");

            entity.HasOne(d => d.GroupCodeNavigation).WithMany(p => p.SeGroupPriviledge02s)
                .HasForeignKey(d => d.GroupCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_se_group_priviledge_02_tbl_group");

            entity.HasOne(d => d.Level02CodeNavigation).WithMany(p => p.SeGroupPriviledge02s)
                .HasForeignKey(d => d.Level02Code)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_se_group_priviledge_02_se_level_02");
        });

        modelBuilder.Entity<SeGroupPriviledge03>(entity =>
        {
            entity.HasKey(e => new { e.GroupCode, e.Level03Code }).HasName("PK_se_group_priviledge");

            entity.ToTable("se_group_priviledge_03");

            entity.Property(e => e.GroupCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("group_code");
            entity.Property(e => e.Level03Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_03_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");

            entity.HasOne(d => d.GroupCodeNavigation).WithMany(p => p.SeGroupPriviledge03s)
                .HasForeignKey(d => d.GroupCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_se_group_priviledge_tbl_group");

            entity.HasOne(d => d.Level03CodeNavigation).WithMany(p => p.SeGroupPriviledge03s)
                .HasForeignKey(d => d.Level03Code)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_se_group_priviledge_se_level_03");
        });

        modelBuilder.Entity<SeLevel01>(entity =>
        {
            entity.HasKey(e => e.Level01Code).HasName("PK_tbl_level_01");

            entity.ToTable("se_level_01");

            entity.Property(e => e.Level01Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_01_code");
            entity.Property(e => e.Level01Active)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("level_01_active");
            entity.Property(e => e.Level01Icon)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("level_01_icon");
            entity.Property(e => e.Level01Index).HasColumnName("level_01_index");
            entity.Property(e => e.Level01Link)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("level_01_link");
            entity.Property(e => e.Level01Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("level_01_name");
        });

        modelBuilder.Entity<SeLevel02>(entity =>
        {
            entity.HasKey(e => e.Level02Code).HasName("PK_tbl_level_02");

            entity.ToTable("se_level_02");

            entity.Property(e => e.Level02Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_02_code");
            entity.Property(e => e.Level01Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_01_code");
            entity.Property(e => e.Level02Active)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("level_02_active");
            entity.Property(e => e.Level02Index)
                .HasDefaultValue(1, "DF_se_level_02_level_02_index")
                .HasColumnName("level_02_index");
            entity.Property(e => e.Level02Link)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("level_02_link");
            entity.Property(e => e.Level02Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("level_02_name");

            entity.HasOne(d => d.Level01CodeNavigation).WithMany(p => p.SeLevel02s)
                .HasForeignKey(d => d.Level01Code)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_level_02_tbl_level_01");
        });

        modelBuilder.Entity<SeLevel03>(entity =>
        {
            entity.HasKey(e => e.Level03Code);

            entity.ToTable("se_level_03");

            entity.Property(e => e.Level03Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_03_code");
            entity.Property(e => e.Level02Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_02_code");
            entity.Property(e => e.Level03Active)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("level_03_active");
            entity.Property(e => e.Level03Link)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("level_03_link");
            entity.Property(e => e.Level03Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("level_03_name");

            entity.HasOne(d => d.Level02CodeNavigation).WithMany(p => p.SeLevel03s)
                .HasForeignKey(d => d.Level02Code)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_se_level_03_se_level_02");
        });

        modelBuilder.Entity<TblAbsentType>(entity =>
        {
            entity.HasKey(e => e.AbsentTypeCode);

            entity.ToTable("tbl_absent_type");

            entity.Property(e => e.AbsentTypeCode)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("absent_type_code");
            entity.Property(e => e.AbsentTypeName)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("absent_type_name");
        });

        modelBuilder.Entity<TblAccount>(entity =>
        {
            entity.HasKey(e => e.AccUsername);

            entity.ToTable("tbl_account");

            entity.Property(e => e.AccUsername)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("acc_username");
            entity.Property(e => e.AccFullname)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("acc_fullname");
            entity.Property(e => e.AccPassword)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("acc_password");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.GroupCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("group_code");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.LineId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("line_id");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");

            entity.HasOne(d => d.GroupCodeNavigation).WithMany(p => p.TblAccounts)
                .HasForeignKey(d => d.GroupCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_account_tbl_group");
        });

        modelBuilder.Entity<TblAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceCode);

            entity.ToTable("tbl_attendance");

            entity.Property(e => e.AttendanceCode)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("attendance_code");
            entity.Property(e => e.AttendanceTypeCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("attendance_type_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.Distance)
                .HasColumnType("decimal(18, 6)")
                .HasColumnName("distance");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.Lat)
                .HasColumnType("decimal(18, 6)")
                .HasColumnName("lat");
            entity.Property(e => e.Lon)
                .HasColumnType("decimal(18, 6)")
                .HasColumnName("lon");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");
            entity.Property(e => e.WorkedInDate)
                .HasColumnType("datetime")
                .HasColumnName("worked_in_date");
            entity.Property(e => e.WorkedOutDate)
                .HasColumnType("datetime")
                .HasColumnName("worked_out_date");

            entity.HasOne(d => d.AttendanceTypeCodeNavigation).WithMany(p => p.TblAttendances)
                .HasForeignKey(d => d.AttendanceTypeCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_attendance_tbl_attendance_type");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblAttendances)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_attendance_tbl_employee");
        });

        modelBuilder.Entity<TblAttendanceDetail>(entity =>
        {
            entity.ToTable("tbl_attendance_detail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttendanceCode)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("attendance_code");
            entity.Property(e => e.Remark)
                .HasColumnType("text")
                .HasColumnName("remark");
        });

        modelBuilder.Entity<TblAttendanceType>(entity =>
        {
            entity.HasKey(e => e.AttendanceTypeCode);

            entity.ToTable("tbl_attendance_type");

            entity.Property(e => e.AttendanceTypeCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("attendance_type_code");
            entity.Property(e => e.AttendanceTypeName)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("attendance_type_name");
            entity.Property(e => e.Color)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("color");
        });

        modelBuilder.Entity<TblContract>(entity =>
        {
            entity.HasKey(e => e.ContractCode);

            entity.ToTable("tbl_contract");

            entity.Property(e => e.ContractCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("contract_code");
            entity.Property(e => e.ContractDate).HasColumnName("contract_date");
            entity.Property(e => e.ContractTypeCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("contract_type_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.Remark)
                .HasColumnType("text")
                .HasColumnName("remark");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");

            entity.HasOne(d => d.ContractTypeCodeNavigation).WithMany(p => p.TblContracts)
                .HasForeignKey(d => d.ContractTypeCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_contract_tbl_contract_type");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblContracts)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_contract_tbl_employee");
        });

        modelBuilder.Entity<TblContractType>(entity =>
        {
            entity.HasKey(e => e.ContractTypeCode);

            entity.ToTable("tbl_contract_type");

            entity.Property(e => e.ContractTypeCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("contract_type_code");
            entity.Property(e => e.ContractTypeName)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("contract_type_name");
        });

        modelBuilder.Entity<TblDepartment>(entity =>
        {
            entity.HasKey(e => e.DeptCode);

            entity.ToTable("tbl_department");

            entity.Property(e => e.DeptCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("dept_code");
            entity.Property(e => e.DeptName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("dept_name");
        });

        modelBuilder.Entity<TblEmployee>(entity =>
        {
            entity.HasKey(e => e.EmpCode);

            entity.ToTable("tbl_employee");

            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.EmpFullname)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("emp_fullname");
            entity.Property(e => e.InitialName)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("initial_name");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("status_code");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");

            entity.HasOne(d => d.StatusCodeNavigation).WithMany(p => p.TblEmployees)
                .HasForeignKey(d => d.StatusCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_employee_tbl_employee_status");
        });

        modelBuilder.Entity<TblEmployeeDepartment>(entity =>
        {
            entity.HasKey(e => new { e.EmpCode, e.DeptCode });

            entity.ToTable("tbl_employee_department");

            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.DeptCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("dept_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");

            entity.HasOne(d => d.DeptCodeNavigation).WithMany(p => p.TblEmployeeDepartments)
                .HasForeignKey(d => d.DeptCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_employee_department_tbl_department");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblEmployeeDepartments)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_employee_department_tbl_employee");
        });

        modelBuilder.Entity<TblEmployeeDetail>(entity =>
        {
            entity.HasKey(e => e.EmpCode).HasName("PK_tbl_employee_etail");

            entity.ToTable("tbl_employee_detail");

            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.EmpAddress)
                .HasColumnType("text")
                .HasColumnName("emp_address");
            entity.Property(e => e.EmpAvatar)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_avatar");
            entity.Property(e => e.EmpGender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_gender");
            entity.Property(e => e.EmpIdcard)
                .IsUnicode(false)
                .HasColumnName("emp_idcard");
            entity.Property(e => e.EmpImg)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_img");
            entity.Property(e => e.EmpMail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_mail");
            entity.Property(e => e.EmpPhone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_phone");
            entity.Property(e => e.IdCard)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("id_card");
            entity.Property(e => e.IdLine)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id_line");
            entity.Property(e => e.OriginalAddress)
                .HasColumnType("text")
                .HasColumnName("original_address");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.EmpCodeNavigation).WithOne(p => p.TblEmployeeDetail)
                .HasForeignKey<TblEmployeeDetail>(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_employee_detail_tbl_employee");
        });

        modelBuilder.Entity<TblEmployeeDocument>(entity =>
        {
            entity.ToTable("tbl_employee_document");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.DocumentDetail)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("document_detail");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("document_name");
            entity.Property(e => e.DocumentPath)
                .HasMaxLength(190)
                .IsUnicode(false)
                .HasColumnName("document_path");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("document_type");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblEmployeeDocuments)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_employee_document_tbl_employee");
        });

        modelBuilder.Entity<TblEmployeeDriver>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_employee_driver_PK");

            entity.ToTable("tbl_employee_driver");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.DriverCode)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("driver_code");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.LicenseTypeCode)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("license_type_code");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblEmployeeDrivers)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_employee_driver_tbl_employee");

            entity.HasOne(d => d.LicenseTypeCodeNavigation).WithMany(p => p.TblEmployeeDrivers)
                .HasForeignKey(d => d.LicenseTypeCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_employee_driver_tbl_license_type");
        });

        modelBuilder.Entity<TblEmployeeFund>(entity =>
        {
            entity.HasKey(e => e.EmpCode);

            entity.ToTable("tbl_employee_fund");

            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.SecureBalanceValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("secure_balance_value");
            entity.Property(e => e.SecureDeductValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("secure_deduct_value");
            entity.Property(e => e.SsoBalanceValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("sso_balance_value");
            entity.Property(e => e.SsoDeductValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("sso_deduct_value");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");

            entity.HasOne(d => d.EmpCodeNavigation).WithOne(p => p.TblEmployeeFund)
                .HasForeignKey<TblEmployeeFund>(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_employee_fund_tbl_employee");
        });

        modelBuilder.Entity<TblEmployeeIclock>(entity =>
        {
            entity.HasKey(e => new { e.EmpCode, e.IclockCode }).HasName("PK_tbl_employee_ref");

            entity.ToTable("tbl_employee_iclock");

            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.IclockCode)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("iclock_code");
        });

        modelBuilder.Entity<TblEmployeeProbation>(entity =>
        {
            entity.ToTable("tbl_employee_probation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContractCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("contract_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.FinishDate).HasColumnName("finish_date");
            entity.Property(e => e.ProbationDays).HasColumnName("probation_days");
            entity.Property(e => e.ProbationStatus)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("probation_status");
            entity.Property(e => e.Remark)
                .HasColumnType("text")
                .HasColumnName("remark");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.ContractCodeNavigation).WithMany(p => p.TblEmployeeProbations)
                .HasForeignKey(d => d.ContractCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_employee_probation_tbl_contract");
        });

        modelBuilder.Entity<TblEmployeeResign>(entity =>
        {
            entity.HasKey(e => e.EmpCode);

            entity.ToTable("tbl_employee_resign");

            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.ResignDate).HasColumnName("resign_date");
            entity.Property(e => e.ResignReason)
                .HasColumnType("text")
                .HasColumnName("resign_reason");
            entity.Property(e => e.ResigntypeCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("resigntype_code");

            entity.HasOne(d => d.EmpCodeNavigation).WithOne(p => p.TblEmployeeResign)
                .HasForeignKey<TblEmployeeResign>(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_employee_resign_tbl_employee");

            entity.HasOne(d => d.ResigntypeCodeNavigation).WithMany(p => p.TblEmployeeResigns)
                .HasForeignKey(d => d.ResigntypeCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_employee_resign_tbl_resign_type");
        });

        modelBuilder.Entity<TblEmployeeSalary>(entity =>
        {
            entity.HasKey(e => e.SalaryCode).HasName("PK_tbl_salary");

            entity.ToTable("tbl_employee_salary");

            entity.Property(e => e.SalaryCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("salary_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.SalaryDate).HasColumnName("salary_date");
            entity.Property(e => e.SalaryDetail)
                .HasColumnType("text")
                .HasColumnName("salary_detail");
            entity.Property(e => e.SalaryTypeCode)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("salary_type_code");
            entity.Property(e => e.SalaryValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("salary_value");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblEmployeeSalaries)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_salary_tbl_employee");

            entity.HasOne(d => d.SalaryTypeCodeNavigation).WithMany(p => p.TblEmployeeSalaries)
                .HasForeignKey(d => d.SalaryTypeCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_salary_tbl_salary_type");
        });

        modelBuilder.Entity<TblEmployeeStatus>(entity =>
        {
            entity.HasKey(e => e.StatusCode);

            entity.ToTable("tbl_employee_status");

            entity.Property(e => e.StatusCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("status_code");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<TblGroup>(entity =>
        {
            entity.HasKey(e => e.GroupCode);

            entity.ToTable("tbl_group");

            entity.Property(e => e.GroupCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("group_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.GroupName)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("group_name");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");
        });

        modelBuilder.Entity<TblInitial>(entity =>
        {
            entity.HasKey(e => e.InitialCode);

            entity.ToTable("tbl_initial");

            entity.Property(e => e.InitialCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("initial_code");
            entity.Property(e => e.InitialName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("initial_name");
        });

        modelBuilder.Entity<TblLeave>(entity =>
        {
            entity.HasKey(e => e.LeaveCode);

            entity.ToTable("tbl_leave");

            entity.Property(e => e.LeaveCode)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("leave_code");
            entity.Property(e => e.AttendanceTypeCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("attendance_type_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.LeaveEnddate).HasColumnName("leave_enddate");
            entity.Property(e => e.LeaveStartdate).HasColumnName("leave_startdate");
            entity.Property(e => e.Remark)
                .HasColumnType("text")
                .HasColumnName("remark");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");

            entity.HasOne(d => d.AttendanceTypeCodeNavigation).WithMany(p => p.TblLeaves)
                .HasForeignKey(d => d.AttendanceTypeCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_leave_tbl_attendance_type");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblLeaves)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_leave_tbl_employee");
        });

        modelBuilder.Entity<TblLicenseType>(entity =>
        {
            entity.HasKey(e => e.LicenseTypeCode);

            entity.ToTable("tbl_license_type");

            entity.Property(e => e.LicenseTypeCode)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("license_type_code");
            entity.Property(e => e.LicenseTypeName)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("license_type_name");
        });

        modelBuilder.Entity<TblLoan>(entity =>
        {
            entity.HasKey(e => e.LoanCode);

            entity.ToTable("tbl_loan");

            entity.Property(e => e.LoanCode)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("loan_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.LoanBalanceValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("loan_balance_value");
            entity.Property(e => e.LoanDedictValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("loan_dedict_value");
            entity.Property(e => e.LoanTotalValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("loan_total_value");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblLoans)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_loan_tbl_employee");
        });

        modelBuilder.Entity<TblLoanDetail>(entity =>
        {
            entity.ToTable("tbl_loan_detail");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.DedictValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("dedict_value");
            entity.Property(e => e.LoanCode)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("loan_code");
            entity.Property(e => e.Remark)
                .HasColumnType("text")
                .HasColumnName("remark");

            entity.HasOne(d => d.LoanCodeNavigation).WithMany(p => p.TblLoanDetails)
                .HasForeignKey(d => d.LoanCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_loan_detail_tbl_loan");
        });

        modelBuilder.Entity<TblPaymentSalary>(entity =>
        {
            entity.HasKey(e => e.PaymentCode).HasName("PK_tbl_payment_salary_1");

            entity.ToTable("tbl_payment_salary");

            entity.Property(e => e.PaymentCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("payment_code");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.PaymentAmount).HasColumnName("payment_amount");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.PaymentEnd).HasColumnName("payment_end");
            entity.Property(e => e.PaymentMonth)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("payment_month");
            entity.Property(e => e.PaymentStart).HasColumnName("payment_start");
            entity.Property(e => e.PaymentTotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("payment_total");
            entity.Property(e => e.PaymentType)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("payment_type");
            entity.Property(e => e.PaymentYear)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("payment_year");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");
        });

        modelBuilder.Entity<TblPaymentSalaryAddition>(entity =>
        {
            entity.ToTable("tbl_payment_salary_addition");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdditionType)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasDefaultValue("T", "DF_tbl_payment_salary_addition_addition_type")
                .HasColumnName("addition_type");
            entity.Property(e => e.AllowanceCode)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("allowance_code");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.PaymentAdditionName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("payment_addition_name");
            entity.Property(e => e.PaymentCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("payment_code");
            entity.Property(e => e.PaymentValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("payment_value");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblPaymentSalaryAdditions)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_payment_salary_addition_tbl_employee");

            entity.HasOne(d => d.PaymentCodeNavigation).WithMany(p => p.TblPaymentSalaryAdditions)
                .HasForeignKey(d => d.PaymentCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_payment_salary_addition_tbl_payment_salary");
        });

        modelBuilder.Entity<TblPaymentSalaryAttendance>(entity =>
        {
            entity.ToTable("tbl_payment_salary_attendance");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DailyType)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("daily_type");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.HolidayWorked)
                .HasColumnType("decimal(10, 1)")
                .HasColumnName("holiday_worked");
            entity.Property(e => e.Hours)
                .HasColumnType("decimal(10, 1)")
                .HasColumnName("hours");
            entity.Property(e => e.IsHoliday).HasColumnName("is_holiday");
            entity.Property(e => e.IsSunday).HasColumnName("is_sunday");
            entity.Property(e => e.Late).HasColumnName("late");
            entity.Property(e => e.LeaveInfo)
                .HasMaxLength(250)
                .HasColumnName("leave_info");
            entity.Property(e => e.Note)
                .HasMaxLength(250)
                .HasColumnName("note");
            entity.Property(e => e.Ot)
                .HasColumnType("decimal(10, 1)")
                .HasColumnName("ot");
            entity.Property(e => e.OtHoliday)
                .HasColumnType("decimal(10, 1)")
                .HasColumnName("ot_holiday");
            entity.Property(e => e.PaymentCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("payment_code");
            entity.Property(e => e.WorkDate).HasColumnName("work_date");
            entity.Property(e => e.WorkEnd).HasColumnName("work_end");
            entity.Property(e => e.WorkStart).HasColumnName("work_start");
            entity.Property(e => e.Worked)
                .HasColumnType("decimal(10, 1)")
                .HasColumnName("worked");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblPaymentSalaryAttendances)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_payment_salary_attendance_tbl_employee");

            entity.HasOne(d => d.PaymentCodeNavigation).WithMany(p => p.TblPaymentSalaryAttendances)
                .HasForeignKey(d => d.PaymentCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_payment_salary_attendance_tbl_payment_salary");

            entity.HasOne(d => d.TblPaymentSalaryDetail).WithMany(p => p.TblPaymentSalaryAttendances)
                .HasForeignKey(d => new { d.PaymentCode, d.EmpCode })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_payment_salary_attendance_tbl_payment_salary_detail");
        });

        modelBuilder.Entity<TblPaymentSalaryDediction>(entity =>
        {
            entity.ToTable("tbl_payment_salary_dediction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.PaymentCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("payment_code");
            entity.Property(e => e.PaymentDedictionName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("payment_dediction_name");
            entity.Property(e => e.PaymentValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("payment_value");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblPaymentSalaryDedictions)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_payment_salary_dediction_tbl_employee");

            entity.HasOne(d => d.PaymentCodeNavigation).WithMany(p => p.TblPaymentSalaryDedictions)
                .HasForeignKey(d => d.PaymentCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_payment_salary_dediction_tbl_payment_salary");
        });

        modelBuilder.Entity<TblPaymentSalaryDetail>(entity =>
        {
            entity.HasKey(e => new { e.PaymentCode, e.EmpCode });

            entity.ToTable("tbl_payment_salary_detail");

            entity.Property(e => e.PaymentCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("payment_code");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.AbsentTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("absent_total");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.DedictionAbsent)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("dediction_absent");
            entity.Property(e => e.DedictionLate)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("dediction_late");
            entity.Property(e => e.DedictionSecure)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("dediction_secure");
            entity.Property(e => e.DedictionSso)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("dediction_sso");
            entity.Property(e => e.DedictionTotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("dediction_total");
            entity.Property(e => e.IncomingDiligence)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("incoming_diligence");
            entity.Property(e => e.IncomingOt)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("incoming_ot");
            entity.Property(e => e.IncomingPosition)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("incoming_position");
            entity.Property(e => e.IncomingSalary)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("incoming_salary");
            entity.Property(e => e.IncomingTotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("incoming_total");
            entity.Property(e => e.IncomingTrip)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("incoming_trip");
            entity.Property(e => e.IsApprove).HasColumnName("is_approve");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.LateTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("late_total");
            entity.Property(e => e.LeaveTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("leave_total");
            entity.Property(e => e.PaymentNetTotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("payment_net_total");
            entity.Property(e => e.PaymentTransfer)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("payment_transfer");
            entity.Property(e => e.Remark)
                .HasColumnType("text")
                .HasColumnName("remark");
            entity.Property(e => e.SalaryType)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("salary_type");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");
            entity.Property(e => e.WorkedTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("worked_total");

            entity.HasOne(d => d.EmpCodeNavigation).WithMany(p => p.TblPaymentSalaryDetails)
                .HasForeignKey(d => d.EmpCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_payment_salary_tbl_employee");

            entity.HasOne(d => d.PaymentCodeNavigation).WithMany(p => p.TblPaymentSalaryDetails)
                .HasForeignKey(d => d.PaymentCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_payment_salary_detail_tbl_payment_salary");
        });

        modelBuilder.Entity<TblPaymentType>(entity =>
        {
            entity.HasKey(e => e.PaymentTypeCode);

            entity.ToTable("tbl_payment_type");

            entity.Property(e => e.PaymentTypeCode)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("payment_type_code");
            entity.Property(e => e.PaymentTypeName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("payment_type_name");
        });

        modelBuilder.Entity<TblResignType>(entity =>
        {
            entity.HasKey(e => e.ResigntypeCode);

            entity.ToTable("tbl_resign_type");

            entity.Property(e => e.ResigntypeCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("resigntype_code");
            entity.Property(e => e.ResigntypeName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("resigntype_name");
        });

        modelBuilder.Entity<TblRestday>(entity =>
        {
            entity.ToTable("tbl_restday");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.Remark)
                .HasColumnType("text")
                .HasColumnName("remark");
            entity.Property(e => e.RestdayEnd).HasColumnName("restday_end");
            entity.Property(e => e.RestdayName)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("restday_name");
            entity.Property(e => e.RestdayStart).HasColumnName("restday_start");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("update_date");
        });

        modelBuilder.Entity<TblSalaryType>(entity =>
        {
            entity.HasKey(e => e.SalaryTypeCode);

            entity.ToTable("tbl_salary_type");

            entity.Property(e => e.SalaryTypeCode)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("salary_type_code");
            entity.Property(e => e.SalaryTypeName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("salary_type_name");
        });

        modelBuilder.Entity<TmpDriver>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tmpDriver");

            entity.Property(e => e.DriverCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("driver_code");
        });

        modelBuilder.Entity<ViewAttendance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewAttendance");

            entity.Property(e => e.AttOntime)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("att_ontime");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.EmpFullname)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("emp_fullname");
            entity.Property(e => e.PunchDate).HasColumnName("punch_date");
            entity.Property(e => e.PunchIn)
                .HasColumnType("datetime")
                .HasColumnName("punch_in");
            entity.Property(e => e.PunchOut)
                .HasColumnType("datetime")
                .HasColumnName("punch_out");
            entity.Property(e => e.WorkHour)
                .HasColumnType("decimal(5, 1)")
                .HasColumnName("work_hour");
        });

        modelBuilder.Entity<ViewEmployeeList>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewEmployeeList");

            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.EmpAddress)
                .HasColumnType("text")
                .HasColumnName("emp_address");
            entity.Property(e => e.EmpAvatar)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_avatar");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.EmpFullname)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("emp_fullname");
            entity.Property(e => e.EmpGender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_gender");
            entity.Property(e => e.EmpIdcard)
                .IsUnicode(false)
                .HasColumnName("emp_idcard");
            entity.Property(e => e.EmpImg)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_img");
            entity.Property(e => e.EmpMail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_mail");
            entity.Property(e => e.EmpPhone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_phone");
            entity.Property(e => e.IdCard)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("id_card");
            entity.Property(e => e.InitialName)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("initial_name");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.OriginalAddress)
                .HasColumnType("text")
                .HasColumnName("original_address");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("status_code");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<ViewEmployeeListActive>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewEmployeeListActive");

            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.EmpAddress)
                .HasColumnType("text")
                .HasColumnName("emp_address");
            entity.Property(e => e.EmpAvatar)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_avatar");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.EmpFullname)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("emp_fullname");
            entity.Property(e => e.EmpGender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_gender");
            entity.Property(e => e.EmpIdcard)
                .IsUnicode(false)
                .HasColumnName("emp_idcard");
            entity.Property(e => e.EmpImg)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_img");
            entity.Property(e => e.EmpMail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_mail");
            entity.Property(e => e.EmpPhone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_phone");
            entity.Property(e => e.IdCard)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("id_card");
            entity.Property(e => e.InitialName)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("initial_name");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.OriginalAddress)
                .HasColumnType("text")
                .HasColumnName("original_address");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("status_code");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<ViewGroupPriviledgeLevel1>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewGroupPriviledgeLevel1");

            entity.Property(e => e.GroupCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("group_code");
            entity.Property(e => e.Level01Active)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("level_01_active");
            entity.Property(e => e.Level01Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_01_code");
            entity.Property(e => e.Level01Icon)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("level_01_icon");
            entity.Property(e => e.Level01Index).HasColumnName("level_01_index");
            entity.Property(e => e.Level01Link)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("level_01_link");
            entity.Property(e => e.Level01Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("level_01_name");
        });

        modelBuilder.Entity<ViewGroupPriviledgeLevel2>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewGroupPriviledgeLevel2");

            entity.Property(e => e.GroupCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("group_code");
            entity.Property(e => e.Level01Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_01_code");
            entity.Property(e => e.Level02Active)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("level_02_active");
            entity.Property(e => e.Level02Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_02_code");
            entity.Property(e => e.Level02Index).HasColumnName("level_02_index");
            entity.Property(e => e.Level02Link)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("level_02_link");
            entity.Property(e => e.Level02Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("level_02_name");
        });

        modelBuilder.Entity<ViewGroupPriviledgeLevel3>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewGroupPriviledgeLevel3");

            entity.Property(e => e.GroupCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("group_code");
            entity.Property(e => e.Level02Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_02_code");
            entity.Property(e => e.Level03Active)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("level_03_active");
            entity.Property(e => e.Level03Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("level_03_code");
            entity.Property(e => e.Level03Link)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("level_03_link");
            entity.Property(e => e.Level03Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("level_03_name");
        });

        modelBuilder.Entity<ViewHrDriverList>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewHrDriverList");

            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.DriverCode)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("driver_code");
            entity.Property(e => e.EmpAddress)
                .HasColumnType("text")
                .HasColumnName("emp_address");
            entity.Property(e => e.EmpAvatar)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_avatar");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.EmpFullname)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("emp_fullname");
            entity.Property(e => e.EmpGender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_gender");
            entity.Property(e => e.EmpIdcard)
                .IsUnicode(false)
                .HasColumnName("emp_idcard");
            entity.Property(e => e.EmpImg)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_img");
            entity.Property(e => e.EmpMail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_mail");
            entity.Property(e => e.EmpPhone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_phone");
            entity.Property(e => e.IdCard)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("id_card");
            entity.Property(e => e.IdLine)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id_line");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.OriginalAddress)
                .HasColumnType("text")
                .HasColumnName("original_address");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("status_code");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status_name");
            entity.Property(e => e.TruckCode)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("truck_code");
            entity.Property(e => e.TruckLicense)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("truck_license");
            entity.Property(e => e.TruckNo)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasColumnName("truck_no");
        });

        modelBuilder.Entity<ViewHrDriverListActive>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewHrDriverListActive");

            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.DriverCode)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("driver_code");
            entity.Property(e => e.EmpAddress)
                .HasColumnType("text")
                .HasColumnName("emp_address");
            entity.Property(e => e.EmpAvatar)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_avatar");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.EmpFullname)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("emp_fullname");
            entity.Property(e => e.EmpGender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_gender");
            entity.Property(e => e.EmpIdcard)
                .IsUnicode(false)
                .HasColumnName("emp_idcard");
            entity.Property(e => e.EmpImg)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_img");
            entity.Property(e => e.EmpMail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_mail");
            entity.Property(e => e.EmpPhone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_phone");
            entity.Property(e => e.IdCard)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("id_card");
            entity.Property(e => e.IdLine)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id_line");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.OriginalAddress)
                .HasColumnType("text")
                .HasColumnName("original_address");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("status_code");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status_name");
            entity.Property(e => e.TruckCode)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("truck_code");
            entity.Property(e => e.TruckLicense)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("truck_license");
            entity.Property(e => e.TruckNo)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasColumnName("truck_no");
        });

        modelBuilder.Entity<ViewHrOfficer>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewHrOfficer");

            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.DeptCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("dept_code");
            entity.Property(e => e.DeptName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("dept_name");
            entity.Property(e => e.EmpAddress)
                .HasColumnType("text")
                .HasColumnName("emp_address");
            entity.Property(e => e.EmpAvatar)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_avatar");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_code");
            entity.Property(e => e.EmpFullname)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("emp_fullname");
            entity.Property(e => e.EmpGender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("emp_gender");
            entity.Property(e => e.EmpIdcard)
                .IsUnicode(false)
                .HasColumnName("emp_idcard");
            entity.Property(e => e.EmpImg)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_img");
            entity.Property(e => e.EmpMail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_mail");
            entity.Property(e => e.EmpPhone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("emp_phone");
            entity.Property(e => e.IdCard)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("id_card");
            entity.Property(e => e.IdLine)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id_line");
            entity.Property(e => e.InitialName)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("initial_name");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.OriginalAddress)
                .HasColumnType("text")
                .HasColumnName("original_address");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("status_code");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<ViewTruckListActive>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("viewTruckListActive");

            entity.Property(e => e.BrandName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("brand_name");
            entity.Property(e => e.DriverCode)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("driver_code");
            entity.Property(e => e.FuelTypeName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("fuel_type_name");
            entity.Property(e => e.IsActived).HasColumnName("is_actived");
            entity.Property(e => e.SupplierCode)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("supplier_code");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("supplier_name");
            entity.Property(e => e.TruckChassisNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("truck_chassis_number");
            entity.Property(e => e.TruckCode)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("truck_code");
            entity.Property(e => e.TruckImage)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("truck_image");
            entity.Property(e => e.TruckLicense)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("truck_license");
            entity.Property(e => e.TruckNo)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasColumnName("truck_no");
            entity.Property(e => e.TruckTypeName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("truck_type_name");
            entity.Property(e => e.TruckYear)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("truck_year");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
