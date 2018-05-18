using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Models.HomeViewModels
{
    public class AlarmEmailProvider
    {
        public bool IsEmailSelected { get; set; }
        public List<EmailSender> EmailSenders { get; set; }
        public string SelectedEmailSender { get; set; }
    }
    public class EmailSender
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
