using Project.Models;

namespace Project.Repository
{
    public interface IBookRepository
    {
        List<Book> GetBooks();

        Book GetBook(string slug);
        void PostBook(Book newBook);
        void UpdateBook(Book updatedBook);
        Book UpdateBook(int id);
        void DeleteBook(int BookID);
        void DecreaseQuantity(int bookId, int quantityToDecrease);

    }
}
