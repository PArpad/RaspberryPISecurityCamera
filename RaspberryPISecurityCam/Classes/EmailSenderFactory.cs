using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaspberryPISecurityCam.Enums;
using RaspberryPISecurityCam.Interfaces;
using RaspberryPISecurityCam.Models.HomeViewModels;
using RaspberryPISecurityCam.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Classes
{
    public class EmailSenderFactory : IEmailSenderFactory
    {
        private readonly EmailSenderSMTP _emailSenderSMTP;
        private readonly EmailSenderGmailAPI _emailSenderGmailAPI;
        public EmailSenderFactory(EmailSenderSMTP emailSenderSMTP, EmailSenderGmailAPI emailSenderGmailAPI)
        {
            _emailSenderSMTP = emailSenderSMTP;
            _emailSenderGmailAPI = emailSenderGmailAPI;
        }
        public IEmailSender GetEmailSender()
        {
            var alarmSettings = SettingsHandler.GetModelFromJSON<AlarmSettings>();
            if (alarmSettings != null)
            {
                var currentMode = OperationMode.getCurrentMode();

                var currentSettings = alarmSettings.AlarmTypes.First(m => m.AlarmCondition == currentMode.ToString());

                if (currentSettings.AlarmEmailProvider.SelectedEmailSender == EmailSenderEnum.SMTP.ToString())
                {
                    return _emailSenderSMTP;
                }
                else if (currentSettings.AlarmEmailProvider.SelectedEmailSender == EmailSenderEnum.GmailAPI.ToString())
                {
                    return _emailSenderGmailAPI;
                }
            }
            return null;
        }
    }
}
