# FIX: Gán xe và tài xế theo quan hệ 1-1

## Quy tắc

- Một xe chỉ có tối đa một tài xế đang được gán.
- Một tài xế chỉ được gán cho tối đa một xe chưa xóa.
- Muốn chuyển tài xế sang xe khác, Owner phải bỏ gán ở xe cũ trước.

## Các vị trí đã khóa

1. Trang thêm/cập nhật xe: chỉ hiển thị tài xế chưa có xe; kiểm tra lại ngay trước khi lưu.
2. Trang tạo/cập nhật hợp đồng Owner/Admin: xe chưa có tài xế chỉ cho chọn tài xế chưa được gán xe khác.
3. `ContractService`: chặn request trực tiếp cố gán một tài xế sang xe thứ hai.
4. Database: unique filtered index `UX_Vehicles_AssignedDriverId` bảo vệ trường hợp ghi đồng thời.

## Xử lý dữ liệu cũ khi khởi động

Nếu dữ liệu cũ có một tài xế bị gán cho nhiều xe, hệ thống giữ lại một xe theo thứ tự:

1. Xe đang hoạt động.
2. Xe được cập nhật/tạo gần nhất.
3. Các xe còn lại được đặt `AssignedDriverId = NULL`.

## Kiểm thử nhanh

1. Gán tài xế A cho xe 01 và lưu thành công.
2. Mở xe 02: tài xế A không xuất hiện trong danh sách chọn.
3. Bỏ gán tài xế A khỏi xe 01; mở lại xe 02: tài xế A xuất hiện và có thể gán.
4. Tạo hợp đồng với xe chưa gán: danh sách tài xế không hiển thị tài xế đã có xe.
5. Thử gửi request trực tiếp gán tài xế đã có xe: service phải trả thông báo chặn.
