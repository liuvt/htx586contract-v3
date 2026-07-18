# Checklist kiểm thử báo cáo Excel V6

## 1. Quyền Owner

- Đăng nhập Owner, mở `/owner/reports`.
- Xuất Hợp đồng: file có sheet `Tổng hợp`, `Chi tiết`; dữ liệu gồm tất cả Công ty/Văn phòng.
- Xuất Tài xế: file có sheet `Tổng hợp`, `Chi tiết`; dữ liệu gồm tất cả Công ty/Văn phòng.
- Xuất Xe và chủ sở hữu: file có sheet `Tổng hợp`, `Chi tiết`; dữ liệu gồm tất cả Công ty/Văn phòng.
- Chọn cùng một ngày và xuất doanh thu: file có 5 sheet, không báo lỗi.
- Chọn khoảng thời gian dưới hoặc bằng 01 năm: xuất thành công.
- Chọn khoảng thời gian vượt 01 năm: nút bị khóa và endpoint trả lỗi nếu gọi trực tiếp.

## 2. Quyền Admin

- Admin đã gán CompanyProfile: trang hiển thị đúng tên Công ty/Văn phòng.
- Các file chỉ chứa dữ liệu có `CompanyProfileId` đúng với Admin.
- Admin chưa gán CompanyProfile: toàn bộ nút xuất bị khóa.
- Gọi trực tiếp `/api/reports/export/...` bằng Admin chưa gán đơn vị phải bị từ chối.
- Thử sửa query string hoặc URL không làm thay đổi phạm vi công ty của Admin.

## 3. Doanh thu

- Chỉ hợp đồng `Completed` được tính.
- Ngày doanh thu dùng `StartTime`; nếu trống dùng `CreatedAt`.
- Tổng doanh thu trên `Tổng quan` khớp tổng sheet `Chi tiết HĐ`.
- Tổng theo ngày/tháng/năm khớp tổng quan.
- Hợp đồng ở ngày kết thúc vẫn được đưa vào báo cáo.
- Hợp đồng ngoài khoảng chọn không xuất hiện.

## 4. File Excel

- Mở được bằng Microsoft Excel và Google Sheets.
- Header có bộ lọc, cố định hàng tiêu đề.
- Cột ngày hiển thị `dd/MM/yyyy` hoặc `dd/MM/yyyy HH:mm`.
- Cột tiền hiển thị phân cách hàng nghìn.
- Tên file có phạm vi và thời gian xuất.
