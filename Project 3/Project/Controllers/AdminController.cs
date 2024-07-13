using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.Models.Identity;
using Project.Repository;
using OfficeOpenXml;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Project.Data;
using X.PagedList;
using static System.Reflection.Metadata.BlobBuilder;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Project.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {

        private readonly RecipeRepository _recipeRepository;
        private readonly ContactRepository _contactRepository;
        private readonly FeedbackRepository _feedbackRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserDetailRepo _userRepository;
        private readonly OrderDashboardRepository _orderRepository;
        private readonly OrderRepository _orderuRepository;
        private readonly IBookRepository _bookRepository;

        private readonly AppDbContext _db;




    
        public AdminController(RecipeRepository recipeRepository, ContactRepository contactRepository, FeedbackRepository feedbackRepository, UserManager<AppUser> userManager, UserDetailRepo userRepository, IBookRepository bookRepository,
       OrderDashboardRepository orderRepository,
         OrderRepository orderuRepository,
        AppDbContext db)
        {
            _recipeRepository = recipeRepository;
            _contactRepository = contactRepository;
            _feedbackRepository = feedbackRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _orderuRepository = orderuRepository;
            _bookRepository = bookRepository;


            _db = db;

        }
        public IActionResult DisplayRecipe(string recipeName, string sortOrder, int? page)
        {
            var model = _recipeRepository.GetRecipes(true);

            if (!string.IsNullOrEmpty(recipeName))
            {
                model = model.Where(r => r.RecipeName!.Contains(recipeName));
            }

            List<Recipe> sortedModel;
            switch (sortOrder)
            {
                case "Free":
                    sortedModel = model.Where(r => r.IsEnabled == "Free").ToList();
                    break;
                case "Free Signature":
                    sortedModel = model.Where(r => r.IsEnabled == "Free Signature").ToList();
                    break;
                case "Signature":
                    sortedModel = model.Where(r => r.IsEnabled == "Signature").ToList();
                    break;
                default:
                    // Mặc định hiển thị tất cả
                    sortedModel = model.ToList();
                    break;
            }

            int pageNumber = page ?? 1;
            int pageSize = 10;

            IPagedList<Recipe> pagedRecipes = sortedModel.ToPagedList(pageNumber, pageSize);

            ViewBag.RecipeName = recipeName;

            return View(pagedRecipes);
        }


        public IActionResult Dashboard()
        {
            // Retrieve data from your data source (e.g., database)
            var users = _userRepository.findAll();
            var orderitems = _orderRepository.findAll();
            var feedbacks = _feedbackRepository.findAll();

            // Create the DashboardViewModel and populate its properties
            var dashboardViewModel = new DashboardViewModel
            {
                Users = users,
                OrderItems = orderitems,
                Feedbacks = feedbacks
            };

            return View(dashboardViewModel);
        }

        public IActionResult CreateRecipe()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateRecipe(Recipe recipe, IFormFile file)
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
                    return RedirectToAction("DisplayRecipe", "Admin");
                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }
            return View();
        }

        [HttpGet]
        public IActionResult UpdateRecipe(int RecipeID)
        {
            var recipe = _recipeRepository.DetailRecipe(RecipeID);
            return View(recipe);
        }

        [HttpPost]
        public IActionResult UpdateRecipe(Recipe recipe, IFormFile file)
        {
            try
            {
                if (file != null)
                {
                    var path = Path.Combine("wwwroot/Images", file.FileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        file.CopyToAsync(stream);
                    }

                    recipe.Images = "Images/" + file.FileName;
                }
                else
                {
                    // Nếu không có tệp tin mới được tải lên, sử dụng hình ảnh cũ
                    var existingRecipe = _recipeRepository.UpdateRecipe(recipe.RecipeId);
                    recipe.Images = existingRecipe.Images; // Gán lại đường dẫn của hình ảnh cũ
                }

                _recipeRepository.UpdateRecipe(recipe);

                return RedirectToAction("DisplayRecipe", "Admin");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }

            return View();
        }


        public IActionResult DeleteRecipe(int RecipeID)
        {
            try
            {
                _recipeRepository.DeleteRecipe(RecipeID);
                return RedirectToAction("DisplayRecipe", "Admin");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }

            return View();
        }
        public IActionResult DetailRecipe(int RecipeID)
        {
            var recipe = _recipeRepository.DetailRecipe(RecipeID);
            return View(recipe);
        }
        //+++++++++end recipe area

        public IActionResult DisplayURecipe(string recipeUserName, int? page)
        {
            var model = _recipeRepository.GetRecipes(false);
            if (!string.IsNullOrEmpty(recipeUserName))
            {
                // Nếu có tên công thức được nhập, lọc dữ liệu theo tên
                model = model.Where(r => r.RecipeName!.Contains(recipeUserName));
            }
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1
            int pageSize = 6; // Số lượng sách trên mỗi trang

            IPagedList<Recipe> model1 = model.ToPagedList(pageNumber, pageSize);
            return View(model1);
        }
        public IActionResult DetailURecipe(int RecipeID)
        {
            var recipe = _recipeRepository.DetailRecipe(RecipeID);
            return View(recipe);
        }
        public IActionResult DeleteURecipe(int RecipeID)
        {
            try
            {
                _recipeRepository.DeleteRecipe(RecipeID);
                return RedirectToAction("DisplayURecipe", "Admin");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }

            return View();
        }
        //ContactUs
        public IActionResult DisplayContact(string sortOrder, string contactName, int? page)
        {
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            var model = _contactRepository.GetContact(); // Thay đổi phần này bằng logic lấy dữ liệu thực tế của bạn

            // Áp dụng bộ lọc tìm kiếm theo tên
            if (!string.IsNullOrEmpty(contactName))
            {
                model = model.Where(item => item.Name!.Contains(contactName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Áp dụng sắp xếp
            switch (sortOrder)
            {
                case "name_desc":
                    model = model.OrderByDescending(item => item.Name).ToList();
                    break;
                default:
                    model = model.OrderBy(item => item.Name).ToList();
                    break;
            }

            int pageNumber = page ?? 1;
            int pageSize = 10;

            IPagedList<ContactU> pagedContacts = model.ToPagedList(pageNumber, pageSize); // Chuyển đổi danh sách sang trang dữ liệu

            ViewBag.SortOrder = sortOrder;

            return View(pagedContacts); // Trả về dữ liệu đã được phân trang
        }

        [HttpGet]
        public IActionResult UpdateContact(int Conid)
        {
            var contact = _contactRepository.UpdateContact(Conid);
            return View(contact);
        }

        [HttpPost]
        public IActionResult UpdateContact(ContactU contact)
        {
            try
            {
                _contactRepository.UpdateContact(contact);

                return RedirectToAction("DisplayContact", "Admin");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }

            return View();
        }
        public IActionResult DetailContact(int ContactID)
        {
            var contact = _contactRepository.DetailContact(ContactID);
            return View(contact);
        }
        public IActionResult DeleteContact(int id)
        {
            _contactRepository.DeleteContact(id);
            TempData["SuccessMessage"] = "Delete successfully!";
            return RedirectToAction("DisplayContact", "Admin");
        }
        public IActionResult DisplayFeedback(string sortOrder, string searchName, int? page)
        {
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date";

            var model = _feedbackRepository.findAll();
            // Áp dụng bộ lọc tìm kiếm theo tên
            if (!string.IsNullOrEmpty(searchName))
            {
                model = model.Where(item => item.Name!.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Áp dụng sắp xếp
            switch (sortOrder)
            {
                case "name_desc":
                    model = model.OrderByDescending(item => item.Name).ToList();
                    break;
                case "Date":
                    model = model.OrderBy(item => item.FeedbackDate).ToList();
                    break;
                case "date_desc":
                    model = model.OrderByDescending(item => item.FeedbackDate).ToList();
                    break;
                default:
                    model = model.OrderBy(item => item.Name).ToList();
                    break;
            }

            int pageNumber = page ?? 1;
            int pageSize = 10;

            IPagedList<Feedback> pagedFeedbacks = model.ToPagedList(pageNumber, pageSize);

            return View(pagedFeedbacks);
        }
        [HttpGet]
        public IActionResult UpdateFeedback(int Fbid)
        {
            var feedback = _feedbackRepository.update(Fbid);
            return View(feedback);
        }
        public async Task<IActionResult> Users(string name, int? page)
        {
            IQueryable<AppUser> usersQuery = _userManager.Users;

            if (!string.IsNullOrWhiteSpace(name))
            {
                usersQuery = usersQuery.Where(u => EF.Functions.Like(u.FullName, $"%{name}%"));
            }

            int pageSize = 10; // Số lượng người dùng trên mỗi trang
            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1

            // Sử dụng ToListAsync() để chuyển dữ liệu về client trước khi phân trang
            List<AppUser> filteredUsers = await usersQuery.ToListAsync();

            IPagedList<AppUser> pagedUsers = filteredUsers.ToPagedList(pageNumber, pageSize);

            return View(pagedUsers);
        }
        public IActionResult DisplayPayment(string searchString, int? page)
        {
            var payments = _db.PaymentInfos.Include(p => p.User).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                payments = payments.Where(p =>
                    p.PaymentId.Contains(searchString) ||
                    p.User.FullName.Contains(searchString)
                );
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var pagedPayments = payments.ToPagedList(pageNumber, pageSize);

            return View(pagedPayments);
        }
        public async Task<IActionResult> ExportToExcel1122()
        {
            var payments = _db.PaymentInfos.Include(p => p.User).ToList();


            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("ExportedData");

                // Add header row
                worksheet.Cells[1, 1].Value = "Paymentid";
                worksheet.Cells[1, 2].Value = "Full name";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Package type";
                worksheet.Cells[1, 5].Value = "Created date";
                worksheet.Cells[1, 6].Value = "Exp date";


                // Add data rows
                var row = 2;
                foreach (var item in payments)
                {
                    worksheet.Cells[row, 1].Value = item.PaymentId;
                    worksheet.Cells[row, 2].Value = item.User.FullName;
                    worksheet.Cells[row, 3].Value = item.User.Email;
                    worksheet.Cells[row, 4].Value = item.PackageType;
                    worksheet.Cells[row, 5].Value = item.CreatedDate;
                    worksheet.Cells[row, 6].Value = item.ExpiryDate;
                    row++;
                }

                // AutoFit columns for better appearance
                worksheet.Cells.AutoFitColumns();

                // Style the header row
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Style data rows
                using (var range = worksheet.Cells[2, 1, row - 1, 5])
                {
                    range.Style.Font.Bold = false;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                package.Save();
            }

            stream.Position = 0;

            var randomFileName = $"ExportedData_{Guid.NewGuid()}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", randomFileName);


        }
        public async Task<IActionResult> ExportToExcel11()
        {
            IQueryable<AppUser> usersQuery = _userManager.Users;

            var data = await usersQuery.ToListAsync();


            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("ExportedData");

                // Add header row
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Username";
                worksheet.Cells[1, 3].Value = "Full name";
                worksheet.Cells[1, 4].Value = "Phone Number";
                worksheet.Cells[1, 5].Value = "Email";

                // Add data rows
                var row = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.Id;
                    worksheet.Cells[row, 2].Value = item.UserName;
                    worksheet.Cells[row, 3].Value = item.FullName;
                    worksheet.Cells[row, 4].Value = item.PhoneNumber;
                    worksheet.Cells[row, 5].Value = item.Email;
                    row++;
                }

                // AutoFit columns for better appearance
                worksheet.Cells.AutoFitColumns();

                // Style the header row
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Style data rows
                using (var range = worksheet.Cells[2, 1, row - 1, 5])
                {
                    range.Style.Font.Bold = false;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                package.Save();
            }

            stream.Position = 0;

            var randomFileName = $"ExportedData_{Guid.NewGuid()}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", randomFileName);


        }
        public async Task<IActionResult> ExportToExcelContact()
        {
            var data = _contactRepository.GetContact(); // Replace this with the actual data retrieval logic

            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("ExportedData");

                // Add header row
                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Phone";
                worksheet.Cells[1, 4].Value = "Content";
             

                // Add data rows
                var row = 2;
                foreach (var item in data)
                {
                   
                    worksheet.Cells[row, 1].Value = item.Name;
                    worksheet.Cells[row, 2].Value = item.Email;
                    worksheet.Cells[row, 3].Value = item.Phone;
                    worksheet.Cells[row, 4].Value = item.Content;
                    row++;
                }

                // AutoFit columns for better appearance
                worksheet.Cells.AutoFitColumns();

                // Style the header row
                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Style data rows
                using (var range = worksheet.Cells[2, 1, row - 1, 4])
                {
                    range.Style.Font.Bold = false;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                package.Save();
            }

            stream.Position = 0;

            var randomFileName = $"ExportedData_{Guid.NewGuid()}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", randomFileName);

        }
        [HttpPost]
        public IActionResult UpdateFeedback(Feedback feedback)
        {
            try
            {
                _feedbackRepository.update(feedback);

                return RedirectToAction("DisplayFeedback", "Admin");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }

            return View();
        }
        public IActionResult DetailFeedback(int FeedbackID)
        {
            var feedback = _feedbackRepository.DetailFeedback(FeedbackID);
            return View(feedback);
        }
        public IActionResult ExportToExcel()
        {
            var data = _feedbackRepository.findAll(); // Replace this with the actual data retrieval logic

            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("ExportedData");

                // Add header row
                worksheet.Cells[1, 1].Value = "FeedbackId";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "FeedbackText";
                worksheet.Cells[1, 5].Value = "FeedbackDate";

                // Add data rows
                var row = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.FeedbackId;
                    worksheet.Cells[row, 2].Value = item.Name;
                    worksheet.Cells[row, 3].Value = item.Email;
                    worksheet.Cells[row, 4].Value = item.FeedbackText;
                    worksheet.Cells[row, 5].Value = item.FeedbackDate;
                    row++;
                }

                // AutoFit columns for better appearance
                worksheet.Cells.AutoFitColumns();

                // Style the header row
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Style data rows
                using (var range = worksheet.Cells[2, 1, row - 1, 5])
                {
                    range.Style.Font.Bold = false;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                package.Save();
            }

            stream.Position = 0;

            var randomFileName = $"ExportedData_{Guid.NewGuid()}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", randomFileName);
        }

        public IActionResult DeleteFeedback(int id)
        {
            _feedbackRepository.delete(id);
            TempData["SuccessMessage"] = "Delete successfully!";
            return RedirectToAction("DisplayFeedback", "Admin");

        }
        [HttpGet]
        public IActionResult OrderDetail(int orderId)
        {
            var model = (from order in _db.Orders
                         where order.OrderId == orderId
                         join orderItem in _db.OrderItems on order.OrderId equals orderItem.OrderId into orderItems
                         from orderItem in orderItems.DefaultIfEmpty()
                         join book in _db.Books on orderItem.BookId equals book.BookId into orderItemBooks
                         from book in orderItemBooks.DefaultIfEmpty()
                         select new BookCombined
                         {
                             Order = order,
                             OrderItem = orderItem,
                             Book = book
                         }).FirstOrDefault();

            if (model == null)
            {
                // Handle the case where the order doesn't exist
                return NotFound();
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult BookTracking(string sortOrder, string searchName)
        {
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date";

            var model = from order in _db.Orders
                        join orderItem in _db.OrderItems on order.OrderId equals orderItem.OrderId into orderItems
                        from orderItem in orderItems.DefaultIfEmpty()
                        join book in _db.Books on orderItem.BookId equals book.BookId into orderItemBooks
                        from book in orderItemBooks.DefaultIfEmpty()
                        select new BookCombined
                        {
                            Order = order,
                            OrderItem = orderItem,
                            Book = book
                        };

            if (!string.IsNullOrEmpty(searchName))
            {
                model = model.Where(item => EF.Functions.Like(item.Order.Name, $"%{searchName}%"));
            }


            // Apply sorting

            switch (sortOrder)
            {
                case "name_desc":
                    // Fix: Change from item.Name to item.Order.Name for sorting by order name
                    model = model.OrderByDescending(item => item.Order!.Name);
                    break;
                case "Date":

                    model = model.OrderBy(item => item.Order.OrderDate);
                    break;
                case "date_desc":
                    // Fix: Change from item.FeedbackDate to item.Order.FeedbackDate for sorting by order date
                    model = model.OrderByDescending(item => item.Order.OrderDate);
                    break;
                default:
                    // Fix: Change from item.Name to item.Order.Name for default sorting by order name
                    model = model.OrderBy(item => item.Order.Name);
                    break;
            }
            var orderStatuses = new List<string> { "Pending", "Delivered", "Delivering", "Reject" };
            ViewBag.OrderStatuses = orderStatuses;
            // Fix: Remove unnecessary ToList() calls
            return View(model.ToList());
        }
        [HttpPost]
        public IActionResult BookTracking(int orderId, string status)
        {
            try
            {
                var success = _orderuRepository.UpdateOrderStatus(orderId, status);


                if (success)
                {
                    return Json(new { success = true, message = "Status updated successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update status" });
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                return Json(new { success = false, message = "An error occurred while updating status" });
            }
        }

        public IActionResult ExportToExcelBook()
        {
            var data = from order in _db.Orders
                       join orderItem in _db.OrderItems on order.OrderId equals orderItem.OrderId into orderItems
                       from orderItem in orderItems.DefaultIfEmpty()
                       join book in _db.Books on orderItem.BookId equals book.BookId into orderItemBooks
                       from book in orderItemBooks.DefaultIfEmpty()
                       select new BookCombined
                       {
                           Order = order,
                           OrderItem = orderItem,
                           Book = book
                       };// Replace this with your actual data retrieval logic

            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("ExportedData");

                // Add header row
                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Book title";
                worksheet.Cells[1, 4].Value = "Order date";
                worksheet.Cells[1, 5].Value = "Price";

                // Add data rows
                var row = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.Order.Name;
                    worksheet.Cells[row, 2].Value = item.Order.Email;
                    worksheet.Cells[row, 3].Value = item.OrderItem.Book.Name;
                    worksheet.Cells[row, 4].Value = item.Order.OrderDate;
                    worksheet.Cells[row, 5].Value = item.OrderItem.Total;
                    row++;
                }

                // AutoFit columns for better appearance
                worksheet.Cells.AutoFitColumns();

                // Style the header row
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Style data rows
                using (var range = worksheet.Cells[2, 1, row - 1, 5])
                {
                    range.Style.Font.Bold = false;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                package.Save();
            }

            stream.Position = 0;

            var randomFileName = $"ExportedData_{"Order Detail"}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", randomFileName);


        }
        [HttpGet]
        [HttpGet]
        public IActionResult DisplayBook(string bookName, int? page)
        {
            var books = _bookRepository.GetBooks();

            if (!string.IsNullOrEmpty(bookName))
            {
                books = books.Where(b => b.Name!.Contains(bookName)).ToList();
            }

            int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1
            int pageSize = 6; // Số lượng sách trên mỗi trang

            IPagedList<Book> model = books.ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        [HttpGet]
        public IActionResult CreateBook()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateBook(Book book, IFormFile file)
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

                    book.Images = "images/" + file.FileName;
                    if (book.Quantity <= 0)
                    {
                        ViewBag.MsgQty = "Quantity should be greater than 0.";
                    }
                    if (book.Price <= 0 || book.Discount <= 0)
                    {
                        ViewBag.MsgPrice = "Price should be greater than 0.";

                    }
                    if (book.Discount >= book.Price)
                    {
                        ViewBag.MsgDP = "Discount should be lower than Price.";
                    }
                    else
                    {
                        _bookRepository.PostBook(book);
                        return RedirectToAction("DisplayBook", "Admin");
                    }

                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }
            return View();
        }

        [HttpGet]
        public IActionResult UpdateBook(string Slug)
        {
            var book = _bookRepository.GetBook(Slug);
            return View(book);
        }

        [HttpPost]
        public IActionResult UpdateBook(Book book, IFormFile file)
        {
            try
            {
                if (file != null)
                {
                    var path = Path.Combine("wwwroot/Images", file.FileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        file.CopyToAsync(stream);
                    }

                    book.Images = "Images/" + file.FileName;
                }
                else
                {
                    // Nếu không có tệp tin mới được tải lên, sử dụng hình ảnh cũ
                    var existingRecipe = _bookRepository.UpdateBook(book.BookId);
                    book.Images = existingRecipe.Images; // Gán lại đường dẫn của hình ảnh cũ
                }

                _bookRepository.UpdateBook(book);

                return RedirectToAction("DisplayBook", "Admin");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }

            return View();
        }


        public IActionResult DeleteBook(int BookID)
        {
            try
            {
                _bookRepository.DeleteBook(BookID);
                return RedirectToAction("DisplayBook", "Admin");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }

            return View();
        }

        [HttpGet]
        public IActionResult DetailBook(string slug)
        {
            var book = _bookRepository.GetBook(slug);
            return View(book);
        }
    

}

}


