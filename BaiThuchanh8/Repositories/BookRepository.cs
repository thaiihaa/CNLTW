using BUOI6.Data;
using BUOI6.Models;
using Microsoft.EntityFrameworkCore;

namespace BUOI6.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly BookManagementDbContext _context;

        public BookRepository(BookManagementDbContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetAllAsync(string? titleSearch, string? sortBy, string? sortOrder)
        {
            var query = _context.Books.Include(b => b.Images).AsQueryable();

            // Tìm kiếm theo tên sách
            if (!string.IsNullOrWhiteSpace(titleSearch))
            {
                titleSearch = titleSearch.Trim();
                query = query.Where(b => b.Title.Contains(titleSearch));
            }

            // Sắp xếp (mặc định theo Id, hỗ trợ sắp xếp theo giá)
            sortBy ??= "Id";
            sortOrder = sortOrder == "desc" ? "desc" : "asc";

            query = sortBy switch
            {
                "Title" => sortOrder == "desc" ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
                "Price" => sortOrder == "desc" ? query.OrderByDescending(b => b.Price) : query.OrderBy(b => b.Price),
                "Author" => sortOrder == "desc" ? query.OrderByDescending(b => b.Author) : query.OrderBy(b => b.Author),
                "Quantity" => sortOrder == "desc" ? query.OrderByDescending(b => b.Quantity) : query.OrderBy(b => b.Quantity),
                _ => sortOrder == "desc" ? query.OrderByDescending(b => b.Id) : query.OrderBy(b => b.Id)
            };

            return await query.ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var book = await _context.Books.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
            if (book != null)
            {
                // remove related images
                if (book.Images != null && book.Images.Count > 0)
                {
                    _context.BookImages.RemoveRange(book.Images);
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddImagesAsync(int bookId, List<BookImage> images)
        {
            foreach (var img in images)
            {
                img.BookId = bookId;
                _context.BookImages.Add(img);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteImageAsync(int imageId)
        {
            var img = await _context.BookImages.FindAsync(imageId);
            if (img != null)
            {
                _context.BookImages.Remove(img);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<BookImage?> GetImageByIdAsync(int id)
        {
            return await _context.BookImages.FindAsync(id);
        }
    }
}
