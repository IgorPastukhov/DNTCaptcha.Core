using System.ComponentModel.DataAnnotations;
using DNTCaptcha.Core.Contracts;

namespace DNTCaptcha.TestWebApp.Models
{
    public class AccountViewModel : DNTCaptchaBase
    {
        [Display(Name = "User name")]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}