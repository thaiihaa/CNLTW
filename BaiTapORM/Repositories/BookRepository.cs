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
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Images)
                .AsQueryable();

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
                "Author" => sortOrder == "desc" ? query.OrderByDescending(b => (b.Author != null ? b.Author.Name : "")) : query.OrderBy(b => (b.Author != null ? b.Author.Name : "")),
                _ => sortOrder == "desc" ? query.OrderByDescending(b => b.Id) : query.OrderBy(b => b.Id)
            };

            return await query.ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == id);
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
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }
    }
}
