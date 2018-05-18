using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Models.HomeViewModels
{
    public class AdminSettings
    {
        public bool isDeletionBasedOnTime { get; set; }

        [Range(0, double.MaxValue)]
        public double sizeThreshold { get; set; }

        public bool isDeletionBasedOnSize { get; set; }

        [Range(0, 100)]
        public int month { get; set; }

        [Range(0, 1000)]
        public int day { get; set; }

        [EmailAddress]
        public List<string> emailAddresses { get; set; }

        public LogLevel logLevel { get; set; }
    }
}
