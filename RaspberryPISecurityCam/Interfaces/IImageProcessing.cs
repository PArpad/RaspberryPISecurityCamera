using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Interfaces
{
    interface IImageProcessing
    {
        string isRunning();
        void startImageProcessing();
        void stopImageProcessing();
        string ImageSource { get; set; }
        List<string> Settings { get; set; }
    }
}
