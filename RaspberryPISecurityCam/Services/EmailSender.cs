using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;

namespace RaspberryPISecurityCam.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public static class EmailSenderHelper
    {
        public static MimeMessage SetupMessage(string ToAddress, string subject, string body, List<string> attachments)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("RaspberrySecurity", "poth.arpad@gmail.com"));
            emailMessage.To.Add(new MailboxAddress(ToAddress, ToAddress));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = body };
            var multipart = new Multipart("mixed");
            multipart.Add(new TextPart("html") { Text = body });

            if (attachments != null)
            {
                foreach (string path in attachments)
                {
                    var mimeType = MimeTypes.GetMimeType(path);
                    var attachment = new MimePart(mimeType)
                    {
                        Content = new MimeContent(System.IO.File.OpenRead(path), ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(path)
                    };

                    multipart.Add(attachment);
                }
            }
            emailMessage.Body = multipart;
            return emailMessage;
        }
    }
    public class EmailSenderGmailAPI : IEmailSender
    {
        public static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public Task SendEmailAsync(string ToAddress,string subject, string body, List<string> attachments)
        {
            string[] Scopes = { GmailService.Scope.GmailSend };
            string ApplicationName = "RaspberryPISecurityCam";
            UserCredential credential;


            using (var stream =
                new FileStream(Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "ClientSecrets", "client_secret_gmailapi.json")), FileMode.Open, FileAccess.Read))
            {
                string credPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "ClientSecrets", "GmailAPI.json"));

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");


            var emailMessage = EmailSenderHelper.SetupMessage(ToAddress, subject, body, attachments);

            Message message = new Message();
            message.Raw = Base64UrlEncode(emailMessage.ToString());
            service.Users.Messages.Send(message, "me").Execute();

            return Task.CompletedTask;
        }
    }
    public class EmailSenderSMTP : IEmailSender
    {
        private IConfiguration Configuration;
        public EmailSenderSMTP(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public Task SendEmailAsync(string ToAddress, string subject, string body, List<string> attachments)
        {
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate(Configuration["SMTPSettings:UserName"], Configuration["SMTPSettings:Password"]);
                var emailMessage = EmailSenderHelper.SetupMessage(ToAddress, subject, body, attachments);
                client.Send(emailMessage);
                client.Disconnect(true);
                return Task.CompletedTask;
            }
        }
    }
}
