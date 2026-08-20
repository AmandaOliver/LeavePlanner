using LeavePlanner.Domain;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Data;

public partial class LeavePlannerContext : DbContext
{
	private readonly IConfiguration _configuration;

	public LeavePlannerContext(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public LeavePlannerContext(DbContextOptions<LeavePlannerContext> options, IConfiguration configuration)
		: base(options)
	{
		_configuration = configuration;
	}

	public virtual DbSet<Country> Countries { get; set; }

	public virtual DbSet<Employee> Employees { get; set; }

	public virtual DbSet<Leave> Leaves { get; set; }

	public virtual DbSet<Organization> Organizations { get; set; }

	public IEnumerable<AggregateRoot> TrackedAggregates() =>
		ChangeTracker.Entries()
			.Select(entry => entry.Entity)
			.OfType<AggregateRoot>()
			.ToList();

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var connectionString = _configuration.GetConnectionString("LeavePlannerDB");
		optionsBuilder.UseMySQL(connectionString);
	}

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
			entity.HasKey(e => e.Id).HasName("PRIMARY");
			entity.ToTable("Employees", "LeavePlanner");
			entity.HasIndex(e => e.ManagedBy, "fk_employee_managedBy");
			entity.HasIndex(e => e.Organization, "idx_organization");
			entity.Property(e => e.Id).HasColumnName("id");
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
			entity.Property(e => e.Title)
				.HasMaxLength(255)
				.HasColumnName("title");
			entity.Ignore(e => e.DomainEvents);
			entity.Ignore(e => e.IsOrgHead);
			entity.Ignore(e => e.IsActive);
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
			entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
			entity.Property(e => e.DateEnd)
				.HasColumnType("datetime")
				.HasColumnName("dateEnd");
			entity.Property(e => e.DateStart)
				.HasColumnType("datetime")
				.HasColumnName("dateStart");
			entity.Property(e => e.Owner).HasColumnName("owner");
			entity.Property(e => e.Description).HasColumnName("description");
			entity.Property(e => e.Type)
				.HasColumnType("enum('paidTimeOff','bankHoliday', 'statutoryLeave')")
				.HasColumnName("type");
			entity.Ignore(e => e.DomainEvents);
			entity.Ignore(e => e.Period);
			entity.Ignore(e => e.IsApproved);
			entity.Ignore(e => e.IsRejected);
			entity.Ignore(e => e.IsPending);

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

		modelBuilder.Entity<Organization>(entity =>
		{
			entity.HasKey(e => e.Id).HasName("PRIMARY");

			entity.ToTable("Organizations", "LeavePlanner");

			entity.Property(e => e.Id).HasColumnName("id");
			entity.Property(e => e.WorkingDays).HasColumnName("workingDays");

			entity.Property(e => e.Name)
				.HasMaxLength(255)
				.HasColumnName("name");
			entity.Ignore(e => e.DomainEvents);
		});

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
