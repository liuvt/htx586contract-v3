# Sửa lỗi Driver ký khách hàng nhưng không lưu được

## Lỗi ghi nhận

Khi Driver bấm **Xác nhận chữ ký**, giao diện tự lưu thông tin khách hàng và danh sách hành khách trước khi lưu chữ ký. Luồng cũ tải `Contract` và `ContractPassengers` dưới dạng entity có `RowVersion`, sau đó gọi `RemoveRange(...)` và `SaveChangesAsync(...)`.

Nếu một bản ghi hành khách/khách hàng/hợp đồng đã đổi `RowVersion` sau lúc được đọc, EF Core phát sinh:

`The database operation was expected to affect 1 row(s), but actually affected 0 row(s)`

Vì lỗi xảy ra ở bước tự lưu nội dung trước chữ ký nên ảnh chữ ký chưa được ghi vào SQL và hợp đồng chưa chuyển sang `Completed`.

## Thay đổi

File chính:

`src/HTX586CONTRACT.Infrastructure/Services/ContractService.cs`

Luồng cập nhật của Driver được tách riêng và thực hiện trong transaction `Serializable`:

1. Khóa hàng hợp đồng bằng `UPDLOCK, HOLDLOCK`.
2. Kiểm tra đúng tài khoản Driver, đúng tài xế được gán, xe còn được gán và hợp đồng chưa ký.
3. Cập nhật khách hàng bằng `ExecuteUpdateAsync`, không dùng `RowVersion` cũ.
4. Xóa danh sách hành khách bằng `ExecuteDeleteAsync` theo `ContractId`, không xóa từng entity có `RowVersion`.
5. Thêm lại danh sách hành khách và audit log.
6. Cập nhật hợp đồng sang `WaitingCustomerSignature` bằng `ExecuteUpdateAsync`.
7. Sau khi transaction này hoàn tất, luồng `SaveSignatureAsync` mới lưu chữ ký và chuyển hợp đồng sang `Completed`.

Không thay đổi cấu trúc database và không cần migration.

## Kiểm thử

1. Owner/Admin tạo và phát hợp đồng xuống Driver.
2. Driver mở hợp đồng, nhập khách hàng và danh sách hành khách.
3. Không cần bấm Lưu lại trước; cho khách ký trực tiếp.
4. Bấm **Xác nhận chữ ký** một lần.
5. Kết quả mong đợi:
   - khách hàng và hành khách được lưu;
   - có một bản ghi `ContractSignatures` với `Party = Customer`;
   - `Contracts.Status = Completed`;
   - giao diện khóa nội dung và cho phép tạo/mở PDF.
