# Fix PDF layout và giới hạn dữ liệu - v7

## File đã thay

- `src/HTX586CONTRACT.Web/Templates/Contracts/HopDongVanChuyenHanhKhach.template.pdf`
- `src/HTX586CONTRACT.Web/Templates/Contracts/HopDongVanChuyenHanhKhach.layout.json`
- `src/HTX586CONTRACT.Web/Services/PdfContractTemplateRenderer.cs`

## Nội dung

- Căn lại toàn bộ vùng dữ liệu theo PDF template ngày 16/07/2026.
- Sửa vùng sức chứa, số lượng khách, tên khách hàng, MST, CCCD, ngày cấp, lộ trình và chữ ký.
- Trang 2 dùng đúng biên 20 dòng của bảng hành khách.
- Giờ hợp đồng xuất theo `HH:mm`.
- Sửa thuật toán tự co chữ và xuống dòng để không làm mất phần nội dung còn lại.
- Không thay đổi database schema, không cần migration.

Xem `PDF_FIELD_LIMITS_v7.md` để áp dụng `MaxLength` trên giao diện.
