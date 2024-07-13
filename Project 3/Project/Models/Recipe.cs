using Project.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project.Models;


public partial class Recipe
{
    public int RecipeId { get; set; }

    [Required]
    public string? RecipeName { get; set; }
    

    public string? Images { get; set; }
    [Required]
    public string? Ingredients { get; set; }
    [Required]
    public string? Procedure { get; set; }
    [Required]
    public string? IsEnabled { get; set; }
    [Required]
    public DateTime Date { get; set; }

    public string? UserId { get; set; }


    public virtual AppUser? User { get; set; }
}
