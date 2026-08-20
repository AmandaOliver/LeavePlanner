using System.ComponentModel.DataAnnotations;

namespace LeavePlanner.Configuration;

/// <summary>
/// SMTP settings for outbound notification mail. Bound from the "Email" section.
/// <para>
/// <see cref="Password"/> is the only secret here and must never be committed. Supply it
/// through user-secrets locally and through the <c>Email__Password</c> environment
/// variable in every deployed environment. See README.md &gt; Configuration.
/// </para>
/// </summary>
public class EmailOptions : IValidatableObject
{
	public const string SectionName = "Email";

	/// <summary>
	/// When false, EmailService logs what it would have sent instead of
	/// sending it. Defaults to false so a misconfigured environment cannot mail real
	/// employees by accident — production must opt in explicitly.
	/// </summary>
	public bool Enabled { get; set; } = false;

	public string Host { get; set; } = "smtp.gmail.com";

	[Range(1, 65535)]
	public int Port { get; set; } = 587;

	public bool UseSsl { get; set; } = true;

	/// <summary>Mailbox notifications are sent from, and the SMTP username.</summary>
	public string? FromAddress { get; set; }

	/// <summary>SMTP password or provider app password. Secret.</summary>
	public string? Password { get; set; }

	/// <summary>
	/// Credentials are only required when sending is switched on, so local development
	/// and CI can run with no mail secrets at all.
	/// </summary>
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
				"Email:Password is required when Email:Enabled is true. Set it with " +
				"'dotnet user-secrets set \"Email:Password\" \"...\"' locally, or the " +
				"Email__Password environment variable when deployed. See README.md > Configuration.",
				new[] { nameof(Password) });
		}
	}
}
