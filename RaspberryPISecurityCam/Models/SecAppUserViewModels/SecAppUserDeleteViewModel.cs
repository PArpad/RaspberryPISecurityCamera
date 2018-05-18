using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Models.SecAppUserViewModels
{
    public class SecAppUserDeleteViewModel
    {
        public int SecAppUserId { get; set; }

        // user ID from AspNetUser table
        public string OwnerID { get; set; }

        public string Name { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public bool isFirstLogin { get; set; }

        public UserStatus Status { get; set; }

        public ErrorResultViewModel errorResultViewModel { get; set; }
    }
}
