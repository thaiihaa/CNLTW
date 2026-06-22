# HƯỚNG DẪN THUYẾT TRÌNH - STUDENT MANAGEMENT

## Tổng quan
Đây là dự án Spring Boot cơ bản quản lý thông tin sinh viên, sử dụng các công nghệ:
- Spring Web
- Thymeleaf
- Spring Boot DevTools

---

## Bài 1: Tạo và chạy project
- Đã tạo project Spring Boot với tên "StudentManagement" (thư mục hiện tại)
- Đã thêm các dependency cần thiết vào `pom.xml`
- Chạy project thành công tại `http://localhost:8080`

---

## Bài 2: Tạo HomeController

### 2.1 Đường dẫn `/home/index`
- **URL**: `http://localhost:8080/home/index`
- **Nội dung hiển thị**: "Welcome to Spring Boot"
- **File code**: `src/main/java/com/example/demo/controller/HomeController.java`
- **Dòng code liên quan**:
  - Dòng 8: `@Controller` → Đánh dấu class là Controller
  - Dòng 9: `@RequestMapping("/home")` → Tiền tố cho tất cả endpoint trong class này
  - Dòng 12: `@GetMapping("/index")` → Xử lý request GET đến `/home/index`
  - Dòng 13: `@ResponseBody` → Trả về nội dung trực tiếp dưới dạng text
  - Dòng 14-16: Phương thức `index()` trả về chuỗi "Welcome to Spring Boot"

### 2.2 Đường dẫn `/home/about`
- **URL**: `http://localhost:8080/home/about`
- **Nội dung hiển thị**: "NGUYỄN THÁI HÀ"
- **File code**: `src/main/java/com/example/demo/controller/HomeController.java`
- **Dòng code liên quan**:
  - Dòng 18: `@GetMapping("/about")` → Xử lý request GET đến `/home/about`
  - Dòng 19: `@ResponseBody`
  - Dòng 20-22: Phương thức `about()` trả về tên sinh viên

### 2.3 Đường dẫn `/home/contact`
- **URL**: `http://localhost:8080/home/contact`
- **Nội dung hiển thị**: "BIT242377@st.cmcu.edu.vn"
- **File code**: `src/main/java/com/example/demo/controller/HomeController.java`
- **Dòng code liên quan**:
  - Dòng 24: `@GetMapping("/contact")` → Xử lý request GET đến `/home/contact`
  - Dòng 25: `@ResponseBody`
  - Dòng 26-28: Phương thức `contact()` trả về email sinh viên

---

## Bài 3: Nhận tham số từ URL - ProductController

### 3.1 Đường dẫn `/product/detail/{id}` (dùng @PathVariable)
- **URL ví dụ**: `http://localhost:8080/product/detail/5`
- **Nội dung hiển thị**: "Product ID = 5"
- **File code**: `src/main/java/com/example/demo/controller/ProductController.java`
- **Dòng code liên quan**:
  - Dòng 8: `@Controller`
  - Dòng 10: `@GetMapping("/product/detail/{id}")` → Định nghĩa đường dẫn với biến {id}
  - Dòng 11: `@ResponseBody`
  - Dòng 12: `@PathVariable("id") Integer id` → Lấy giá trị từ URL gán vào biến `id`
  - Dòng 13-15: Kiểm tra tính hợp lệ của ID
  - Dòng 16: Trả về kết quả

### 3.2 Đường dẫn `/product/category` (dùng @RequestParam)
- **URL ví dụ**: `http://localhost:8080/product/category?name=Laptop`
- **Nội dung hiển thị**: "Category = Laptop"
- **File code**: `src/main/java/com/example/demo/controller/ProductController.java`
- **Dòng code liên quan**:
  - Dòng 20: `@GetMapping("/product/category")`
  - Dòng 21: `@ResponseBody`
  - Dòng 22: `@RequestParam(name = "name", required = false) String name` → Lấy tham số `name` từ query string
  - Dòng 23-25: Kiểm tra tính hợp lệ
  - Dòng 26: Trả về kết quả

---

## Bài 4: Truyền dữ liệu sang View - StudentController

### 4.1 Class Student (Model)
- **File code**: `src/main/java/com/example/demo/model/Student.java`
- **Mô tả**: Lưu thông tin sinh viên gồm 3 thuộc tính:
  - Dòng 5-7: Thuộc tính `name`, `age`, `major`
  - Dòng 9-13: Constructor khởi tạo
  - Dòng 15-31: Getter và Setter cho các thuộc tính

### 4.2 Đường dẫn `/student/info`
- **URL**: `http://localhost:8080/student/info`
- **Nội dung hiển thị**: Thông tin sinh viên trên giao diện HTML
- **File code Controller**: `src/main/java/com/example/demo/controller/StudentController.java`
- **Dòng code liên quan**:
  - Dòng 9: `@Controller`
  - Dòng 11: `@GetMapping("/student/info")` → Xử lý request GET
  - Dòng 12: `Model model` → Đối tượng Model dùng để truyền dữ liệu sang View
  - Dòng 13: Tạo đối tượng Student với thông tin "Nguyễn Văn A", 20 tuổi, ngành "Công nghệ thông tin"
  - Dòng 14: `model.addAttribute("student", student)` → Thêm đối tượng student vào model với key "student"
  - Dòng 15: `return "student/info"` → Trả về tên view (tương ứng với file templates/student/info.html)

### 4.3 File View Thymeleaf
- **File code**: `src/main/resources/templates/student/info.html`
- **Dòng code liên quan**:
  - Dòng 2: `xmlns:th="http://www.thymeleaf.org"` → Khai báo namespace Thymeleaf
  - Dòng 8: `th:text="${student.name}"` → Lấy giá trị thuộc tính `name` của đối tượng `student`
  - Dòng 9: `th:text="${student.age}"` → Lấy giá trị thuộc tính `age`
  - Dòng 10: `th:text="${student.major}"` → Lấy giá trị thuộc tính `major`

---

## Cách chạy dự án
1. Mở terminal, di chuyển đến thư mục dự án
2. Chạy lệnh: `mvnw.cmd spring-boot:run`
3. Mở trình duyệt và truy cập các đường dẫn trên
