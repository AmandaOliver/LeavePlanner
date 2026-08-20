using FluentEmail.Core;
using FluentEmail.Smtp;
using LeavePlanner.Configuration;
using LeavePlanner.Domain;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace LeavePlanner.Infrastructure.Email;

public class EmailSender : IEmailSender
{
	private readonly EmailOptions _options;
	private readonly ILogger<EmailSender> _logger;

	public EmailSender(IOptions<EmailOptions> options, ILogger<EmailSender> logger)
	{
		_options = options.Value;
		_logger = logger;
	}

	public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
	{
		if (!_options.Enabled)
		{
			_logger.LogInformation("Email sending is disabled; skipped \"{Subject}\" to {Recipient}.", subject, toEmail);
			return;
		}

		var fromAddress = _options.FromAddress!;

		var sender = new SmtpSender(() => new SmtpClient(_options.Host)
		{
			UseDefaultCredentials = false,
			Credentials = new System.Net.NetworkCredential(fromAddress, _options.Password),
			EnableSsl = _options.UseSsl,
			Port = _options.Port
		});

		FluentEmail.Core.Email.DefaultSender = sender;

		var email = await FluentEmail.Core.Email
			.From(fromAddress)
			.To(toEmail)
			.Subject(subject)
			.Body(body)
			.SendAsync(cancellationToken);

		if (!email.Successful)
		{
			_logger.LogError("Failed to send \"{Subject}\" to {Recipient}: {Errors}",
				subject, toEmail, string.Join("; ", email.ErrorMessages));
		}
	}
}
