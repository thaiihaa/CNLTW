using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace BUOI6.Data
{
    public static class DbInitializer
    {
        private const string DatabaseName = "NguyenThaiHa_19062006";

        public static void Initialize(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("BookManagementConnection")
                ?? throw new InvalidOperationException("Connection string 'BookManagementConnection' not found.");

            CreateDatabaseIfNotExists(connectionString);
            CreateTableIfNotExists(connectionString);
            SeedSampleData(connectionString);
        }

        /// <summary>
        /// Bước 1: Kết nối tới master và tạo CSDL BookManagement nếu chưa có.
        /// </summary>
        private static void CreateDatabaseIfNotExists(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            builder.InitialCatalog = "master";

            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();

            var sql = $@"
                IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{DatabaseName}')
                BEGIN
                    CREATE DATABASE [{DatabaseName}];
                END";

            using var command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Bước 2: Kết nối tới BookManagement và tạo bảng Book nếu chưa có.
        /// </summary>
        private static void CreateTableIfNotExists(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var sql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NguyenThaiHa_19062006_Author')
                BEGIN
                    CREATE TABLE [NguyenThaiHa_19062006_Author] (
                        [Id]          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Name]        NVARCHAR(200)     NOT NULL,
                        [Description] NVARCHAR(500)     NULL
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NguyenThaiHa_19062006_Book')
                BEGIN
                    CREATE TABLE [NguyenThaiHa_19062006_Book] (
                        [Id]          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Title]       NVARCHAR(200)     NOT NULL,
                        [AuthorId]    INT               NOT NULL,
                        [Price]       DECIMAL(18,2)     NOT NULL,
                        [Quantity]    INT               NOT NULL,
                        [Description] NVARCHAR(500)     NULL,
                        CONSTRAINT FK_Book_Author FOREIGN KEY(AuthorId) REFERENCES [NguyenThaiHa_19062006_Author](Id)
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NguyenThaiHa_19062006_BookImage')
                BEGIN
                    CREATE TABLE [NguyenThaiHa_19062006_BookImage] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [BookId] INT NOT NULL,
                        [FileName] NVARCHAR(260) NOT NULL,
                        CONSTRAINT FK_BookImage_Book FOREIGN KEY(BookId) REFERENCES [NguyenThaiHa_19062006_Book](Id)
                    );
                END";

            using var command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Bước 3: Thêm dữ liệu mẫu nếu bảng Book đang trống.
        /// </summary>
        private static void SeedSampleData(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BookManagementDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new BookManagementDbContext(optionsBuilder.Options);

            if (context.Authors.Any() || context.Books.Any())
                return;

            var a1 = new Models.Author { Name = "Nguyễn Văn A", Description = "Tác giả A" };
            var a2 = new Models.Author { Name = "Trần Thị B", Description = "Tác giả B" };
            var a3 = new Models.Author { Name = "Lê Văn C", Description = "Tác giả C" };

            context.Authors.AddRange(a1, a2, a3);
            context.SaveChanges();

            context.Books.AddRange(
                new Models.Book
                {
                    Title = "Lập trình C# cơ bản",
                    AuthorId = a1.Id,
                    Price = 120000,
                    Quantity = 50,
                    Description = "Sách hướng dẫn lập trình C# cho người mới bắt đầu."
                },
                new Models.Book
                {
                    Title = "ASP.NET Core MVC",
                    AuthorId = a2.Id,
                    Price = 150000,
                    Quantity = 30,
                    Description = "Xây dựng ứng dụng web với ASP.NET Core MVC."
                },
                new Models.Book
                {
                    Title = "SQL Server thực hành",
                    AuthorId = a3.Id,
                    Price = 99000,
                    Quantity = 40,
                    Description = "Thực hành quản lý cơ sở dữ liệu SQL Server."
                }
            );

            // Additional books requested by user with distinct authors
            var a4 = new Models.Author { Name = "Ian Stewart", Description = "Author" };
            var a5 = new Models.Author { Name = "Editorial Team", Description = "Editor" };
            var a6 = new Models.Author { Name = "Dale Carnegie", Description = "Author" };
            context.Authors.AddRange(a4, a5, a6);
            context.SaveChanges();

            var b1 = new Models.Book
            {
                Title = "17 phương trình",
                AuthorId = a4.Id,
                Price = 89000,
                Quantity = 20,
                Description = "Tuyển tập 17 phương trình quan trọng trong toán học và vật lý."
            };

            var b2 = new Models.Book
            {
                Title = "100 nhà khoa học",
                AuthorId = a5.Id,
                Price = 120000,
                Quantity = 15,
                Description = "Hồ sơ và đóng góp của 100 nhà khoa học có ảnh hưởng."
            };

            var b3 = new Models.Book
            {
                Title = "Đắc nhân tâm",
                AuthorId = a6.Id,
                Price = 150000,
                Quantity = 25,
                Description = "Bản dịch/ấn bản Đắc nhân tâm - kỹ năng giao tiếp và thành công."
            };

            context.Books.AddRange(b1, b2, b3);
            context.SaveChanges();

            // Copy seed images into wwwroot/images/books and create thumbnails (200x280)
            var contentRoot = Directory.GetCurrentDirectory();
            var srcFolder = Path.Combine(contentRoot, "images");
            var destFolder = Path.Combine(contentRoot, "wwwroot", "images", "books");
            Directory.CreateDirectory(destFolder);

            void CopyImage(string srcFileName, int bookId)
            {
                var srcPath = Path.Combine(srcFolder, srcFileName);
                if (!File.Exists(srcPath))
                {
                    // try alternative relative path
                    srcPath = Path.Combine(contentRoot, "BUOI6", "images", srcFileName);
                }
                if (!File.Exists(srcPath)) return;

                var destName = Path.GetFileName(srcFileName);
                var destPath = Path.Combine(destFolder, destName);
                File.Copy(srcPath, destPath, true);

                context.BookImages.Add(new Models.BookImage { BookId = bookId, FileName = Path.Combine("images", "books", destName).Replace("\\", "/") });
            }

            CopyImage("17-phuong-trinh.jpg", b1.Id);
            CopyImage("100-nha-khoa-hoc.jpg", b2.Id);
            CopyImage("dac-nhan-tam.jpg", b3.Id);

            context.SaveChanges();
        }
    }
}
