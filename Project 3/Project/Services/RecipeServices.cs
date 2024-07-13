using System;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Project.Data;
using Project.Models;
using Project.Repository;

namespace Project.Services
{
    public class RecipeServices : RecipeRepository
    {
        private readonly AppDbContext _db;
        public RecipeServices(AppDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Recipe> GetRecipes(bool isAdmin)
        {
            if (isAdmin)
            {
                // Hiển thị tất cả các dữ liệu với UserId == null (cho AdminRecipe)
                return _db.Recipes.Where(p => p.UserId == null).ToList();
            }
            else
            {
                // Hiển thị tất cả các dữ liệu với UserId != null (cho AdminRecipeUser)
                return _db.Recipes.Where(p => p.UserId != null).ToList();
            }
        }

        public bool PostRecipe(Recipe newRecipe)
        {
            _db.Recipes.Add(newRecipe);
            _db.SaveChanges();
            return true;
        }

        public void UpdateRecipe(Recipe updatedRecipe)
        {
            var model = _db.Recipes.FirstOrDefault(u => u.RecipeId == updatedRecipe.RecipeId);
            if (model != null)
            {
                model.RecipeName = updatedRecipe.RecipeName;
                model.Images = updatedRecipe.Images;
                model.Ingredients = updatedRecipe.Ingredients;
                model.Procedure = updatedRecipe.Procedure;
                model.IsEnabled = updatedRecipe.IsEnabled;
                _db.Recipes.Update(model);
                _db.SaveChanges();
            }
        }

        public Recipe DetailRecipe(int RecipeID)
        {
            return _db.Recipes.FirstOrDefault(u => u.RecipeId == RecipeID)!;
        }
        public Recipe UpdateRecipe(int RecipeID)
        {
            return _db.Recipes.FirstOrDefault(u => u.RecipeId == RecipeID)!;
        }

        public bool DeleteRecipe(int RecipeID)
        {
            var model = _db.Recipes.FirstOrDefault(u => u.RecipeId == RecipeID);
            _db.Recipes.Remove(model);
            _db.SaveChanges();
            return true;
        }
    }
}

