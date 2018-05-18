using Microsoft.AspNetCore.Hosting;
using RaspberryPISecurityCam.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Classes
{
    public class Motion : IImageProcessing
    {
        public Motion(IHostingEnvironment hostingEnvironment)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.configPath = hostingEnvironment.ContentRootPath + @"\motion.conf";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                this.configPath = $"/etc/motion/motion.conf";
            }
        }
        private string configPath;
        public string ConfigPath { get { return this.configPath; } set { this.configPath = value; } }
        private string imageSource;
        public string ImageSource { get { return this.imageSource; } set { this.imageSource = value; } }
        private List<string> settings;
        public List<string> Settings { get { return this.settings; } set { this.settings = value; } }

        public string isRunning()
        {
            bool MotionStatus = false;
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == "motion")
                { MotionStatus = true; }
            }
            if (MotionStatus == true)
            {
                return "Jelenleg fut a Motion";
            }
            else
            {
                return "Jelenleg nem fut a Motion";
            }
        }

        public void startImageProcessing()
        {
            Process.Start("motion","-c /etc/motion/motion.conf");
        }

        public void stopImageProcessing()
        {
            foreach (Process p in Process.GetProcesses())
            {         
                if (p.ProcessName == "motion")
                {
                    //Process.Start("service", "motion stop");
                    p.Kill();
                }
            }
        }
    }
}
