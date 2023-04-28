# Project 1: MyShop

# A. Mô tả
Thành viên: 
- 19127363 - Lê Văn Đông    
- 20127004 - Huỳnh Minh Bảo
- 20127600 - Lưu Tuấn Quân

<br> Chương trình MyShop quản lí việc bán hàng cho một cửa hàng bán sách.

# B. Các chức năng đã hoàn thành

## Chức năng cơ sở (7 điểm)

Cơ sở dữ liệu sử dụng: SQL Server theo RestAPI

### 1. Màn hình đăng nhập -> <b>Huỳnh Minh Bảo</b>
-   Cho nhập username và password để đi vào màn hình chính. 
-   Có chức năng lưu username và password ở local để lần sau người dùng không cần đăng nhập lại. Password cần được mã hóa.

### 2. Màn hình dashboard -> <b>Lưu Tuấn Quân</b>
Cung cấp tổng quan về hệ thống, ví dụ:
-   Có tổng cộng bao nhiêu sản phẩm đang bán
-   Có tổng cộng bao nhiêu đơn hàng mới trong tuần / tháng
-   Liệt kê top 5 sản phẩm đang sắp hết hàng (số lượng < 5)

### 3. Quản lí hàng hóa -> <b>Huỳnh Minh Bảo</b>
-   Import dữ liệu gốc ban đầu (loại sản phẩm, danh sách các sản phẩm) từ tập tin Excel hoặc Access.
-   Xem danh sách các sản phẩm theo loại sản phẩm có phân trang.
-   Cho phép thêm một loại sản phẩm, hiệu chỉnh loại sản phẩm
-   Cho phép thêm một sản phẩm, xóa một sản phẩm,
-   Hiệu chỉnh thông tin sản  phẩm
-   Cho phép tìm kiếm sản phẩm theo tên
-   Cho phép lọc lại sản phẩm theo khoảng giá

### 4. Quản lí các dơn hàng -> <b>Lưu Tuấn Quân</b>
-   Tạo ra các đơn hàng
-   Cho phép xóa một đơn hàng
-   Cho phép xem danh sách các đơn hàng có phân trang
-   Xem chi tiết một đơn hàng
-   Tìm kiếm các đơn hàng từ ngày đến ngày

### 5. Báo cáo thống kê -> <b>Lưu Tuấn Quân</b>
-   Báo cáo doanh thu và lợi nhuận theo ngày đến ngày, theo tuần, theo tháng, theo năm (vẽ biểu đồ)
-   Xem các sản phẩm và số lượng bán theo ngày đến ngày, theo tuần, theo tháng, theo năm (vẽ biểu đồ)

### 6. Cấu hình -> <b>Huỳnh Minh Bảo</b>
-   Cho phép hiệu chỉnh số lượng sản phẩm mỗi trang
-   Cho phép khi chạy chương trình lên thì mở lại màn hình cuối mà lần trước tắt 

### 7. Đóng gói thành file cài đặt -> <b>Huỳnh Minh Bảo</b>
-   Cần đóng gói thành file exe để tự cài chương trình vào hệ thống 

## Các chức năng gợi ý nâng cao (Tự chọn để được 3 điểm)
-   <b>Sử dụng mô hình MVVM cho tất cả màn hình (1 điểm) -> Lưu Tuấn Quân
-   <b>Kết nối API Rest API, lưu ảnh lên Imgur, tạo các API để truy xuất dữ liệu (2 điểm) -> Lê Văn Đông

## Chức năng chưa làm:
-   Xóa một loại sản phẩm
-   Cập nhật một đơn hàng

# C. Điểm đề nghị
- 19127363 - Lê Văn Đông    - 9
- 20127004 - Huỳnh Minh Bảo - 10
- 20127600 - Lưu Tuấn Quân  - 10