using BUOI6.Data;
using BUOI6.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// 1. Đăng ký DbContext (kết nối SQL Server)
builder.Services.AddDbContext<BookManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BookManagementConnection")));

// 2. Đăng ký BookRepository (xử lý CRUD qua Repository pattern)
builder.Services.AddScoped<IBookRepository, BookRepository>();

var app = builder.Build();

// Tự động tạo CSDL + bảng Book khi ứng dụng khởi động
DbInitializer.Initialize(app.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
