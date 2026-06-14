# BUOI6 - Quản lý sách có upload hình ảnh

Ứng dụng ASP.NET Core MVC quản lý sách bằng SQL Server, Entity Framework Core và Repository pattern. Phần mới được bổ sung là chức năng upload hình ảnh cho sách: cho phép chọn 1 hoặc nhiều ảnh, chỉ chấp nhận JPG/PNG, lưu file ảnh lên server và hiển thị ảnh trong danh sách sách.

---

## 1. Kịch bản thuyết minh yêu cầu thêm hình ảnh

### Mở đầu

Trong bài này, em bổ sung chức năng upload hình ảnh cho module Book. Mục tiêu là khi thêm hoặc sửa một quyển sách, người dùng có thể chọn ảnh sản phẩm từ máy tính. Ảnh hợp lệ sẽ được lưu lên server, đường dẫn ảnh sẽ được lưu vào cơ sở dữ liệu, sau đó danh sách sách sẽ hiển thị các thông tin chính gồm tên sách, giá và hình ảnh.

Yêu cầu chính của chức năng gồm:

- Cho phép upload 1 hoặc nhiều hình ảnh cho một sách.
- Chỉ cho phép file ảnh định dạng JPG, JPEG hoặc PNG.
- Nếu người dùng chọn file khác định dạng ảnh cho phép, hệ thống báo lỗi và không lưu dữ liệu sai.
- Ảnh được lưu trong thư mục `wwwroot/images`.
- Cơ sở dữ liệu chỉ lưu đường dẫn ảnh, không lưu trực tiếp dữ liệu file ảnh.
- Trang danh sách sách hiển thị tên, giá và ảnh đại diện.

### Luồng người dùng

Khi người dùng vào trang thêm sách, form sẽ có thêm một ô chọn file với tên `imageFiles`. Ô này cho phép chọn nhiều ảnh cùng lúc nhờ thuộc tính `multiple`.

Sau khi người dùng nhập thông tin sách và chọn ảnh, người dùng bấm nút thêm mới. Form sẽ gửi dữ liệu bằng phương thức `POST` tới action `Create` trong `BookController`. Vì form có upload file nên trong view phải có:

```html
enctype="multipart/form-data"
```

Nếu thiếu dòng này thì trình duyệt sẽ không gửi nội dung file lên server.

### Luồng xử lý trong controller

Ở `BookController`, action `Create` nhận 2 phần dữ liệu:

```csharp
public async Task<IActionResult> Create(Book book, List<IFormFile>? imageFiles)
```

Trong đó:

- `Book book` là dữ liệu thông tin sách như tên, tác giả, giá, số lượng, mô tả.
- `List<IFormFile>? imageFiles` là danh sách file ảnh người dùng upload.

Controller xử lý theo thứ tự:

1. Lọc ra các file thật sự được upload bằng hàm `GetUploadedFiles`.
2. Kiểm tra định dạng ảnh bằng hàm `ValidateImageFiles`.
3. Nếu file không phải JPG/JPEG/PNG thì thêm lỗi vào `ModelState` và trả lại form.
4. Nếu dữ liệu hợp lệ thì lưu thông tin sách vào bảng `Book`.
5. Sau khi sách được lưu, hệ thống có `book.Id`.
6. Từng ảnh được lưu vào thư mục `wwwroot/images` bằng hàm `SaveImageAsync`.
7. Mỗi đường dẫn ảnh được đưa vào model `BookImage`.
8. Repository lưu danh sách ảnh vào bảng `BookImage`.
9. Chuyển hướng về trang danh sách.

Điểm quan trọng là hệ thống validate ảnh trước khi lưu sách. Nhờ vậy nếu người dùng chọn file sai định dạng, chương trình không tạo bản ghi sách lỗi trong cơ sở dữ liệu.

### Luồng lưu file ảnh

File ảnh không được lưu trực tiếp vào SQL Server. Thay vào đó, chương trình lưu file vật lý vào server:

```text
wwwroot/images
```

Tên file được đổi thành `Guid` để tránh trùng tên:

```csharp
var fileName = $"{Guid.NewGuid()}{extension}";
```

Ví dụ, người dùng upload file `book.png`, hệ thống có thể lưu thành:

```text
wwwroot/images/7f7c1f5e-6c4c-4c71-a6a8-8a66f4f6b820.png
```

Còn trong database chỉ lưu đường dẫn web:

```text
/images/7f7c1f5e-6c4c-4c71-a6a8-8a66f4f6b820.png
```

Cách làm này nhẹ hơn, dễ hiển thị hơn và phù hợp với ứng dụng web.

### Luồng hiển thị ảnh

Khi vào trang danh sách sách, action `Index` gọi:

```csharp
_bookRepository.GetAllAsync(search, sortBy, sortOrder)
```

Trong repository, dữ liệu sách được lấy kèm ảnh bằng `Include`:

```csharp
var query = _context.Books.Include(b => b.Images).AsQueryable();
```

Nhờ đó, mỗi sách có danh sách ảnh đi kèm. Trong view `Views/Book/Index.cshtml`, nếu sách có ảnh thì hiển thị ảnh đầu tiên:

```cshtml
@if (item.Images != null && item.Images.Any())
{
    <img src="@item.Images.First().ImagePath" alt="Ảnh sản phẩm" />
}
else
{
    <span class="text-muted">(Chưa có)</span>
}
```

Vì ảnh nằm trong `wwwroot/images`, ứng dụng cần bật static file middleware trong `Program.cs`:

```csharp
app.UseStaticFiles();
```

Middleware này cho phép trình duyệt truy cập các file tĩnh như ảnh, CSS, JavaScript trong thư mục `wwwroot`.

### Luồng sửa và xóa ảnh

Ở trang sửa sách, người dùng có thể upload thêm ảnh mới. Các ảnh cũ vẫn được giữ lại. Nếu muốn xóa một ảnh, form sẽ gọi action `DeleteImage`.

Action `DeleteImage` xử lý 2 việc:

1. Xóa file ảnh vật lý trong `wwwroot/images`.
2. Xóa bản ghi ảnh trong bảng `BookImage`.

Khi xóa cả sách, controller cũng duyệt qua danh sách ảnh của sách đó và xóa file ảnh khỏi server trước khi xóa dữ liệu trong database.

---

## 2. Các file liên quan đến chức năng upload ảnh

| File | Vai trò |
| --- | --- |
| `BUOI6/Views/Book/Create.cshtml` | Form thêm sách, có input upload nhiều ảnh |
| `BUOI6/Views/Book/Edit.cshtml` | Form sửa sách, hiển thị ảnh hiện tại và cho upload thêm ảnh |
| `BUOI6/Views/Book/Index.cshtml` | Hiển thị danh sách sách gồm tên, giá, hình ảnh |
| `BUOI6/Controllers/BookController.cs` | Nhận file upload, validate định dạng, lưu file, xóa file |
| `BUOI6/Models/Book.cs` | Model sách, có quan hệ `ICollection<BookImage>` |
| `BUOI6/Models/BookImage.cs` | Model lưu đường dẫn ảnh của sách |
| `BUOI6/Repositories/BookRepository.cs` | Lấy sách kèm ảnh, thêm ảnh, xóa ảnh |
| `BUOI6/Data/BookManagementDbContext.cs` | Khai báo `DbSet<Book>` và `DbSet<BookImage>` |
| `BUOI6/Program.cs` | Bật `UseStaticFiles()` để hiển thị ảnh trong `wwwroot` |

---

## 3. Model dữ liệu ảnh

### Book

Model `Book` có quan hệ một-nhiều với `BookImage`:

```csharp
public ICollection<BookImage>? Images { get; set; }
```

Một sách có thể có nhiều ảnh.

### BookImage

Model `BookImage` lưu thông tin từng ảnh:

```csharp
public class BookImage
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public Book? Book { get; set; }
}
```

Ý nghĩa:

- `Id`: khóa chính của ảnh.
- `BookId`: khóa ngoại liên kết ảnh với sách.
- `ImagePath`: đường dẫn ảnh trong web, ví dụ `/images/abc.png`.
- `Book`: navigation property để EF Core hiểu quan hệ giữa ảnh và sách.

---

## 4. Validate định dạng ảnh

Controller chỉ chấp nhận các đuôi file:

```csharp
".jpg", ".jpeg", ".png"
```

Và chỉ chấp nhận MIME type:

```csharp
"image/jpeg", "image/png"
```

Hàm kiểm tra:

```csharp
private static bool IsSupportedImage(IFormFile file)
{
    var extension = Path.GetExtension(file.FileName);
    return AllowedImageExtensions.Contains(extension)
        && AllowedImageContentTypes.Contains(file.ContentType);
}
```

Nếu file không hợp lệ, controller báo lỗi:

```csharp
ModelState.AddModelError("imageFiles", "Chỉ chấp nhận ảnh JPG hoặc PNG.");
```

Sau đó trả lại view để người dùng chọn lại file.

---

## 5. Luồng tổng quát của chức năng

```text
Người dùng chọn ảnh
        ↓
Form Create/Edit gửi multipart/form-data
        ↓
BookController nhận List<IFormFile> imageFiles
        ↓
Lọc file rỗng
        ↓
Kiểm tra JPG/JPEG/PNG
        ↓
Sai định dạng → báo lỗi, không lưu
        ↓
Đúng định dạng → lưu sách vào bảng Book
        ↓
Lưu file ảnh vào wwwroot/images
        ↓
Lưu đường dẫn ảnh vào bảng BookImage
        ↓
Index lấy sách kèm ảnh bằng Include
        ↓
View hiển thị tên, giá, hình ảnh
```

---

## 6. Kết nối database và Repository pattern

### Đăng ký DbContext

Trong `Program.cs`:

```csharp
builder.Services.AddDbContext<BookManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BookManagementConnection")));
```

Connection string nằm trong `appsettings.json`:

```json
"ConnectionStrings": {
  "BookManagementConnection": "Server=LAPTOP-12OGD3V1\\MSSQLSERVER01;Database=BookManagement;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Đăng ký Repository

```csharp
builder.Services.AddScoped<IBookRepository, BookRepository>();
```

Controller không làm việc trực tiếp với `DbContext`, mà gọi qua `IBookRepository`. Cách này giúp code rõ ràng hơn:

```text
View → BookController → IBookRepository → BookRepository → DbContext → SQL Server
```

---

## 7. Hiển thị danh sách sách

Trang danh sách nằm ở:

```text
BUOI6/Views/Book/Index.cshtml
```

Các cột chính theo yêu cầu:

- Tên
- Giá
- Hình ảnh

Ngoài ra trang vẫn giữ các cột và chức năng hỗ trợ như tác giả, số lượng, chi tiết, sửa, xóa.

---

## 8. Cách chạy chương trình

```bash
cd BUOI6
dotnet run
```

Mở trình duyệt:

```text
http://localhost:5080/Book
```

Nếu gặp lỗi port `5080` đã được sử dụng, dừng process đang giữ port hoặc đổi port chạy ứng dụng.

---

## 9. Tóm tắt khi trình bày

Có thể nói ngắn gọn như sau:

Em đã thêm chức năng upload hình ảnh cho sách. Ở giao diện thêm và sửa sách, em dùng input file có `multiple` và form `multipart/form-data` để gửi ảnh lên server. Ở controller, em nhận ảnh bằng `List<IFormFile>`, kiểm tra file rỗng và kiểm tra định dạng ảnh. Hệ thống chỉ cho phép JPG, JPEG và PNG. Nếu sai định dạng thì báo lỗi và không lưu dữ liệu. Nếu hợp lệ, hệ thống lưu thông tin sách vào bảng `Book`, lưu file ảnh vào thư mục `wwwroot/images`, sau đó lưu đường dẫn ảnh vào bảng `BookImage`. Khi hiển thị danh sách, repository dùng `Include` để lấy sách kèm ảnh, view lấy ảnh đầu tiên làm ảnh đại diện và hiển thị cùng tên, giá của sách. Ngoài ra, em có thêm xử lý xóa ảnh để khi xóa sách hoặc xóa từng ảnh thì file trên server và bản ghi trong database được xóa đồng bộ.
