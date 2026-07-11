# Fix xác nhận chữ ký tài xế tự đăng ký

## Nguyên nhân
Canvas trên điện thoại được tạo theo `devicePixelRatio`. Khi ký tay, `toDataURL("image/png")` xuất toàn bộ canvas độ phân giải cao và truyền chuỗi Base64 về Blazor Server qua SignalR. Dữ liệu có thể vượt giới hạn message mặc định, nên thao tác **Xác nhận chữ ký** thất bại. Upload ảnh vẫn hoạt động vì file được đọc ở phía .NET qua `InputFile`, không đi theo cùng luồng JS interop.

## Thay đổi
- Cắt bỏ vùng canvas trống quanh nét ký.
- Thu nhỏ tối đa 720 x 260 px.
- Xuất JPEG nền trắng chất lượng 0.82 để giảm kích thước.
- Tăng `MaximumReceiveMessageSize` của Blazor Server lên 256 KB làm dự phòng.
- Bổ sung thông báo khi JS interop lỗi.
- Đổi cache version của `signature-pad.js` sang `20260711-3`.
