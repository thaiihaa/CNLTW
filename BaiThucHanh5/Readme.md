# Giải thích luồng hoạt động - Bài 6 & Bài 7 (Book Management)

Tài liệu này mô tả chi tiết luồng hoạt động của project "BaiThucHanh3" cho **Bài 6** (thêm sách với validation) và **Bài 7** (middleware ghi log và chặn request không hợp lệ). Mỗi phần liệt kê các file liên quan và các bước thực thi khi người dùng tương tác với ứng dụng.

---

## Tổng quan kiến trúc

- Loại project: ASP.NET Core MVC (Controllers + Views)
- Điểm vào ứng dụng: Program.cs
- Các thành phần chính:
  - Models/Book.cs: lớp dữ liệu với DataAnnotations để xác thực.
  - Services/BookService.cs: quản lý dữ liệu sách (tải / lưu vào Data/books.json, cung cấp API thêm/sửa/xóa/đọc).
  - Controllers/BookController.cs: định nghĩa các action cho CRUD (Index, Detail, Create, Edit, Delete).
  - Views/Book/*.cshtml: giao diện cho danh sách, chi tiết, thêm, sửa, xóa.
  - Middlewares/RequestLoggingMiddleware.cs: ghi log request/response và chặn `Book/Detail` với id không hợp lệ (≤ 0).
  - Data/books.json: file lưu dữ liệu thực tế.

---

## Khởi tạo ứng dụng (Program.cs)

1. WebApplicationBuilder tạo `builder` và cấu hình dịch vụ.
2. `builder.Services.AddSingleton<BookService>()` đăng ký BookService như singleton (một instance trong toàn bộ lifetime ứng dụng).
3. `builder.Services.AddControllersWithViews()` đăng ký MVC controllers + views.
4. `app.UseMiddleware<RequestLoggingMiddleware>()` đăng ký middleware **trước** `MapControllerRoute` để mọi request MVC đi qua pipeline log/chặn.
5. `app.MapControllerRoute(...)` thiết lập routing mặc định: `{controller=Home}/{action=Index}/{id?}`.
6. Kết quả: các request đến `/Book`, `/Book/Create`, `/Book/Detail/1` sẽ đi qua middleware rồi được chuyển đến `BookController` tương ứng.

Lưu ý: BookService đăng ký là singleton => constructor của nó chạy một lần khi container khởi tạo instance lần đầu.

---

## BookService - quản lý dữ liệu

File: Services/BookService.cs

Chức năng chính:
- Xác định đường dẫn file lưu trữ: Data/books.json (dựa vào IWebHostEnvironment.ContentRootPath).
- Nếu file chưa tồn tại hoặc nội dung trống thì khởi tạo bằng `DefaultBooks` và lưu vào file.
- Cung cấp API: GetAll(), GetById(id), Add(book), Update(book), Delete(id).

Chi tiết luồng:
1. Constructor: tạo folder Data nếu chưa có, lưu đường dẫn `_filePath`, sau đó gọi `Load()`.
2. `Load()`:
   - Nếu file không tồn tại: tạo danh sách từ `DefaultBooks`, gọi `Save()` để ghi file và return.
   - Nếu file tồn tại: đọc JSON vào `_books` (List<Book>). Nếu sau khi đọc danh sách rỗng thì thay bằng `DefaultBooks` rồi ghi lại.
3. `Save()` viết `_books` ra file JSON với `JsonSerializer`.
4. Khi thêm sách (`Add`): gán Id mới bằng `(_books.Count == 0 ? 1 : _books.Max(b=>b.Id)+1)`; thêm vào `_books` rồi `Save()`.
5. Sửa/ xóa: tìm theo Id, sửa/xóa trong `_books`, rồi `Save()`.

Đồng bộ hóa:
- Có một `_lock` object; mọi thao tác đọc/ghi trên `_books` được bọc trong `lock(_lock)` để tránh race condition vì BookService là singleton và có thể được truy cập đồng thời từ nhiều request.

---

## Model và Validation

File: Models/Book.cs

- Thuộc tính:
  - Id (int)
  - Name (string) có DataAnnotation `[Required]` và `[Display(Name = "Tên sách")]`.
  - Price (decimal) có `[Range(1, int.MaxValue)]` và `[Display(Name = "Giá")]`.

Ý nghĩa:
- DataAnnotations được dùng để validate server-side (ModelState) và hỗ trợ client-side validation (khi kết hợp với script unobtrusive validation).

---

## Controller - luồng xử lý request

File: Controllers/BookController.cs

Các action chính và luồng:

1. Index()
   - Gọi `_bookService.GetAll()` để lấy danh sách sách.
   - Trả về View(books) => View nhận model là IEnumerable<Book> và hiển thị danh sách.

2. Detail(int id)
   - Trước khi vào action, `RequestLoggingMiddleware` kiểm tra path `/Book/Detail/{id}`: nếu `id <= 0` thì trả **400** + `"Book id không hợp lệ"` và **không** gọi Controller.
   - Gọi `_bookService.GetById(id)`; nếu trả về null => NotFound(); ngược lại View(book).
   - URL ví dụ: `/Book/Detail/1` (hợp lệ); `/Book/Detail/0`, `/Book/Detail/-1`, `/Book/Detail/-5` bị chặn ở middleware.

3. Create() [GET]
   - Trả về form tạo sách (View) để người dùng nhập Name và Price.

4. Create(Book book) [POST]
   - MVC model binding map dữ liệu form vào `book`.
   - `[ValidateAntiForgeryToken]` kiểm tra token chống CSRF.
   - `if (!ModelState.IsValid) return View(book);` => nếu validation thất bại (ví dụ Name trống hoặc Price <= 0), trả lại view cùng lỗi hiển thị.
   - Nếu hợp lệ: gọi `_bookService.Add(new Book { Name = book.Name, Price = book.Price });` để thêm (Id được gán trong service), gọi `Save()` nội bộ.
   - Đặt `ViewBag.SuccessMessage` để hiển thị thông báo thành công, `ModelState.Clear()` rồi trả về `View(new Book())` để reset form.

5. Edit(int id) [GET]
   - Lấy book theo id; nếu không tồn tại => NotFound(); ngược lại trả view với model để sửa.

6. Edit(int id, Book book) [POST]
   - Xác thực id khớp `if (id != book.Id) NotFound()`.
   - Kiểm tra `ModelState.IsValid` tương tự; nếu hợp lệ gọi `_bookService.Update(book)`; nếu update thất bại trả NotFound(); nếu thành công redirect về Index.

7. Delete(int id) [GET]
   - Hiện trang xác nhận xóa, lấy book theo id.

8. DeleteConfirmed(int id) [POST]
   - Thực hiện xóa bằng `_bookService.Delete(id)`; nếu thất bại trả NotFound(); nếu thành công redirect về Index.

---

## Views và client-side validation

Các view liên quan: Views/Book/Create.cshtml, Edit.cshtml, Index.cshtml, Detail.cshtml, Delete.cshtml

- Form tạo/sửa dùng các tag helpers `asp-for` để gắn field với model properties.
- Mỗi form có `@Html.AntiForgeryToken()` và `[ValidateAntiForgeryToken]` trên POST action.
- Validation summary / field validation hiển thị thông báo lỗi từ ModelState via `asp-validation-for` và `asp-validation-summary`.
- Client-side validation: trong các view Create/Edit, phần Scripts include `_ValidationScriptsPartial` (có sẵn các script jQuery validation + unobtrusive). Khi browser tải trang, unobtrusive wires up validation dựa trên attributes HTML (data-val-*) được render từ DataAnnotations. Vì vậy, khi người dùng submit, browser có thể chặn submit nếu có lỗi, giảm số request tới server.

Luồng validation tóm tắt:
- Người dùng submit form -> client validation (nếu bật) kiểm tra -> nếu pass, form gửi request POST -> server model binding map dữ liệu -> server chạy validation dựa trên DataAnnotations -> nếu ModelState invalid, action trả lại view kèm lỗi; nếu valid, tiếp tục lưu.

---

## Dữ liệu thực tế: Data/books.json

- Đây là file JSON chứa mảng các book object: { Id, Name, Price }.
- BookService đọc/ghi file này để giữ trạng thái giữa các lần khởi động app.

---

## Flow ví dụ: Thêm sách mới (chi tiết từng bước)

1. Người dùng mở `/Book/Create` (GET) -> BookController.Create() trả về view form.
2. Người dùng nhập `Name` và `Price` rồi bấm "Thêm".
3. Trình duyệt thực hiện client-side validation (nếu có): kiểm tra dữ liệu theo attribute (required, range). Nếu lỗi, hiển thị lỗi và không submit.
4. Nếu pass, form gửi HTTP POST tới `/Book/Create` cùng anti-forgery token.
5. Server nhận POST, model binding tạo Book object từ dữ liệu form.
6. `[ValidateAntiForgeryToken]` kiểm tra token hợp lệ.
7. Server kiểm tra `ModelState.IsValid` (DataAnnotations): nếu invalid -> trả về view với lỗi; nếu valid -> gọi BookService.Add(book) để thêm:
   - BookService gán Id mới, thêm vào List, Save() để ghi file Data/books.json.
8. Controller đặt ViewBag.SuccessMessage và ModelState.Clear(), trả về form rỗng để người dùng có thể thêm tiếp.

---

## Bài 7 – RequestLoggingMiddleware

File: `Middlewares/RequestLoggingMiddleware.cs`

### Vai trò

Middleware nằm trong **HTTP request pipeline**, xử lý mọi request trước (và một phần sau) khi request tới Controller.

### Luồng xử lý trong `InvokeAsync`

1. **Trước `_next`**: Ghi Console dạng `[yyyy-MM-dd HH:mm:ss] Method: GET - Path: /Book`.
2. **Kiểm tra chặn**: Nếu path khớp `/Book/Detail/{id}` (không phân biệt hoa thường phần prefix) và `id` parse được là số nguyên **≤ 0**:
   - `StatusCode = 400`
   - Body: `Book id không hợp lệ`
   - `return` — **không** gọi `_next`, request không vào `BookController.Detail`.
3. **`await _next(context)`**: Chuyển request cho middleware/endpoint tiếp theo (routing → Controller → View).
4. **Sau `_next`**: Ghi `Status Code: {mã}` (200, 404, 400, …) theo response thực tế.

### Hàm `IsInvalidBookDetailPath`

- Prefix cố định: `/Book/Detail/`.
- Lấy segment cuối làm `id`, dùng `int.TryParse`.
- Trả `true` khi `id <= 0` (bao gồm `0`, `-1`, `-100`, …).
- Path không đúng dạng Detail hoặc id không phải số nguyên → middleware **không** chặn (để routing/Controller xử lý).

### Đăng ký trong Program.cs

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Middleware **phải** đặt trước `MapControllerRoute`. Nếu đặt sau, endpoint có thể đã được map và middleware không chạy đúng cho request MVC.

### Flow ví dụ: `/Book/Detail/0`

1. Request GET `/Book/Detail/0` vào pipeline.
2. Middleware log: `[...] Method: GET - Path: /Book/Detail/0`.
3. `IsInvalidBookDetailPath` → `true` → response 400 + text, `return`.
4. Middleware ghi `Status Code: 400` rồi `return` (không gọi `_next`).

### Flow ví dụ: `/Book/Detail/1`

1. Middleware log request.
2. `IsInvalidBookDetailPath` → `false` → `await _next(context)`.
3. Routing → `BookController.Detail(1)` → View hoặc 404 nếu không có sách.
4. Middleware log: `Status Code: 200` (hoặc 404).

### Kiểm tra nhanh (video nộp bài)

| URL | Kỳ vọng Console | Kỳ vọng trình duyệt |
|-----|-----------------|---------------------|
| `/Book` | GET + Status 200 | Danh sách sách |
| `/Book/Detail/1` | GET + Status 200/404 | Chi tiết hoặc Not Found |
| `/Book/Create` POST | POST + Status 200 | Form thêm sách |
| `/Book/Detail/0` | GET + Status 400 | `Book id không hợp lệ`, HTTP 400 |
| `/Book/Detail/-5` | Tương tự 0 | 400, không vào Controller |

### Câu hỏi báo cáo (tóm tắt)

| Câu hỏi | Trả lời ngắn |
|---------|----------------|
| Middleware dùng để làm gì? | Xử lý chuỗi request/response chung (log, auth, chặn, …) trước/sau endpoint. |
| Khác Controller? | Middleware theo pipeline; Controller theo route/action và logic nghiệp vụ. |
| `await _next(context)`? | Gọi bước tiếp theo trong pipeline; code sau dòng này chạy khi response quay lại. |
| Vì sao `return` không vào Controller? | Không gọi `_next` → pipeline dừng, response đã gửi. |
| Middleware sau `MapControllerRoute`? | Có thể không chạy cho request MVC → mất log/chặn. |

---

## Ghi chú & điểm cần lưu ý

- BookService là singleton: đảm bảo dùng lock để tránh race khi nhiều request cùng thao tác ghi file.
- Id tự gán trong Add(): nếu file bị sửa tay và có Id không liên tiếp thì method vẫn gán Id = Max(Id)+1.
- Validation thực hiện ở hai tầng: client (unobtrusive) và server (ModelState). Luôn kiểm tra ModelState trên server dù có client validation.
- File Data/books.json có thể bị lock/không ghi được nếu environment hoặc quyền file hệ thống giới hạn; BookService hiện không có xử lý lỗi IO ngoại lệ chi tiết.
- Middleware chỉ chặn **Detail** với `id <= 0`; Edit/Delete với id âm vẫn do Controller xử lý (có thể mở rộng tương tự nếu cần).

---
