using System.ComponentModel.DataAnnotations;

namespace Onlink.Models
{
    public class Employer
    {
        public int EmployerId { get; set; }

        [Required(ErrorMessage = "First Name Must Be Added")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Name At least 2 Chars")]
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email Is Required")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password Not Valid!")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Password Min 8 Chars")]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Password Not Valid!")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Password Min 8 Chars")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password Not Valid!")]
        public string PasswordConfirmation { get; set; }

        [StringLength(15, MinimumLength = 10, ErrorMessage = "Phone Not Valid")]
        [Phone]
        public string? PhoneNumber { get; set; }

        public IEnumerable<Job>? Jobs { get; set; }
        public IEnumerable<Resume>? Resume { get; set; }

    }
}
