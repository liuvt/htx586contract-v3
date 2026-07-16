# Fix luồng Driver ký chân ký khách hàng

## Quy tắc sau khi sửa

- Chỉ tài khoản có role `Driver`, đang hoạt động và đúng `DriverId` của hợp đồng được lưu chữ ký khách hàng.
- Owner/Admin chỉ xem chữ ký đã có; không hiển thị canvas/upload trống để tránh hiểu nhầm có thể ký.
- Trên trang Driver, chân ký khách hàng được sử dụng khi hợp đồng ở `Draft` hoặc `WaitingCustomerSignature`.
- Khi Driver bấm **Xác nhận chữ ký**:
  1. Hệ thống kiểm tra và tự lưu thông tin khách hàng cùng danh sách hành khách nếu có thay đổi.
  2. Hợp đồng chuyển sang `WaitingCustomerSignature`.
  3. Lưu chữ ký khách hàng.
  4. Chuyển hợp đồng sang `Completed` và khóa toàn bộ nội dung.
- Nếu lưu thông tin hoặc lưu chữ ký thất bại, nét ký vẫn được giữ trên canvas để thử lại.
- Không thay đổi database schema, không cần migration.

## File thay đổi

- `src/HTX586CONTRACT.Web/Components/Pages/Driver/Contracts/Create.razor`
- `src/HTX586CONTRACT.Web/Components/Pages/Admin/ContractRecords/Edit.razor`
- `src/HTX586CONTRACT.Web/Components/Pages/Owner/ContractRecords/Edit.razor`
- `src/HTX586CONTRACT.Web/Services/ContractDocumentService.cs`

## Kiểm thử đề nghị

1. Đăng nhập Driver đúng hợp đồng, nhập/sửa thông tin khách hàng rồi ký mà không cần bấm Lưu trước.
2. Kiểm tra hợp đồng chuyển thẳng sang `Completed`.
3. Đăng nhập Owner/Admin: chỉ xem trạng thái/chữ ký, không có canvas hoặc upload để ký khách hàng.
4. Thử gọi lưu chữ ký bằng tài khoản không có role Driver: backend phải từ chối.
5. Thử Driver khác hợp đồng: backend phải từ chối.
