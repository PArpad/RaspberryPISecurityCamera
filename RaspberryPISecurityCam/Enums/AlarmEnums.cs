using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Enums
{
    public enum AlarmConditionEnum
    {
        [DisplayName("Normal")]
        Normal,
        [DisplayName("No internet connection")]
        NoInternetConnection,
        [DisplayName("On UPS")]
        OnUPS,
        [DisplayName("SD card low on space")]
        SdLowOnSpace
    }
    public enum LoggerProviderEnum
    {
        [DisplayName("To database")]
        ToDatabase,
        [DisplayName("To file")]
        ToFile
    }
    public enum CloudStorageProviderEnum
    {
        [DisplayName("Google Drive")]
        GoogleDrive,
        [DisplayName("Dropbox")]
        Dropbox
    }
    public enum ActionToAlarmEnum
    {
        [DisplayName("Send email notification")]
        SendEmailNotification,
        [DisplayName("Save to SD card")]
        SaveToSdCard,
        [DisplayName("Save to cloud storage")]
        SaveToCloudStorage,
        [DisplayName("Logging")]
        Logging
    }
    public enum EmailSenderEnum
    {
        [DisplayName("Gmail API")]
        GmailAPI,
        [DisplayName("SMTP")]
        SMTP,
    }
}
