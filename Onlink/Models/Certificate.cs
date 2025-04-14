using System.ComponentModel.DataAnnotations;

namespace Onlink.Models
{
    public class Certificate
    {
        public int CertificateId { get; set; }

        [Required(ErrorMessage = "Certificate Name is required")]
        public string Name { get; set; }

        public string? Issuer { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateEarned { get; set; }

        [DataType(DataType.Url)]
        public string? PathLink { get; set; } 

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

    }
}
