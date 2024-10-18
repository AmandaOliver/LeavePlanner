using FluentEmail.Core;
using FluentEmail.Smtp;
using System.Net.Mail;

public class EmailService
{
	public async Task SendEmail(string toEmail, string subject, string body)
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
