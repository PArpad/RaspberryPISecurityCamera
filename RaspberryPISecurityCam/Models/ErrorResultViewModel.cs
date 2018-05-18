using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Models
{
    public class ErrorResultViewModel
    {
        public string Text { get; set; }
        public string ErrorMessage { get; set; }
        public string EndText { get; set; }
        public bool isError { get; set; }
    }
}
