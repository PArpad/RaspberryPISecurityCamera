using RaspberryPISecurityCam.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Classes
{
    public static class OperationMode
    {
        public static DateTime LastSentDate { get; set; }

        private static AlarmConditionEnum currentMode { get; set; }

        public static AlarmConditionEnum getCurrentMode()
        { return currentMode; }

        public static void DetermineCurrentMode()
        {
            if (CheckUPSStatus())
            { currentMode = AlarmConditionEnum.OnUPS; }
            else if (CheckSDStatus())
            { currentMode = AlarmConditionEnum.SdLowOnSpace; }
            else if (CheckInternetConnection())
            { currentMode = AlarmConditionEnum.NoInternetConnection; }
            else
            { currentMode = AlarmConditionEnum.Normal; }
        }

        public static bool CheckInternetConnection()
        {
            try
            {
                Ping myPing = new Ping();
                String host = "8.8.8.8";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {
                    return false;
                }
                else return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return true;
            }
        }

        private static bool CheckUPSStatus()
        {
            return false;
        }

        private static bool CheckSDStatus()
        {
            DriveInfo driveInfo = new DriveInfo(Directory.GetCurrentDirectory());
            var space = driveInfo.AvailableFreeSpace / 1024f / 1024f / 1024f;
            if (space<1.0)
            { return true; }
            else { return false; }
        }
    }
}
