# Cập nhật luồng phát hợp đồng xuống tài xế

## Luồng mới

1. Owner/Admin tạo hoặc cập nhật hợp đồng và phát xuống tài xế.
2. Trên hợp đồng đã tồn tại, tài xế chỉ được sửa:
   - Thông tin khách hàng.
   - Danh sách hành khách.
3. Tài xế bấm **Lưu thông tin khách hàng** để chuyển hợp đồng sang trạng thái `WaitingCustomerSignature`.
4. Khách hàng ký tại khung **Khách hàng (Người thuê xe)**.
5. Khi chữ ký được lưu thành công, hợp đồng chuyển thẳng sang `Completed` và bị khóa.

## Các sửa lỗi chính

- Khóa cả giao diện và backend đối với các trường xe, tài xế, loại hợp đồng, lộ trình, thời gian, giá trị và thanh toán khi tài xế mở hợp đồng đã tạo/phát xuống.
- Backend chỉ nhận cập nhật khách hàng và danh sách hành khách từ tài khoản Driver.
- Kiểm tra đúng tài xế của hợp đồng trước khi lưu chữ ký khách hàng.
- Không để các chữ ký cũ của vai trò khác khóa nhầm khung ký khách hàng.
- Giữ nguyên nét ký trên màn hình khi lưu server thất bại để có thể bấm xác nhận lại.
- Lưu đúng phần mở rộng `.png` hoặc `.jpg` theo dữ liệu ảnh thực tế.
- Cập nhật trạng thái `Completed` ngay trong cùng transaction sau khi lưu chữ ký.
- Tài xế không được hủy hợp đồng do Owner/Admin phát xuống.

## File đã thay đổi

- `src/HTX586CONTRACT.Application/Abstractions/IContractDocumentService.cs`
- `src/HTX586CONTRACT.Infrastructure/Services/ContractService.cs`
- `src/HTX586CONTRACT.Web/Components/App.razor`
- `src/HTX586CONTRACT.Web/Components/Pages/Driver/Contracts/Create.razor`
- `src/HTX586CONTRACT.Web/Components/Shared/SignaturePad.razor`
- `src/HTX586CONTRACT.Web/Services/ContractDocumentService.cs`

## Kiểm thử đề nghị

1. Owner/Admin phát một hợp đồng mới xuống tài xế.
2. Đăng nhập Driver và mở hợp đồng.
3. Xác nhận chỉ thông tin khách hàng và danh sách hành khách có thể chỉnh sửa.
4. Nhập tên, số điện thoại, danh sách hành khách rồi bấm **Lưu thông tin khách hàng**.
5. Vẽ chữ ký khách hàng và bấm **Xác nhận chữ ký**.
6. Xác nhận trạng thái chuyển thành **Hoàn tất** và nút tạo/mở PDF được bật.

Không có thay đổi schema database, không cần migration mới.
