using System.ComponentModel.DataAnnotations;

namespace Namaya.Model
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string StudentCustomId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Contact { get; set; }
        public string Parent { get; set; }

        // Made nullable - this allows it to be null from the frontend
        public string? Arts { get; set; }

        public string Branch { get; set; }
        public string Batch { get; set; }
        public DateTime EnrollDate { get; set; }
        public string CurrentBelt { get; set; } = "White Belt";
        public int Progress { get; set; } = 0;
        public string BloodGroup { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }

        // Made public and nullable - this allows it to be null from the frontend
        public string? CourseType { get; set; }

        public bool IsSilambam { get; set; }
        public bool IsYoga { get; set; }
        public bool IsKarate { get; set; }
        public bool IsGymnastics { get; set; }
        public bool IsBoxing { get; set; }

        public string? FeeStatus { get; set; }


    }
}