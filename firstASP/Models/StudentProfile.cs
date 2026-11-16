using System.ComponentModel.DataAnnotations;

namespace firstASP.Models;

public class StudentProfile
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your student ID number.")]
    [StringLength(20)]
    public string StudentIdNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}