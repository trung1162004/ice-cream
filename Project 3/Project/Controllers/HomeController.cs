using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using PayPal.Api;
using Project.Data;
using Project.Models;
using Project.Repository;
using Project.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Project.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RecipeRepository _recipeRepository;
        private readonly ContactRepository _contactRepository;
        private readonly FeedbackRepository _feedbackRepository;
        private readonly AppDbContext _db;

        public HomeController(ILogger<HomeController> logger, RecipeRepository recipeRepository, ContactRepository contactRepository, FeedbackRepository feedbackRepository, AppDbContext db)
        {
            _logger = logger;
            _recipeRepository = recipeRepository;
            _contactRepository = contactRepository;
            _feedbackRepository = feedbackRepository;
            _db = db;

        }


        public IActionResult Index()
        {
            var model = _recipeRepository.GetRecipes(true).OrderByDescending(r => r.Date);
            return View(model);
        }

        [HttpGet]
        public IActionResult ContactUs()
        {

            return View();
        }
        [HttpPost]
        public IActionResult ContactUs(ContactU contact)
        {
            // Array of bad words
            string[] badWords = new string[] { "stupid", "bad", "ugly", /* ... other bad words ... */ };

            try
            {
                // Check if contact content contains any bad words
                if (badWords.Any(word => contact.Content.Contains(word, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError(string.Empty, "Your message contains inappropriate language.");
                    return View(contact);
                }

                if (ModelState.IsValid)
                {
                    _contactRepository.PostContact(contact);
                    TempData["SuccessMessage"] = "Contact us form submitted successfully!";
                    return RedirectToAction("ContactUs", "Home");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
                return View(contact);
            }

            // If ModelState is not valid, return to the view with the existing model
            return View(contact);
        }
        public IActionResult OrderTracking() {
            var uid = HttpContext.Session.GetString("uid");
           
              

                var model = from order in _db.Orders
                            join orderItem in _db.OrderItems on order.OrderId equals orderItem.OrderId into orderItems
                            from orderItem in orderItems.DefaultIfEmpty()
                            join book in _db.Books on orderItem.BookId equals book.BookId into orderItemBooks
                            from book in orderItemBooks.DefaultIfEmpty()
                            where order.UserId == uid // Lọc theo UserID
                            select new BookCombined
                            {
                                Order = order,
                                OrderItem = orderItem,
                                Book = book
                            };

                return View(model.ToList());
           
        
        }
        [Authorize(Roles ="Users")]
        [Authorize(Policy = "CheckPaymentExpiry")]
        public IActionResult PremiumRecipe(string sortOption = "None", string searchName = "")
        {
            var recipes = _recipeRepository.GetRecipes(true);

            // Filter by search query
            if (!string.IsNullOrEmpty(searchName))
            {

                recipes = recipes.Where(item => item.RecipeName!.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToList();

            }

            // Sort based on user's selection
            switch (sortOption)
            {
                case "Name":
                    recipes = recipes.OrderBy(r => r.RecipeName);
                    break;
                case "Signature":
                    recipes = recipes.OrderByDescending(r => r.IsEnabled!.Contains("Signature"));
                    break;
                // Add more cases if needed for additional sorting options
                case "Latest":
                    recipes = recipes.OrderByDescending(r => r.Date).ToList();
                    break;
                case "Oldest":
                    recipes = recipes.OrderBy(r => r.Date).ToList();
                    break;
                default:
                    // Default sorting option (fallback to sorting by Name)
                    recipes = recipes.OrderBy(r => r.RecipeName);
                    break;
            }

            return View(recipes.ToList());
        }
        public IActionResult FreeRecipe(string sortOption = "None", string searchName = "")
        {
            var recipes = _recipeRepository.GetRecipes(true);

            // Filter by search query
            if (!string.IsNullOrEmpty(searchName))
            {

                recipes = recipes.Where(item => item.RecipeName!.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToList();

            }

            // Sort based on user's selection
            switch (sortOption)
            {
                case "Name":
                    recipes = recipes.OrderBy(r => r.RecipeName);
                    break;
                case "Signature":
                    recipes = recipes.OrderByDescending(r => r.IsEnabled!.Contains("Signature"));
                    break;
                // Add more cases if needed for additional sorting options
                case "Latest":
                    recipes = recipes.OrderByDescending(r => r.Date).ToList();
                    break;
                case "Oldest":
                    recipes = recipes.OrderBy(r => r.Date).ToList();
                    break;
                default:
                    // Default sorting option (fallback to sorting by Name)
                    recipes = recipes.OrderBy(r => r.RecipeName);
                    break;
            }

            return View(recipes.ToList());
        }
        public IActionResult AboutUs()
        {
            return View();
        }
        [Authorize(Roles = "Users")]
        public IActionResult AddRecipe()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddRecipe(Recipe recipe, IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    ViewBag.msg = "file not selected...";
                }
                else
                {
                    var path = Path.Combine("wwwroot/images", file.FileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        file.CopyToAsync(stream);
                    }

                    recipe.Images = "images/" + file.FileName;
                    _recipeRepository.PostRecipe(recipe);
                    TempData["SuccessMessage"] = "Your recipe submitted successfully! *We will send an email if your recipe approve and post on our website";
                    return RedirectToAction("AddRecipe", "Home");
                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }
            return View();
        }
        public IActionResult DisplayFeedback()
        { 

            var model = _feedbackRepository.findAll().OrderByDescending(r => r.FeedbackDate);

            return View(model);
        }
        [HttpGet]
        public IActionResult Feedback()
        {


            return View();
        }
        [HttpPost]
        public IActionResult Feedback(Feedback feedback)
        {
            // Array of bad words
            string[] badWords = new string[] { "stupid", "bad", "ugly", /* ... other bad words ... */ };

            try
            {
                // Check if feedback is null or whitespace
                if (string.IsNullOrWhiteSpace(feedback.FeedbackText))
                {
                    ModelState.AddModelError(string.Empty, "Feedback cannot be empty.");
                    return View(feedback);
                }

                // Check if feedback content contains any bad words
                if (badWords.Any(word => feedback.FeedbackText.Contains(word, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError(string.Empty, "Your feedback contains inappropriate language.");
                    return View(feedback);
                }

                if (ModelState.IsValid)
                {
                    _feedbackRepository.create(feedback);
                    TempData["SuccessMessage"] = "Feedback submitted successfully!";
                    HttpContext.Session.SetString("FeedbackData", JsonConvert.SerializeObject(feedback));
                    return RedirectToAction("Feedback", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Fail!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                throw;
            }

            return View(feedback);
        }

        public IActionResult ExportToExcel(int recipeId)
        {
            // Replace this with the actual data retrieval logic based on the recipeId
            var data = _recipeRepository.DetailRecipe(recipeId);

            if (data == null)
            {
                // Handle the case where the recipe with the given ID is not found
                return NotFound();
            }

            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("ExportedData");

                // Add header row
                worksheet.Cells[1, 1].Value = "RecipeName";
                worksheet.Cells[1, 2].Value = "Ingredients";
                worksheet.Cells[1, 3].Value = "Process";

                // Add data row for the specific recipe
                worksheet.Cells[2, 1].Value = data.RecipeName;
                worksheet.Cells[2, 2].Value = data.Ingredients;
                worksheet.Cells[2, 3].Value = data.Procedure;

                // AutoFit columns for better appearance
                worksheet.Cells.AutoFitColumns();

                // Style the header row
                using (var range = worksheet.Cells[1, 1, 1, 3])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Style data row
                using (var range = worksheet.Cells[2, 1, 2, 3])
                {
                    range.Style.Font.Bold = false;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                package.Save();
            }

            stream.Position = 0;

            var randomFileName = $"ExportedData_{data.RecipeName}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", randomFileName);
        }

        public IActionResult FAQ()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }

}
