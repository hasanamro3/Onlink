namespace Onlink.Models
{
    public class JobApplication
    {
        public int JobApplicationId { get; set; }
        public int JobId { get; set; }

        // Status properties
        public string Status { get; set; } = "Pending";
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Job? Job { get; set; }
        public ICollection<EmployeeJob> EmployeeJobs { get; set; } = new List<EmployeeJob>();
    }
}
