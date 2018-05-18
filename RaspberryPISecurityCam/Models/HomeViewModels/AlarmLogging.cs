using RaspberryPISecurityCam.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Models.HomeViewModels
{
    public class AlarmLogging
    {
        public bool IsSelected { get; set; }
        public LoggerProviderEnum loggerProviderEnum { get; set; }
    }
}
