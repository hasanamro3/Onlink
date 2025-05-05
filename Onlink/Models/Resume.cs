namespace Onlink.Models
{
    public class Resume
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Summary { get; set; }

        public string Education { get; set; }

        public string Experience { get; set; }

        public string Skills { get; set; }
        public string LinkPath { get; set; }

        public int? EmployeeId { get; set; }
        public int? EmployerId { get; set; }

        public Employee? Employee { get; set; }
        public Employer? Employer { get; set; }
    }
}
