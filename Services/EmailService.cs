using MailKit.Net.Smtp;
using CrucibleBlog.Models;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CrucibleBlog.Services
{
	public class EmailService : IEmailSender
	{
		private readonly MailSettings _mailSettings;

		public EmailService(IOptions<MailSettings> mailSettings)
		{
			_mailSettings = mailSettings.Value; //dependency injection; slightly diff from other injections with other services
		}

		public async Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			try
			{
				var emailSender = _mailSettings.EmailAddress ?? Environment.GetEnvironmentVariable("EmailAddress");
				MimeMessage newEmail = new MimeMessage();
				newEmail.Sender = MailboxAddress.Parse(emailSender);
				{

					foreach (var emailAddress in email.Split(";"))
					{
						newEmail.To.Add(MailboxAddress.Parse(emailAddress));
					}

					newEmail.Subject = subject;

					BodyBuilder emailBody = new BodyBuilder(); //creat email using body builder
					emailBody.HtmlBody = htmlMessage; //using html to create body of email

					newEmail.Body = emailBody.ToMessageBody();

					using SmtpClient smtpClient = new SmtpClient();

					try
					{
						var host = _mailSettings.EmailHost ?? Environment.GetEnvironmentVariable("EmailHost");
						var port = _mailSettings.EmailPort != 0 ? _mailSettings.EmailPort : int.Parse(Environment.GetEnvironmentVariable("EmailPort")!);
						var password = _mailSettings.EmailPassword ?? Environment.GetEnvironmentVariable("EmailPassword");

						//these 3 lines send out an email
						await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
						await smtpClient.AuthenticateAsync(emailSender, password);

						await smtpClient.SendAsync(newEmail);
						await smtpClient.DisconnectAsync(true);
					}
					catch (Exception ex)
					{
						var error = ex.Message;
						throw;
					}

				}
			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
