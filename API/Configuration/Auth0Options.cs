using System.ComponentModel.DataAnnotations;

namespace LeavePlanner.Configuration;

/// <summary>
/// Auth0 tenant settings. Bound from the "Auth0" configuration section.
/// None of these are secrets: the domain and audience are public identifiers that
/// appear in every token the browser already holds. They live in configuration so
/// that dev, test and production can point at different tenants without a code change.
/// </summary>
public class Auth0Options
{
	public const string SectionName = "Auth0";

	/// <summary>Tenant domain, e.g. "your-tenant.eu.auth0.com" (no scheme, no trailing slash).</summary>
	[Required(ErrorMessage = "Auth0:Domain is required. See README.md > Configuration.")]
	public string Domain { get; set; } = default!;

	/// <summary>API identifier configured in Auth0, e.g. "https://api.leaveplanner.org".</summary>
	[Required(ErrorMessage = "Auth0:Audience is required. See README.md > Configuration.")]
	public string Audience { get; set; } = default!;

	/// <summary>The issuer URL Auth0 stamps into tokens.</summary>
	public string Authority => $"https://{Domain?.TrimEnd('/')}/";
}
