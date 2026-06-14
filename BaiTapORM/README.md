# BUOI6 - Quản lý sách (BookManagement)

Ứng dụng ASP.NET Core MVC quản lý sách với SQL Server, Repository pattern.

---

## 1. Kết nối DB đăng ký ở đâu?

### Bước 1: Cấu hình connection string — `appsettings.json`

```json
"ConnectionStrings": {
  "BookManagementConnection": "Server=LAPTOP-12OGD3V1\\MSSQLSERVER01;Database=BookManagement;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Bước 2: Đăng ký DbContext — `Program.cs`

```csharp
builder.Services.AddDbContext<BookManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BookManagementConnection")));
```

| Vị trí | Vai trò |
|--------|---------|
| `appsettings.json` | Lưu chuỗi kết nối SQL Server |
| `Program.cs` → `AddDbContext` | Đăng ký `BookManagementDbContext` vào DI container |
| `BookManagementDbContext` | Class trung gian giao tiếp với CSDL qua EF Core |

Khi app chạy, ASP.NET Core tự tạo `BookManagementDbContext` và inject vào các class cần dùng.

---

## 2. CRUD gọi qua BookRepository như thế nào?

### Kiến trúc

```
View  →  BookController  →  IBookRepository  →  BookRepository  →  DbContext  →  SQL Server
```

### Bước 1: Đăng ký Repository — `Program.cs`

```csharp
builder.Services.AddScoped<IBookRepository, BookRepository>();
```

- `IBookRepository`: interface định nghĩa các hàm CRUD
- `BookRepository`: class thực thi, dùng `BookManagementDbContext` truy vấn CSDL
- `AddScoped`: mỗi request HTTP tạo 1 instance Repository

### Bước 2: Interface — `Repositories/IBookRepository.cs`

```csharp
public interface IBookRepository
{
    Task<List<Book>> GetAllAsync(string? titleSearch, string? sortBy, string? sortOrder);
    Task<Book?> GetByIdAsync(int id);
    Task AddAsync(Book book);
    Task UpdateAsync(Book book);
    Task DeleteAsync(int id);
}
```

### Bước 3: Implementation — `Repositories/BookRepository.cs`

| Hàm Repository | Thao tác SQL |
|----------------|--------------|
| `GetAllAsync` | `SELECT` + `WHERE Title LIKE` + `ORDER BY` |
| `GetByIdAsync` | `SELECT` theo `Id` |
| `AddAsync` | `INSERT` |
| `UpdateAsync` | `UPDATE` |
| `DeleteAsync` | `DELETE` |

### Bước 4: Controller gọi Repository — `Controllers/BookController.cs`

```csharp
public class BookController : Controller
{
    private readonly IBookRepository _bookRepository;

    public BookController(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;  // DI tự inject
    }

    public async Task<IActionResult> Index(...)
        => View(await _bookRepository.GetAllAsync(...));

    public async Task<IActionResult> Create(Book book)
    {
        await _bookRepository.AddAsync(book);
        ...
    }
}
```

**Luồng gọi ví dụ khi thêm sách:**

1. User submit form → `POST /Book/Create`
2. `BookController.Create(book)` kiểm tra validation
3. Gọi `_bookRepository.AddAsync(book)`
4. `BookRepository` → `_context.Books.Add(book)` → `SaveChangesAsync()`
5. EF Core sinh câu `INSERT INTO Book ...` gửi SQL Server

---

## 3. Tìm kiếm & Sắp xếp

### Tìm kiếm theo tên sách

- URL: `/Book?search=Lập trình`
- Code trong `BookRepository.GetAllAsync`:

```csharp
if (!string.IsNullOrWhiteSpace(titleSearch))
    query = query.Where(b => b.Title.Contains(titleSearch));
```

- View: ô nhập "Tìm kiếm theo tên sách..." trên trang `/Book`

### Sắp xếp theo giá

- URL: `/Book?sortBy=Price&sortOrder=asc` (giá tăng dần)
- URL: `/Book?sortBy=Price&sortOrder=desc` (giá giảm dần)
- Code trong `BookRepository.GetAllAsync`:

```csharp
"Price" => sortOrder == "desc"
    ? query.OrderByDescending(b => b.Price)
    : query.OrderBy(b => b.Price),
```

- View: click tiêu đề cột **Giá** trên bảng để đổi chiều sắp xếp (▲ tăng / ▼ giảm)

### Kết hợp tìm + sắp xếp

```
/Book?search=ASP.NET&sortBy=Price&sortOrder=desc
```

→ Tìm sách có tên chứa "ASP.NET", sắp xếp giá từ cao xuống thấp.

---

## 4. Cấu trúc thư mục

```
BUOI6/
├── Controllers/
│   └── BookController.cs       # Nhận request, gọi Repository
├── Data/
│   ├── BookManagementDbContext.cs
│   └── DbInitializer.cs        # Tự tạo DB + bảng
├── Models/
│   └── Book.cs
├── Repositories/
│   ├── IBookRepository.cs      # Interface CRUD
│   └── BookRepository.cs       # Thực thi truy vấn CSDL
├── Views/Book/
│   ├── Index.cshtml            # Danh sách + tìm kiếm + sắp xếp
│   ├── Create.cshtml
│   ├── Edit.cshtml
│   ├── Delete.cshtml
│   └── Detail.cshtml
├── appsettings.json            # Connection string
└── Program.cs                  # Đăng ký DbContext + Repository
```

---

## 5. Cách chạy

```bash
cd BUOI6
dotnet run
```

Mở trình duyệt: **http://localhost:5080/Book**

**Yêu cầu:** SQL Server `LAPTOP-12OGD3V1\MSSQLSERVER01` đang chạy, Windows Authentication.
