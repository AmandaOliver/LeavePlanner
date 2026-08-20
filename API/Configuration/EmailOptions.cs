using System.ComponentModel.DataAnnotations;

namespace LeavePlanner.Configuration;

public class EmailOptions : IValidatableObject
{
	public const string SectionName = "Email";

	// Defaults to off so an environment that has not been given mail credentials
	// cannot reach real employees; production opts in explicitly.
	public bool Enabled { get; set; } = false;

	public string Host { get; set; } = "smtp.gmail.com";

	[Range(1, 65535)]
	public int Port { get; set; } = 587;

	public bool UseSsl { get; set; } = true;

	public string? FromAddress { get; set; }

	public string? Password { get; set; }

	// Credentials matter only when sending is on, so local development and CI need none.
	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (!Enabled)
		{
			yield break;
		}

		if (string.IsNullOrWhiteSpace(FromAddress))
		{
			yield return new ValidationResult(
				"Email:FromAddress is required when Email:Enabled is true. See README.md > Configuration.",
				new[] { nameof(FromAddress) });
		}

		if (string.IsNullOrWhiteSpace(Password))
		{
			yield return new ValidationResult(
				"Email:Password is required when Email:Enabled is true. See README.md > Configuration.",
				new[] { nameof(Password) });
		}
	}
}
