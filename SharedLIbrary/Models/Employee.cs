using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SharedLIbrary.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Required]
        public string EmployeeName { get; set; } = null!;

        public bool IsActive { get; set; }

        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }

        public string? ImageUrl { get; set; }

        public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
    }
    public class Experience
    {
        public int ExperienceId { get; set; }
        public string Title { get; set; } = null!;
        public int Duration { get; set; }
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;
    }
}
