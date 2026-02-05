using MailKit.Security;
using MimeKit;
using Platform_Kastam.Helpers;
using System.Net.Mail;
using MailKit.Net.Smtp;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Cms;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Platform_Kastam.Services
{
    public class EmailService: IEmailService
    {

        MimeMessage emailMessage;
        BodyBuilder builder;

        public EmailService()
        {
            emailMessage = new MimeMessage();
            builder = new BodyBuilder();
        }


        public async Task SendEmailByService(MailRequest mailrequest)
        {
            var email = new MimeMessage();
            var builder = new BodyBuilder();

            email.From.Add(new MailboxAddress("Integrasi Data Kastam", "dataKastam@lgm.gov.my")); // sender
            email.To.Add(new MailboxAddress("", mailrequest.ToEmail)); // recipient 
            email.Subject = mailrequest.Subject;

            builder.HtmlBody = mailrequest.Body;
            email.Body = builder.ToMessageBody();

            try
            {
                using (var smtp = new SmtpClient())
                {
                    smtp.LocalDomain = "lgm.gov.my";

                    // Hardcoded values from your settings
                    string host = "postmaster2.mygovuc.gov.my";
                    int port = 25;

                    await smtp.ConnectAsync(host, port, SecureSocketOptions.None).ConfigureAwait(false);
                    await smtp.SendAsync(email).ConfigureAwait(false);
                    await smtp.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // IMPORTANT: surface the real SMTP error
                throw new Exception("Email sending failed: " + ex.Message, ex);
            }
        }
    }
}
