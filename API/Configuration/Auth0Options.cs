using System.ComponentModel.DataAnnotations;

namespace LeavePlanner.Configuration;

public class Auth0Options
{
	public const string SectionName = "Auth0";

	/// <summary>Tenant domain, e.g. "your-tenant.eu.auth0.com" (no scheme, no trailing slash).</summary>
	[Required(ErrorMessage = "Auth0:Domain is required. See README.md > Configuration.")]
	public string Domain { get; set; } = default!;

	[Required(ErrorMessage = "Auth0:Audience is required. See README.md > Configuration.")]
	public string Audience { get; set; } = default!;

	// Null-tolerant so validation can report a missing Domain instead of throwing here.
	public string Authority => $"https://{Domain?.TrimEnd('/')}/";
}
