using Microsoft.Extensions.Logging;
using RaspberryPISecurityCam.Enums;
using RaspberryPISecurityCam.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Models.HomeViewModels
{
    public class AlarmSettings
    {
        public List<AlarmType> AlarmTypes { get; set; }
    }    
}
