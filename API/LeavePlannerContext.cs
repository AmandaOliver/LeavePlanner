using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner;

public partial class LeavePlannerContext : DbContext
{
    public LeavePlannerContext()
    {
    }

    public LeavePlannerContext(DbContextOptions<LeavePlannerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySQL("server=127.0.0.1;port=3306;user=root;password=;database=LeavePlanner");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Employee", "LeavePlanner");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
