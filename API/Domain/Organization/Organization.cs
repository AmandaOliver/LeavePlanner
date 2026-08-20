namespace LeavePlanner.Domain;

public class Organization : AggregateRoot
{
	private Organization()
	{
	}

	public int Id { get; private set; }

	public string Name { get; private set; } = null!;

	public int[] WorkingDays { get; private set; } = [1, 2, 3, 4, 5];

	public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

	public static Organization Create(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new DomainException("Invalid data.");
		}

		return new Organization { Name = name };
	}

	public static Organization Rehydrate(int id, string name, int[] workingDays)
	{
		return new Organization
		{
			Id = id,
			Name = name,
			WorkingDays = workingDays
		};
	}

	public void Rename(string name) => Name = name;

	public void ChangeWorkingDays(int[] days)
	{
		if (days.Length < 1 || !days.All(day => day >= 1 && day <= 7))
		{
			throw new DomainException("Working days must be defined.");
		}

		WorkingDays = days;
	}
}
