using System;
using System.Collections.Generic;
using LeavePlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Data;

public partial class LeavePlannerContext : DbContext
{
    public LeavePlannerContext()
    {
    }

    public LeavePlannerContext(DbContextOptions<LeavePlannerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Leave> Leaves { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySQL("server=127.0.0.1;port=3306;user=root;password=;database=LeavePlanner;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity.ToTable("Countries", "LeavePlanner");

            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Email).HasName("PRIMARY");

            entity.ToTable("Employees", "LeavePlanner");

            entity.HasIndex(e => e.ManagedBy, "fk_employee_managedBy");

            entity.HasIndex(e => e.Organization, "idx_organization");

            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .HasColumnName("country");
            entity.Property(e => e.IsOrgOwner)
                .HasDefaultValueSql("'0'")
                .HasColumnName("isOrgOwner");
            entity.Property(e => e.ManagedBy).HasColumnName("managedBy");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Organization).HasColumnName("organization");
            entity.Property(e => e.PaidTimeOff)
                .HasDefaultValueSql("'0'")
                .HasColumnName("paidTimeOff");
            entity.HasOne(d => d.ManagedByNavigation).WithMany(p => p.InverseManagedByNavigation)
                .HasForeignKey(d => d.ManagedBy)
                .HasConstraintName("fk_employee_managedBy");

            entity.HasOne(d => d.OrganizationNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Organization)
                .HasConstraintName("fk_employee_organization");
        });

        modelBuilder.Entity<Leave>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Leaves", "LeavePlanner");

            entity.HasIndex(e => e.ApprovedBy, "fk_leave_approvedBy");
            entity.HasIndex(e => e.RejectedBy, "fk_leave_rejectedBy");


            entity.HasIndex(e => e.Owner, "idx_owner");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approvedBy");
            entity.Property(e => e.RejectedBy).HasColumnName("rejectedBy");

            entity.Property(e => e.DateEnd)
                .HasColumnType("datetime")
                .HasColumnName("dateEnd");
            entity.Property(e => e.DateStart)
                .HasColumnType("datetime")
                .HasColumnName("dateStart");
            entity.Property(e => e.Owner).HasColumnName("owner");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Type)
                .HasColumnType("enum('sickLeave','paidTimeOff','unpaidTimeOff','bankHoliday')")
                .HasColumnName("type");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.LeaveApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("fk_leave_approvedBy");
            entity.HasOne(d => d.RejectedByNavigation).WithMany(p => p.LeaveRejectedByNavigations)
                           .HasForeignKey(d => d.RejectedBy)
                           .HasConstraintName("fk_leave_rejectedBy");
            entity.HasOne(d => d.OwnerNavigation).WithMany(p => p.LeaveOwnerNavigations)
                .HasForeignKey(d => d.Owner)
                .HasConstraintName("fk_leave_owner");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Notifications", "LeavePlanner");

            entity.HasIndex(e => e.Creator, "fk_notification_creator");

            entity.HasIndex(e => e.LeaveId, "fk_notification_leave");

            entity.HasIndex(e => e.Recipient, "fk_notification_recipient");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Creator).HasColumnName("creator");
            entity.Property(e => e.LeaveId).HasColumnName("leaveId");
            entity.Property(e => e.Recipient).HasColumnName("recipient");

            entity.HasOne(d => d.CreatorNavigation).WithMany(p => p.NotificationCreatorNavigations)
                .HasForeignKey(d => d.Creator)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notification_creator");

            entity.HasOne(d => d.Leave).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.LeaveId)
                .HasConstraintName("fk_notification_leave");

            entity.HasOne(d => d.RecipientNavigation).WithMany(p => p.NotificationRecipientNavigations)
                .HasForeignKey(d => d.Recipient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_notification_recipient");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Organizations", "LeavePlanner");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
