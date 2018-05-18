using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string address, string subject, string body, List<string> attachments);
    }
}
