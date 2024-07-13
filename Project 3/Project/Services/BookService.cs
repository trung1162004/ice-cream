
using Project.Data;
using Project.Models;
using Project.Repository;

namespace Project.Services
{
    public class BookService : IBookRepository
    {
        private AppDbContext _context;
        public BookService(AppDbContext context)
        {
            _context = context;
        }

        public Book GetBook(string slug)
        {
            var book = _context.Books.SingleOrDefault(x => x.Slug == slug);
            return book!;
        }

        public List<Book> GetBooks()
        {
            return _context.Books.ToList();
        }
        public void DeleteBook(int BookID)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == BookID);
            if (book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
            }
        }
        public void PostBook(Book newBook)
        {
            _context.Books.Add(newBook);
            _context.SaveChanges();
        }

        public void UpdateBook(Book updateBook)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == updateBook.BookId);
            if (book != null)
            {
                book.Name = updateBook.Name;
                book.Description = updateBook.Description;
                book.Price = updateBook.Price;
                book.Discount = updateBook.Discount;
                book.Slug = updateBook.Slug;
                book.Quantity = updateBook.Quantity;
                book.Images = updateBook.Images;
                _context.SaveChanges();
            }
        }

        public Book UpdateBook(int id)
        {
            return _context.Books.SingleOrDefault(s => s.BookId.Equals(id));
        }

        public void DecreaseQuantity(int bookId, int quantityToDecrease)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == bookId);

            if (book != null)
            {
                if (quantityToDecrease <= book.Quantity)
                {
                    book.Quantity -= quantityToDecrease;
                    _context.SaveChanges();
                }
            }
        }
    }
}
