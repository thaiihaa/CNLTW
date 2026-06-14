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
        public DbSet<Author> Authors { get; set; }
        public DbSet<BookImage> BookImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookImage>()
                .HasOne(i => i.Book)
                .WithMany(b => b.Images)
                .HasForeignKey(i => i.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
