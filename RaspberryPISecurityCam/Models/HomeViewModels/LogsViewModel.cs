using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Models.HomeViewModels
{
    public class LogsViewModel
    {
        public List<string> FileNames { get; set; }
        public List<string> FileContent { get; set; }
        public LogLevel LogLevel { get; set; }
    }
}
