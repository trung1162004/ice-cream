using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public partial class ContactU
{
    public int ContactId { get; set; }

    [Required(ErrorMessage = "Content is required")]
    public string? Content { get; set; }
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]

    public string? Email { get; set; }
    [Required(ErrorMessage = "Phone is required")]


    public string? Phone { get; set; }
    [Required(ErrorMessage = "Name is required")]


    public string? Name { get; set; }


}
