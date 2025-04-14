using System.ComponentModel.DataAnnotations;

namespace Onlink.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "First Name Must Be Added")]
        [StringLength(24, MinimumLength = 3, ErrorMessage = "Name At least 3 Chars")]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "Last Name Must Be Added")]
        [StringLength(24, MinimumLength = 3, ErrorMessage = "Name At least 3 Chars")]
        public string LastName { get; set; }

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

        public IEnumerable<Resume>? Resume { get; set; }

        //public string? Certificates { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Bio { get; set; }
        public CheckInfo? CheckInfo { get; set; }
        public IEnumerable<JobApplication>? EmpJob { get; set; }

        public IEnumerable<Certificate>? Certificates { get; set; }

    }
}
