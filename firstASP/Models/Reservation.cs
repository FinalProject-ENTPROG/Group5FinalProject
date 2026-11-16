
using System.ComponentModel.DataAnnotations;

namespace firstASP.Models;

public class Reservation
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter your student ID number.")]
    [StringLength(20)]
    public string StudentIdNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your Benilde email.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [RegularExpression(
        @"^[a-zA-Z0-9._%+-]+@([a-zA-Z0-9-]+\.)*benilde\.edu\.ph$",
        ErrorMessage = "Please use your official Benilde email address."
    )]
    public string BenildeEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100)]
    public string StudentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a starting date and time.")]
    public DateTime StartDateTime { get; set; }

    [Required(ErrorMessage = "Please select an ending date and time.")]
    public DateTime EndDateTime { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Notification information
    public bool IsNotificationRead { get; set; } = true;

    public DateTime? StatusUpdatedAt { get; set; }
}
