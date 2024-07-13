using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Repository;
using X.PagedList;

namespace Project.Controllers
{
    public class BookController : Controller
    {
        private IBookRepository _bookRepository;
        public BookController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet]
        public IActionResult Book(int? page, string searchName)
        {
            var model = _bookRepository.GetBooks();
            IEnumerable<Book> model2 = model;

            if (!string.IsNullOrEmpty(searchName))
            {
                model2 = model.Where(book => book.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase));
            }

            int pageNumber = page ?? 1;
            int pageSize = 12;

            IPagedList<Book> pagedBooks = model2.ToPagedList(pageNumber, pageSize);

            return View(pagedBooks);
        }


        [HttpGet]
        public IActionResult BookDetail(string slug)
        {
            var model = _bookRepository.GetBook(slug);
            return View(model);
        }
    }
}
