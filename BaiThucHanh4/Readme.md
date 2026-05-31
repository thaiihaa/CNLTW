# Giải thích luồng hoạt động - Bài 6 (Thêm sách + Validation)

Tài liệu này mô tả chi tiết luồng hoạt động của project "BaiThucHanh3" cho Bài 6 (thêm sách với validation). Mỗi phần liệt kê các file liên quan và các bước thực thi khi người dùng tương tác với ứng dụng.

---

## Tổng quan kiến trúc

- Loại project: ASP.NET Core MVC (Controllers + Views)
- Điểm vào ứng dụng: Program.cs
- Các thành phần chính:
  - Models/Book.cs: lớp dữ liệu với DataAnnotations để xác thực.
  - Services/BookService.cs: quản lý dữ liệu sách (tải / lưu vào Data/books.json, cung cấp API thêm/sửa/xóa/đọc).
  - Controllers/BookController.cs: định nghĩa các action cho CRUD (Index, Detail, Create, Edit, Delete).
  - Views/Book/*.cshtml: giao diện cho danh sách, chi tiết, thêm, sửa, xóa.
  - Data/books.json: file lưu dữ liệu thực tế.

---

## Khởi tạo ứng dụng (Program.cs)

1. WebApplicationBuilder tạo `builder` và cấu hình dịch vụ.
2. `builder.Services.AddSingleton<BookService>()` đăng ký BookService như singleton (một instance trong toàn bộ lifetime ứng dụng).
3. `builder.Services.AddControllersWithViews()` đăng ký MVC controllers + views.
4. Khi app chạy, `app.MapControllerRoute(...)` thiết lập routing mặc định: `{controller=Home}/{action=Index}/{id?}`.
5. Kết quả: các request đến `/Book/Index`, `/Book/Create`, `/Book/Detail/1` sẽ được chuyển đến `BookController` tương ứng.

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
   - Gọi `_bookService.GetById(id)`; nếu trả về null => NotFound(); ngược lại View(book).
   - URL ví dụ: `/Book/Detail/1`.

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

## Ghi chú & điểm cần lưu ý

- BookService là singleton: đảm bảo dùng lock để tránh race khi nhiều request cùng thao tác ghi file.
- Id tự gán trong Add(): nếu file bị sửa tay và có Id không liên tiếp thì method vẫn gán Id = Max(Id)+1.
- Validation thực hiện ở hai tầng: client (unobtrusive) và server (ModelState). Luôn kiểm tra ModelState trên server dù có client validation.
- File Data/books.json có thể bị lock/không ghi được nếu environment hoặc quyền file hệ thống giới hạn; BookService hiện không có xử lý lỗi IO ngoại lệ chi tiết.

---

Nếu bạn muốn tôi chuyển ngôn ngữ giải thích sang tiếng Anh hoặc rút gọn/độ sâu hơn cho từng file (ví dụ giải thích code cụ thể theo dòng trong BookService.cs), hãy cho biết yêu cầu cụ thể.