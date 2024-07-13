using Project.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public string? UserId { get; set; }
    [Required(ErrorMessage = "Feedback text is required")]

    public string? FeedbackText { get; set; }

    public DateTime? FeedbackDate { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Name is required")]
    public string? Name { get; set; }
    public virtual AppUser? User { get; set; }
}
