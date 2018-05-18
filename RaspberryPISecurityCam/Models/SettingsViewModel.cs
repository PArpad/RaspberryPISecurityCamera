using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPiSecurityCam.Models
{
    public class SettingsViewModel
    {
        [RegularExpression(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d{1,5}")]
        [StringLength(60, MinimumLength = 10)]
        public string IPAddress { get; set; }

        [Range(0, 100)]
        public int FrameRate { get; set; }


        public string SelectedResolution { get; set; }

        [RegularExpression(@"(/[a-z A-Z]*(/[a-z A-Z]*)*)|([a-z A-Z]:\\([a-z A-Z]*\\)*)")]
        public string SaveDirectory { get; set; }

        public List<Resolution> ResolutionList { get; set; }
    }

    public class Resolution
    {
        [Key]
        public int ID { get; set; }
        public string ResolutionString { get; set; }
    }
}
