using RaspberryPISecurityCam.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Models.HomeViewModels
{
    public class AlarmType
    {
        public string AlarmCondition { get; set; }
        public AlarmEmailProvider AlarmEmailProvider { get; set; }
        public List<AlarmLogging> AlarmLogging { get; set; }
        public List<AlarmCloudStorage> AlarmCloudStorage { get; set; }
        public AlarmSaveToSd AlarmSaveToSd { get; set; }
    }
}
