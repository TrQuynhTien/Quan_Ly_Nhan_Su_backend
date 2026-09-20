# Hệ thống Quản lý Nhân sự

Đồ án tốt nghiệp **Xây dựng hệ thống quản lý nhân sự thông minh trên nền tảng Web sử dụng ASP.NET Core Web API và Angular**.

## Thành viên thực hiện

- Trần Quỳnh Tiên – 24410360
- Châu Ngọc Phát – 25410274

## Công nghệ sử dụng

### Backend
- ASP.NET Core Web API
- C#
- Entity Framework Core
- Microsoft SQL Server
- JWT Authentication
- BCrypt
- ClosedXML
- Gemini API

### Frontend
- Angular 18
- TypeScript

## Chức năng chính

- Quản lý tài khoản và phân quyền
- Quản lý phòng ban, chức vụ và trình độ
- Quản lý nhân viên
- Quản lý hợp đồng và gia hạn hợp đồng
- Quản lý chấm công
- Quản lý nghỉ phép và thông báo
- Quản lý phụ cấp
- Quản lý khen thưởng – kỷ luật
- Tính lương
- Thống kê và báo cáo
- Import dữ liệu nhân viên từ Excel
- Hỗ trợ tra cứu thông tin bằng Gemini AI

## Hệ thống đã triển khai

Frontend:  
https://hrm-enterprise-steel.vercel.app

Backend API:  
https://qlns-uit.runasp.net

## Mã nguồn

Frontend:  
https://github.com/ngocphat922004/HRM-Enterprise-FE

Backend:  
https://github.com/TrQuynhTien/Quan_Ly_Nhan_Su_backend

## Cấu hình Backend

Các thông tin nhạy cảm không được lưu trực tiếp trong mã nguồn.

Các biến môi trường cần thiết:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `GEMINI_API_KEY`

## Ghi chú

Frontend và Backend được quản lý tại hai repository riêng để thuận tiện trong quá trình phát triển.

Sau khi triển khai, hai thành phần được tích hợp và kiểm thử trực tiếp trên môi trường deployment.
