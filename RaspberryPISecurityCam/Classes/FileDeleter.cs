using log4net;
using Microsoft.Extensions.Logging;
using MimeKit;
using RaspberryPISecurityCam.Models.HomeViewModels;
using RaspberryPISecurityCam.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Classes
{
    public class FileDeleter
    {
        private ILog _logger;
        private string _targetDirectory;

        public FileDeleter()
        {
            _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _targetDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images");
        }

        public async Task DeleteFiles()
        {
            _logger.Debug("DeleteFiles started");
            int numberOfFilesDeleted = 0;
            try
            {
                List<FileInfo> filesToDelete = new List<FileInfo>();
                double sizeThreshold = 0.0;
                DateTime dateThreshold = DateTime.MinValue;
                AdminSettings model = SettingsHandler.GetModelFromJSON<AdminSettings>();
                if (model != null)
                {
                    if (model.isDeletionBasedOnSize)
                    {
                        //Kilobytes
                        sizeThreshold = model.sizeThreshold * 1024f;
                        filesToDelete.AddRange(GetFilesToDeleteBasedOnSize(sizeThreshold));
                    }
                    if (model.isDeletionBasedOnTime)
                    {
                        dateThreshold = DateTime.Now.AddMonths(-model.month).AddDays(-model.day);
                        filesToDelete.AddRange(GetFilesToDeleteBasedOnTime(dateThreshold));
                    }
                    _logger.Info(String.Format("File deletion started. Date threshold: {0}, Size treshold: {1} Kilobytes", dateThreshold, sizeThreshold));
                    foreach (var file in filesToDelete)
                    {
                        if (DeleteFile(file.FullName))
                        {
                            numberOfFilesDeleted++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            _logger.Info(String.Format("File deletion was successful. Deleted: {0} files.", numberOfFilesDeleted));
        }

        private bool DeleteFile(string filepath)
        {
            try
            {
                string mimeType = MimeTypes.GetMimeType(filepath);
                if (mimeType.Split("/")[0] == "video" || mimeType.Split("/")[0] == "image")
                {
                    System.IO.File.Delete(filepath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
            return true;
        }

        private List<FileInfo> GetFilesToDeleteBasedOnTime(DateTime dateThreshold)
        {
            List<FileInfo> result = new List<FileInfo>();

            var directory = new DirectoryInfo(_targetDirectory);

            foreach (var file in directory.GetFiles())
            {
                if (file.LastWriteTime < dateThreshold)
                {
                    result.Add(file);
                }
            }
            return result;
        }

        private List<FileInfo> GetFilesToDeleteBasedOnSize(double sizeThreshold)
        {
            List<FileInfo> result = new List<FileInfo>();

            var directory = new DirectoryInfo(_targetDirectory);

            foreach (var file in directory.GetFiles())
            {
                if (file.Length > sizeThreshold)
                {
                    result.Add(file);
                }
            }
            return result;
        }
    }
}
