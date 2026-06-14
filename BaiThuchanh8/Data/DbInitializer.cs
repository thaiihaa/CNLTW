using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BUOI6.Data
{
    public static class DbInitializer
    {
        private const string DatabaseName = "BookManagement";

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
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Book')
                BEGIN
                    CREATE TABLE [Book] (
                        [Id]          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Title]       NVARCHAR(200)     NOT NULL,
                        [Author]      NVARCHAR(100)     NOT NULL,
                        [Price]       DECIMAL(18,2)     NOT NULL,
                        [Quantity]    INT               NOT NULL,
                        [Description] NVARCHAR(500)     NULL
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

            if (context.Books.Any())
                return;

            context.Books.AddRange(
                new Models.Book
                {
                    Title = "Lập trình C# cơ bản",
                    Author = "Nguyễn Văn A",
                    Price = 120000,
                    Quantity = 50,
                    Description = "Sách hướng dẫn lập trình C# cho người mới bắt đầu."
                },
                new Models.Book
                {
                    Title = "ASP.NET Core MVC",
                    Author = "Trần Thị B",
                    Price = 150000,
                    Quantity = 30,
                    Description = "Xây dựng ứng dụng web với ASP.NET Core MVC."
                },
                new Models.Book
                {
                    Title = "SQL Server thực hành",
                    Author = "Lê Văn C",
                    Price = 99000,
                    Quantity = 40,
                    Description = "Thực hành quản lý cơ sở dữ liệu SQL Server."
                }
            );

            context.SaveChanges();
        }
    }
}
