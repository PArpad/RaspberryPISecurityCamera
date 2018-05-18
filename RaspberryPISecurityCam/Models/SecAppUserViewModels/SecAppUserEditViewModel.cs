using RaspberryPISecurityCam.Models;
using System.ComponentModel.DataAnnotations;

namespace RaspberryPISecurityCam.Models
{
    public class SecAppUserEditViewModel
    {
        public int SecAppUserId { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public ErrorResultViewModel errorResultViewModel { get;set;}
    }
}
