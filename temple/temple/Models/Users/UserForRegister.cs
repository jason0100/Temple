using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace temple.Models.Users
{
    public class UserForRegister : IValidatableObject
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "請輸入6~50位字元當作密碼")]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult("兩次密碼輸入不相符", new string[] { "ConfirmPassword" });
            }
        }
    }


}
