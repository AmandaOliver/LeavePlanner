using System.ComponentModel.DataAnnotations;

namespace LeavePlanner.Configuration;

public class AppOptions
{
	public const string SectionName = "App";

	// Also the single allowed CORS origin.
	[Required(ErrorMessage = "App:FrontendUrl is required. See README.md > Configuration.")]
	[Url]
	public string FrontendUrl { get; set; } = default!;
}
