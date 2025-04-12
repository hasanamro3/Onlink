namespace Onlink.Models
{
    public class JobApplication
    {
        public int JobApplicationId { get; set; }
        public int JobId { get; set; }
        public Job? Job { get; set; }
        public IEnumerable<Employee>? EmpJob { get; set; }
    }
}
