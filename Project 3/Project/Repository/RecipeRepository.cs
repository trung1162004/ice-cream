using System;
using Project.Models;

namespace Project.Repository
{
    public interface RecipeRepository
    {
        IEnumerable<Recipe> GetRecipes(bool isAdmin);
        bool PostRecipe(Recipe newRecipe);
        void UpdateRecipe(Recipe updatedRecipe);
        bool DeleteRecipe(int RecipeID);
        Recipe UpdateRecipe(int RecipeID);
        Recipe DetailRecipe(int RecipeID);
    }
}


