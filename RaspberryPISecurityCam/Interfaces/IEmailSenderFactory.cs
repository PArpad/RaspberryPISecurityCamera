using RaspberryPISecurityCam.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Interfaces
{
    public interface IEmailSenderFactory
    {
        IEmailSender GetEmailSender();
    }
}
