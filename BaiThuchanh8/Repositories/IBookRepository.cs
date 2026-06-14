using BUOI6.Models;

namespace BUOI6.Repositories
{
    public interface IBookRepository
    {
        Task<List<Book>> GetAllAsync(string? titleSearch, string? sortBy, string? sortOrder);
        Task<Book?> GetByIdAsync(int id);
        Task AddAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(int id);
        Task AddImagesAsync(int bookId, List<BookImage> images);
        Task DeleteImageAsync(int imageId);
        Task<BookImage?> GetImageByIdAsync(int id);
    }
}
