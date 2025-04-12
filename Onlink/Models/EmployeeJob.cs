namespace Onlink.Models
{
    public class EmployeeJob
    {
        public int EmployeeJobId { get; set; }
        public int EmployeeId { get; set; }
        public int JobApplicationId { get; set; }
        public Employee? Employee { get; set; }
        public JobApplication? JobApplication { get; set; }

    }
}
