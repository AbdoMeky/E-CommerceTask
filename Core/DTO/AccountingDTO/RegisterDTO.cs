using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Core.DTO.AccountingDTO
{
    public class RegisterDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        [Required]
        [RegularExpression(@"^[A-Za-z][A-Za-z0-9]*$", ErrorMessage = "it should be letters and numbers only,and first index must be character")]
        public string UserName { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "It should be numbers only.")]
        public string PhoneNumber { get; set; }
        [Required]
        [Compare("PhoneNumber")]
        public string PhoneNumberConfirmed { get; set; }
        public IFormFile? Image { get; set; }
    }
}