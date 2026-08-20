using System.ComponentModel.DataAnnotations;

namespace LeavePlanner.Configuration;

/// <summary>
/// Non-secret application settings. Bound from the "App" configuration section.
/// </summary>
public class AppOptions
{
	public const string SectionName = "App";

	/// <summary>
	/// Public URL of the frontend. Used as the CORS allowed origin and as the base
	/// for links embedded in notification emails.
	/// </summary>
	[Required(ErrorMessage = "App:FrontendUrl is required. See README.md > Configuration.")]
	[Url]
	public string FrontendUrl { get; set; } = default!;
}
