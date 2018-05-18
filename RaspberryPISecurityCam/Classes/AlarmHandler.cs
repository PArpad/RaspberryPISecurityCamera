using log4net;
using RaspberryPiSecurityCam.Services;
using RaspberryPISecurityCam.Enums;
using RaspberryPISecurityCam.Interfaces;
using RaspberryPISecurityCam.Models.HomeViewModels;
using RaspberryPISecurityCam.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Classes
{
    public class AlarmHandler
    {
        private ILog _logger;
        private string _targetDirectory;
        private IEmailSenderFactory _emailSenderFactory;

        public AlarmHandler(IEmailSenderFactory emailSenderFactory)
        {
            _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _targetDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            _emailSenderFactory = emailSenderFactory;
        }

        public async Task RunOperations()
        {
            _logger.Debug("RunOperations started");
            var LastUploadTime = OperationMode.LastSentDate;
            var filesToUpload = GetFilesToUpload(LastUploadTime);
            try
            {
                OperationMode.DetermineCurrentMode();
                AlarmSettings model = SettingsHandler.GetModelFromJSON<AlarmSettings>();
                var settings = GetCurrentOperationModeSettings(model);

                if (filesToUpload.Count() != 0)
                {
                    try
                    {
                        if (settings.AlarmEmailProvider.IsEmailSelected)
                        {
                            List<string> attachments = new List<string>();
                            string subject = String.Format("Motion Detected at {0}", DateTime.Now);
                            string body = String.Format("Your security camera detected motion at {0}.\nPictures and video can be found in the attachments.", DateTime.Now);
                            var adminSettings = SettingsHandler.GetModelFromJSON<AdminSettings>();
                            double sizeOfAttachments = 0.0;
                            foreach (var file in filesToUpload)
                            {
                                sizeOfAttachments += file.Length / 1024f / 1024f;
                                if (sizeOfAttachments < 25.0)
                                {
                                    attachments.Add(file.FullName);
                                }
                            }
                            await _emailSenderFactory.GetEmailSender().SendEmailAsync(String.Join(";",adminSettings.emailAddresses), subject, body, attachments);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, ex);
                    }
                }
                foreach (var file in filesToUpload)
                {
                    try
                    {
                        if (settings.AlarmCloudStorage.FirstOrDefault(x => x.cloudStorageProviderEnum == CloudStorageProviderEnum.GoogleDrive).IsSelected)
                        {
                            var adminSettings = SettingsHandler.GetModelFromJSON<AdminSettings>();
                            string ToUploadFileName = file.Name;
                            string ToUploadFilePath = file.FullName;
                            GoogleDriveServices googleDriveService = new GoogleDriveServices();
                            await googleDriveService.GoogleDriveUploadAsync(ToUploadFileName, ToUploadFilePath, "RaspberryPICamera", adminSettings.emailAddresses);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, ex);
                    }
                    try
                    {
                        if (settings.AlarmSaveToSd.IsSdSelected)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, ex);
                    }
                    try
                    {
                        if (settings.AlarmLogging.FirstOrDefault(x => x.loggerProviderEnum == LoggerProviderEnum.ToDatabase).IsSelected)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            OperationMode.LastSentDate = DateTime.Now;
        }

        private AlarmType GetCurrentOperationModeSettings(AlarmSettings model)
        {
            if (model != null)
            {
                OperationMode.DetermineCurrentMode();
                var currentMode = OperationMode.getCurrentMode();
                return model.AlarmTypes.FirstOrDefault(x => x.AlarmCondition == currentMode.ToString());
            }
            else { return null; }
        }

        private List<FileInfo> GetFilesToUpload(DateTime LastUploadTime)
        {
            List<FileInfo> result = new List<FileInfo>();

            var directory = new DirectoryInfo(_targetDirectory);

            foreach (var file in directory.GetFiles())
            {
                if (file.LastWriteTime > LastUploadTime && !file.Name.Contains("Mask"))
                {
                    result.Add(file);
                }
            }
            return result;
        }
    }
}
