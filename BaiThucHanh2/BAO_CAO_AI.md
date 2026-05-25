# Báo cáo sử dụng AI hỗ trợ học tập

| Nội dung | Yêu cầu |
| :--- | :--- |
| **Prompt đã dùng** | "giúp tôi hoàn thành các bài tập trong ảnh thật chính xác và đúng yêu cầu bài toán" (kèm ảnh Bài 1, 2, 3 và mẫu báo cáo AI) |
| **AI hỗ trợ gì** | Phân tích yêu cầu từng bài; tạo/sửa `HomeController`, `ProductController`, `StudentController`; tạo model `StudentInfo` và View `Info.cshtml`; gợi ý xử lý lỗi khi thiếu tham số (Bài 2 nâng cao) |
| **Đã chỉnh sửa gì** | Đã cập nhật tên **Nguyễn Thái Hà** và email **haahaa05022006@gmail.com** trong `HomeController` (Bài 1). Bài 3 giữ dữ liệu mẫu theo đề (`Nguyễn Văn A`, tuổi 20, ngành CNTT) |
| **Kiểm chứng code** | Chạy `dotnet run` trong thư mục project; mở trình duyệt và kiểm tra các URL bên dưới |

## URL kiểm tra

| Bài | URL | Kết quả mong đợi |
| :--- | :--- | :--- |
| 1 | `/Home/Index` | `Welcome to ASP.NET MVC` |
| 1 | `/Home/About` | Tên sinh viên |
| 1 | `/Home/Contact` | Email sinh viên |
| 2 | `/Product/Detail/5` | `Product ID = 5` |
| 2 | `/Product/Category?name=Laptop` | `Category = Laptop` |
| 2 | `/Product/Detail` hoặc `/Product/Category` | Thông báo lỗi thiếu tham số |
| 3 | `/Student/Info` | Tên, Tuổi, Ngành đúng định dạng |
