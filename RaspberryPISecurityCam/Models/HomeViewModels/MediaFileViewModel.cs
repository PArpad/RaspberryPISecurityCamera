using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Models.HomeViewModels
{
    public class MediaFileViewModel
    {
        public List<FileInfo> MediaFiles { get; set; }
        public List<DateTime> FileDates { get; set; }
        public int MediaWidth { get; set; }
    }
}
