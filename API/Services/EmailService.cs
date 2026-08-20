using FluentEmail.Core;
using FluentEmail.Smtp;
using LeavePlanner.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Mail;

public class EmailService
{
	private readonly EmailOptions _options;
	private readonly ILogger<EmailService> _logger;

	public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
	{
		_options = options.Value;
		_logger = logger;
	}

	public async Task SendEmail(string toEmail, string subject, string body)
	{
		if (!_options.Enabled)
		{
			_logger.LogInformation("Email sending is disabled; skipped \"{Subject}\" to {Recipient}.", subject, toEmail);
			return;
		}

		// EmailOptions.Validate guarantees both are present whenever Enabled is true,
		// which the early return above has already established.
		var fromAddress = _options.FromAddress!;

		var sender = new SmtpSender(() => new SmtpClient(_options.Host)
		{
			UseDefaultCredentials = false,
			Credentials = new System.Net.NetworkCredential(fromAddress, _options.Password),
			EnableSsl = _options.UseSsl,
			Port = _options.Port
		});

		Email.DefaultSender = sender;

		var email = await Email
			.From(fromAddress)
			.To(toEmail)
			.Subject(subject)
			.Body(body)
			.SendAsync();

		if (!email.Successful)
		{
			// Notification delivery is best-effort: a failed email must not roll back the
			// leave operation that triggered it, so this is logged rather than thrown.
			_logger.LogError("Failed to send \"{Subject}\" to {Recipient}: {Errors}",
				subject, toEmail, string.Join("; ", email.ErrorMessages));
		}
	}
}
