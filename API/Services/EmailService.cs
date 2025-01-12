using FluentEmail.Core;
using FluentEmail.Smtp;
using System.Net.Mail;

public class EmailService
{
	private readonly IConfiguration _configuration;
	private readonly string _preventEmails;
	public EmailService(IConfiguration configuration)
	{
		_configuration = configuration;
		_preventEmails = _configuration.GetValue<string>("ConnectionStrings:PreventEmails");

	}
	public async Task SendEmail(string toEmail, string subject, string body)
	{
		if (_preventEmails != "true")
		{
			var sender = new SmtpSender(() => new SmtpClient("smtp.gmail.com")
			{
				UseDefaultCredentials = false,
				Credentials = new System.Net.NetworkCredential("notifications.leaveplanner@gmail.com", "gcoa xeln xgks iqin"),
				EnableSsl = true,
				Port = 587
			});

			Email.DefaultSender = sender;

			var email = await Email
				.From("notifications.leaveplanner@gmail.com")
				.To(toEmail)
				.Subject(subject)
				.Body(body)
				.SendAsync();

		}
	}
}
