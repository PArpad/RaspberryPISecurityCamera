using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaspberryPISecurityCam.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using RaspberryPiSecurityCam.Services;
using RaspberryPISecurityCam.Classes;
using RaspberryPISecurityCam.Interfaces;
using Microsoft.Extensions.Primitives;
using RaspberryPiSecurityCam.Models;
using Microsoft.AspNetCore.Authorization;
using Google.Apis.Gmail.v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Google.Apis.Gmail.v1.Data;
using Org.BouncyCastle.Utilities.Encoders;
using System.Net.Mail;
using MimeKit;
using RaspberryPISecurityCam.Services;
using System.Web;
using RaspberryPISecurityCam.Models.HomeViewModels;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using log4net;
using RaspberryPISecurityCam.Enums;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Net;
using Hangfire;
using RaspberryPISecurityCam.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace RaspberryPISecurityCam.Controllers
{

    [Authorize(Roles = "SecCamUserAdministrators, ApprovedUser")]
    public class HomeController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private SignInManager<ApplicationUser> _signInManager;
        private UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        private ILog _logger;
        private IImageProcessing _ImageProcessing;
        private IEmailSender _emailSender;
        private GoogleDriveServices _googleDriveServices;
        private string[] _configFile;
        private string targetDirectory;
        public HomeController(IHostingEnvironment hostingEnvironment, 
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager,
            IEmailSenderFactory emailSenderFactory,
            IAuthorizationService authorizationService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _authorizationService = authorizationService;
            _emailSender = emailSenderFactory.GetEmailSender();
            _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _ImageProcessing = new Motion(_hostingEnvironment);
            _googleDriveServices = new GoogleDriveServices();

            _configFile = System.IO.File.ReadAllLines((_ImageProcessing as Motion).ConfigPath);
            foreach (string s in _configFile)
            {
                if (s.Contains("target_dir") && !s.Contains("#"))
                {
                    targetDirectory = s.Split(" ")[1];
                }
            }
            if (OperationMode.LastSentDate == DateTime.MinValue)
            {
                OperationMode.LastSentDate = DateTime.Now;
            }
        }

        public IActionResult Settings()
        {
            try
            {
                List<Resolution> resolutionList = new List<Resolution>
                {
                    new Resolution{ID=1, ResolutionString="640x480" },
                    new Resolution{ID=2, ResolutionString="1024x768" },
                    new Resolution{ID=3, ResolutionString="1366x768" },
                    new Resolution{ID=4, ResolutionString="1920x1080" },
                };

                SettingsViewModel model = new SettingsViewModel();
                model.ResolutionList = resolutionList;
                model.SelectedResolution = string.Empty;
                List<string> settings = new List<string>();

                foreach (string s in _configFile)
                {
                    if (s.Contains("stream_maxrate") && !s.Contains("#"))
                    {
                        settings.Add(s);
                    }
                    if ((s.Contains("height") || s.Contains("width")) && !s.Contains("#"))
                    {
                        settings.Add(s);
                    }
                    if (s.Contains("target_dir") && !s.Contains("#"))
                    {
                        settings.Add(s);
                    }
                    if (s.Contains("#StreamAddress"))
                    {
                        settings.Add(s);
                    }
                }
                ViewData["Options"] = settings;
                ViewData["MotionStatus"] = _ImageProcessing.isRunning();
                return View(model);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(SettingsViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string[] resolution = { "", "" };
                    string[] address = { "", "" };
                    if (model.SelectedResolution != null)
                    {
                        resolution = model.SelectedResolution.Split('x');
                    }
                    if (model.IPAddress != null)
                    {
                        address = model.IPAddress.Split(':');
                    }
                    bool isaddress = false;

                    for (int i = 0; i < _configFile.Count(); i++)
                    {
                        if (_configFile[i].Contains("stream_maxrate") && !_configFile[i].Contains("#") && model.FrameRate != 0)
                        {
                            _configFile[i] = "stream_maxrate " + model.FrameRate.ToString();
                        }
                        if (_configFile[i].Contains("framerate") && !_configFile[i].Contains("#") && model.FrameRate != 0)
                        {
                            _configFile[i] = "framerate " + model.FrameRate.ToString();
                        }
                        if (_configFile[i].Contains("height") && !_configFile[i].Contains("#") && resolution[1] != "")
                        {
                            _configFile[i] = "height " + resolution[1];
                        }
                        if (_configFile[i].Contains("width") && !_configFile[i].Contains("#") && resolution[0] != "")
                        {
                            _configFile[i] = "width " + resolution[0];
                        }
                        if (_configFile[i].Contains("target_dir") && !_configFile[i].Contains("#") && model.SaveDirectory != null)
                        {
                            _configFile[i] = "target_dir " + model.SaveDirectory;
                        }
                        if (_configFile[i].Contains("#StreamAddress") && model.IPAddress != null)
                        {
                            _configFile[i] = "#StreamAddress http://" + model.IPAddress;
                            isaddress = true;
                        }
                        if (_configFile[i].Contains("stream_port") && !_configFile[i].Contains("#") && address[1] != "")
                        {
                            _configFile[i] = "stream_port " + address[1];
                        }
                    }
                    if (isaddress == false && model.IPAddress != null)
                    {
                        System.IO.File.AppendAllText((_ImageProcessing as Motion).ConfigPath, "#StreamAddress http://" + model.IPAddress);
                    }
                    System.IO.File.WriteAllLines((_ImageProcessing as Motion).ConfigPath, _configFile);
                    System.Threading.Thread.Sleep(10);
                    return RedirectToAction("Settings");
                }
                return View();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }

        public IActionResult About()
        {
            try
            {
                ViewData["Message"] = "This is a university project featuring security camera implementation with Raspberry PI and it's official camera modules.";
                return View();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }


        public IActionResult Alarms()
        {
            try
            {
                AlarmSettings model = SettingsHandler.GetModelFromJSON<AlarmSettings>();
                if (!System.IO.File.Exists(SettingsHandler.GetFilePath<AlarmSettings>()) || model==null)
                {
                    var alarmTypeViewModelList = new List<AlarmType>();
                    var alarmCloudViewModelList = new List<AlarmCloudStorage>();
                    var loggingViewModelList = new List<AlarmLogging>();
                    var emailSenderList = new List<EmailSender>();
                    int numberOfEmailSender = 0;
                    foreach (CloudStorageProviderEnum cloudStorageProvider in Enum.GetValues(typeof(CloudStorageProviderEnum)))
                    {
                        alarmCloudViewModelList.Add(new AlarmCloudStorage() { cloudStorageProviderEnum = cloudStorageProvider, IsSelected = false });
                    }
                    foreach (LoggerProviderEnum loggerProviderEnum in Enum.GetValues(typeof(LoggerProviderEnum)))
                    {
                        loggingViewModelList.Add(new AlarmLogging() { loggerProviderEnum = loggerProviderEnum, IsSelected = false });
                    }
                    foreach (EmailSenderEnum emailSenderEnum in Enum.GetValues(typeof(EmailSenderEnum)))
                    {
                        emailSenderList.Add(new EmailSender { Name = EnumExtensions.GetDisplayName(emailSenderEnum), ID = numberOfEmailSender });
                        numberOfEmailSender++;
                    }
                    foreach (AlarmConditionEnum actionType in Enum.GetValues(typeof(AlarmConditionEnum)))
                    {
                        alarmTypeViewModelList.Add(new AlarmType
                        {
                            AlarmEmailProvider = new AlarmEmailProvider { EmailSenders= emailSenderList },
                            AlarmCloudStorage = alarmCloudViewModelList,
                            AlarmLogging = loggingViewModelList,
                            AlarmSaveToSd = new AlarmSaveToSd(),
                            AlarmCondition = EnumExtensions.GetDisplayName(actionType)
                        });
                    }

                    return View(new AlarmSettings
                    {
                        AlarmTypes = alarmTypeViewModelList
                    });
                }
                return View(model);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }

        [HttpPost]
        public IActionResult Alarms(AlarmSettings model)
        {
            try
            {
                SettingsHandler.WriteModelToJSON(model);
                return View(model);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }

        public IActionResult History()
        {
            try
            {
                return View();
            }
            catch(Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error",new ErrorViewModel {ErrorMessage=e.Message});
            }
        }

        [HttpPost]
        public PartialViewResult AlarmLoggingPartial()
        {
            return PartialView("AlarmLoggingPartial");
        }

        [HttpPost]
        public IActionResult MediaPartial(string[] dates, int width)
        {
            try
            {
                List<string> datesString = dates.ToList();
                var mediaFiles = GetMediaFiles();
                var mediaFilesToShow = mediaFiles.Where(m => datesString.Contains(m.LastWriteTime.ToShortDateString())).ToList();

                List<DateTime> fileDates = mediaFilesToShow
                                                  .GroupBy(g => g.LastWriteTime.DayOfYear)
                                                  .Select(s => s.First().LastWriteTime)
                                                  .OrderBy(o => o.Date)
                                                  .ToList();

                return PartialView("MediaPartial", new MediaFileViewModel { MediaFiles = mediaFilesToShow, FileDates = fileDates, MediaWidth = width });
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }

        public IActionResult Logs(string fileName)
        {
            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                string[] fileEntries = Directory.GetFiles(path);
                List<string> fileNames = new List<string>();
                foreach (string file in fileEntries)
                {
                    fileNames.Add(file.Replace(path, "").Replace("/", "").Replace(@"\", ""));
                }
                LogsViewModel logsViewModel = new LogsViewModel { FileNames = fileNames };

                return View(logsViewModel);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }

        [HttpPost]
        public IActionResult LogsPartial(string fileName, string logLevel)
        {
            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                List<string> fileText = new List<string>();
                if (fileName != null)
                {
                    using (var csv = new FileStream(Path.Combine(path, fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(csv))
                    {
                        while (!sr.EndOfStream)
                        {

                            fileText.Add(sr.ReadLine());
                        }
                    }
                }
                else
                {
                    fileText.Add("teszt");
                }
                LogLevel logLevelEnum = (LogLevel)System.Enum.Parse(typeof(LogLevel), logLevel);
                return PartialView("LogsPartial", new LogsViewModel { FileContent = fileText, LogLevel=logLevelEnum });
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }
        public IActionResult Live()
        {
            try
            {
                DriveInfo driveInfo = new DriveInfo(Directory.GetCurrentDirectory());
                ViewData["CurrentSpaceInGigabyte"] = driveInfo.AvailableFreeSpace / 1024f / 1024f / 1024f;
                ViewData["InternetConnection"] = OperationMode.CheckInternetConnection();
                return View();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }

        [Authorize(Roles = "SecCamUserAdministrators")]
        public IActionResult AdminSettings()
        {

            try
            {
                AdminSettings xmlModel = SettingsHandler.GetModelFromJSON<AdminSettings>();
                if (!System.IO.File.Exists("AdminSettings.xml") || xmlModel == null)
                {
                    var admin = _userManager.Users.FirstOrDefault(u => u.UserName == "admin");
                    var emailAddresses = new List<string>();
                    emailAddresses.Add(admin.Email);
                    return View(new AdminSettings { emailAddresses = emailAddresses });
                }
                return View(xmlModel);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }

        [Authorize(Roles = "SecCamUserAdministrators")]
        [HttpPost]
        public IActionResult AdminSettings(AdminSettings model)
        {
            try
            {
                if (!model.isDeletionBasedOnTime)
                {
                    model.month = 0;
                    model.day = 0;
                }
                if (!model.isDeletionBasedOnSize)
                {
                    model.sizeThreshold = 0;
                }
                var savedModel = SettingsHandler.GetModelFromJSON<AdminSettings>();
                SettingsHandler.WriteModelToJSON(model);
                if (savedModel != null)
                {
                    if (savedModel.logLevel != model.logLevel)
                    {
                        Log4NetLoglevelChanger.ChangeLogLevel();
                    }
                }
                return RedirectToAction("AdminSettings");
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }

        
       

        private List<FileInfo> GetMediaFiles()
        {
            List<FileInfo> result = new List<FileInfo>();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            var directory = new DirectoryInfo(path);

            foreach (var file in directory.GetFiles())
            {
                 result.Add(file);
            }
            return result;
        }
       

        public IActionResult StartImageProcessing()
        {
            try
            {
                _ImageProcessing.startImageProcessing();
                return RedirectToAction("Live");
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }
        public IActionResult StopImageProcessing()
        {
            try
            {
                _ImageProcessing.stopImageProcessing();
                return RedirectToAction("Live");
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }
        public IActionResult Contact()
        {
            try
            {
                ViewData["Message"] = "This is an open-source project, but if you have any questions, feel free to ask them on the contacts below.";
                return View();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }

        public IActionResult Error()
        {
            return View();
        }
        [HttpPost]
        public ActionResult PostFile(string files)
        {
            try
            {
                StringValues imageData;
                HttpContext.Request.ReadFormAsync().Result.TryGetValue("imageData", out imageData);
                string fileName = "Mask.pgm";
                string fileNameWithPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", fileName);

                using (FileStream fs = new FileStream(fileNameWithPath, FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        byte[] data = Convert.FromBase64String(imageData);
                        bw.Write(data);
                        bw.Close();
                    }
                    fs.Close();
                }
                for (int i = 0; i < _configFile.Count(); i++)
                {
                    if (_configFile[i].Contains("mask_file"))
                    {
                        _configFile[i] = "mask_file " + fileNameWithPath;
                    }
                }
                System.IO.File.WriteAllLines((_ImageProcessing as Motion).ConfigPath, _configFile);
                return RedirectToAction("Settings");
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                return View("Error", new ErrorViewModel { ErrorMessage = e.Message });
            }
        }
    }
}
