using RaspberryPISecurityCam.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Models.HomeViewModels
{
    public class AlarmCloudStorage
    {
        public bool IsSelected { get; set; }
        public CloudStorageProviderEnum cloudStorageProviderEnum { get; set; }
    }
}

