using BUOI6.Models;
using Microsoft.EntityFrameworkCore;

namespace BUOI6.Data
{
    public class BookManagementDbContext : DbContext
    {
        public BookManagementDbContext(DbContextOptions<BookManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<BookImage> BookImages { get; set; }
    }
}
