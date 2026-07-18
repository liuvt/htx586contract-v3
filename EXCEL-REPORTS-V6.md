# Báo cáo và xuất Excel V6

## Trang sử dụng

- Owner: `/owner/reports`
- Admin: `/admin/reports`

## Phạm vi quyền

- Owner xuất toàn bộ dữ liệu của tất cả Công ty/Văn phòng.
- Admin chỉ xuất dữ liệu thuộc `CompanyProfileId` đang gán cho tài khoản Admin.
- Phạm vi được kiểm tra lại tại endpoint và `ExcelReportService`, không chỉ khóa trên giao diện.

## File danh mục

1. Hợp đồng: `Tổng hợp`, `Chi tiết`.
2. Tài xế: `Tổng hợp`, `Chi tiết`.
3. Xe và chủ sở hữu: `Tổng hợp`, `Chi tiết`.

## Báo cáo doanh thu

- Chọn cùng một ngày để xuất tối thiểu 01 ngày.
- Ngày kết thúc không được trước ngày bắt đầu.
- Khoảng thời gian tối đa 01 năm, tính đến ngày liền trước cùng ngày của năm sau.
- Chỉ tính hợp đồng `Completed`.
- Ngày doanh thu: `StartTime`, nếu không có thì dùng `CreatedAt`, đồng nhất logic Dashboard hiện tại.
- File gồm: `Tổng quan`, `Theo ngày`, `Theo tháng`, `Theo năm`, `Chi tiết HĐ`.

## Thư viện

- ClosedXML 0.105.0 được dùng để tạo `.xlsx` trực tiếp trên server, không yêu cầu Microsoft Excel hoặc LibreOffice.

## Mẫu tham chiếu

- `src/HTX586CONTRACT.Web/Templates/Reports/MauBaoCaoExcel_HTX586.xlsx`
