# Cập nhật luồng Driver

## 1. Đăng ký và đăng nhập lần đầu

- Tài xế tự đăng ký phải tự đặt mật khẩu và tạo chữ ký cố định khi gửi yêu cầu.
- Sau khi Owner/Admin duyệt, tài khoản được kích hoạt và giữ nguyên mật khẩu/chữ ký đã đăng ký; không bị ép đổi mật khẩu hoặc ký lại.
- Tài khoản do Owner/Admin tạo luôn được đánh dấu bắt buộc đổi mật khẩu.
- Sau khi đổi mật khẩu và đăng nhập lại, tài xế phải tạo chữ ký cố định lần đầu nếu tài khoản chưa có chữ ký.
- Màn hình tạo tài xế của Owner/Admin không còn cho quản trị viên ký thay tài xế.

## 2. Giao diện Driver

Menu Driver gồm:

- Tổng quan & doanh thu
- Hợp đồng của tôi
- Tạo hợp đồng
- Thông báo
- Hồ sơ cá nhân

Màn hình tổng quan có:

- Chọn tháng thống kê.
- Tổng doanh thu hợp đồng hoàn tất trong tháng.
- Bảng doanh thu theo từng ngày.
- Số hợp đồng hoàn tất/đang xử lý.
- Danh sách phương tiện đang được gán.
- Thông báo mới và số thông báo chưa đọc.

Màn hình hồ sơ cá nhân:

- Chỉ xem thông tin đã đăng ký/được cấp.
- Xem chữ ký cố định và phương tiện đang được giao.
- Có nút đổi mật khẩu.
- Driver không tự sửa hồ sơ hoặc thay chữ ký cố định sau khi đăng ký/khởi tạo lần đầu.

## 3. Quyền hợp đồng của Driver

- Chỉ được tự tạo hợp đồng khi có ít nhất một phương tiện đang hoạt động được gán cho tài khoản.
- Bố cục tạo/xử lý và xem chi tiết hợp đồng được chia giống Owner/Admin:
  1. Thông tin hợp đồng.
  2. Phương tiện.
  3. Khách hàng/người thuê.
  4. Nội dung hợp đồng.
  5. Danh sách hành khách.
- Thông tin hợp đồng và phương tiện luôn lấy tự động, Driver không được thay đổi.
- Hợp đồng Owner/Admin phát xuống:
  - Khách hàng/người thuê do Owner/Admin chọn và bị khóa đối với Driver.
  - Owner/Admin phải nhập khách hàng hợp lệ trước khi phát hợp đồng.
- Hợp đồng Driver tự tạo:
  - Driver được chọn khách đã dùng trước đó hoặc nhập khách hàng mới ngay khi tạo hợp đồng.
  - Khách hàng mới lưu `CreatedByDriverId`, `CreatedBy` bằng ID tài khoản Driver tạo.
  - Sau khi hợp đồng được lưu, Driver không được sửa hồ sơ khách hàng; chỉ Owner/Admin được quản lý tại màn hình khách hàng quản trị.
  - Trang quản lý khách hàng riêng của Driver đã được loại bỏ.
- Trước khi khách ký, Driver được cập nhật nhóm Nội dung hợp đồng gồm ngày/giờ, điểm đi, điểm đến, lộ trình, tổng km, giá trị, hình thức/thời gian thanh toán và ghi chú.
- Driver được thêm, sửa, xóa Danh sách hành khách; số lượng khách tự tính theo danh sách.
- Service kiểm tra lại quyền phía máy chủ; request Driver cố đổi loại hợp đồng, số hợp đồng, phương tiện hoặc khách hàng sẽ không được chấp nhận.
- Sau khi khách hàng ký, hợp đồng chuyển sang Hoàn tất và toàn bộ nội dung bị khóa.
- Hợp đồng do Owner/Admin phát xuống không cho Driver tự hủy.

## 4. Thông báo Driver

Hệ thống tạo thông báo khi:

- Owner/Admin tạo hoặc cập nhật/phát hợp đồng cho Driver.
- Owner gán hoặc đổi phương tiện cho Driver.
- Xe chưa có tài xế được chọn trên hợp đồng và hệ thống tự động gán xe cho Driver.

Dữ liệu được lưu trong bảng `DriverNotifications`. Khi ứng dụng khởi động, `DatabaseSeeder` tự tạo bảng này cho database SQL Server cũ nếu chưa tồn tại.

## 5. Kịch bản kiểm thử đề xuất

1. Tự đăng ký tài xế, ký tên, chờ duyệt; sau duyệt đăng nhập trực tiếp và xác nhận không xuất hiện yêu cầu đổi mật khẩu/ký lại.
2. Owner/Admin tạo tài khoản mới; đăng nhập lần đầu phải đổi mật khẩu, đăng nhập lại phải tạo chữ ký.
3. Tài xế chưa có xe: nút tạo hợp đồng bị khóa và truy cập trang tạo hiển thị cảnh báo.
4. Gán xe cho tài xế: kiểm tra xe xuất hiện ở Dashboard/Hồ sơ và có thông báo mới.
5. Owner/Admin phát hợp đồng: Driver nhận thông báo và mở được hợp đồng.
6. Owner/Admin tạo hợp đồng không có khách hàng: service phải từ chối; chọn/nhập khách hợp lệ rồi phát xuống.
7. Driver mở hợp đồng được phát xuống: thông tin hợp đồng, phương tiện và khách hàng phải khóa; thử sửa request trực tiếp phải bị service từ chối.
8. Driver tự tạo hợp đồng: chọn khách cũ hoặc nhập khách mới; kiểm tra khách mới ghi nhận đúng ID Driver tạo.
9. Sau khi lưu hợp đồng tự tạo, Driver không còn quyền sửa khách hàng; Owner/Admin vẫn quản lý được tại `/admin/customers`.
10. Driver cập nhật ngày giờ, lộ trình, tổng km, thanh toán, ghi chú và danh sách hành khách; lưu thành công.
11. Khách ký: hợp đồng chuyển Hoàn tất, doanh thu xuất hiện đúng ngày/tháng và hợp đồng không còn chỉnh sửa được.

## Bổ sung Fix 1–4 (phân trang, reset mật khẩu, chữ ký xem trước, trạng thái HĐ)

### Fix 1 – Phân trang danh sách
- Bổ sung `MudTablePager` với lựa chọn 10/20/50/100 dòng cho:
  - Xe và chủ sở hữu.
  - Tài xế.
  - Yêu cầu đăng ký tài xế.
  - Khách hàng.
  - Hợp đồng Owner/Admin và danh sách hợp đồng Driver.
- Danh sách tài xế dùng `PageSize = 0` để lấy toàn bộ dữ liệu trước khi phân trang trên giao diện, không còn bị chặn ở 100/200 bản ghi.
- Admin được xem danh sách và chi tiết xe thuộc đúng CompanyProfile được gán; chỉ Owner được thêm/cập nhật xe.

### Fix 2 – Đặt lại mật khẩu tài xế
- Trang chi tiết tài xế có nút `Đặt lại mật khẩu` cho Owner/Admin.
- Yêu cầu nhập mật khẩu tạm và xác nhận lại.
- Sau khi reset, `MustChangePassword = true`; tài xế bắt buộc đổi mật khẩu ở lần đăng nhập tiếp theo.

### Fix 3 – Chữ ký tài xế không còn mất sau khi ký
- Luồng đăng ký tài khoản và đăng nhập lần đầu tách thành hai bước:
  1. `Lưu chữ ký tạm` để hiển thị lại chữ ký cho tài xế kiểm tra.
  2. `Xác nhận...` mới lưu thật vào hệ thống.
- Sau khi lưu tạm, chữ ký được hiển thị ngay và có nút `Xóa và ký lại`.
- Chữ ký tạm không tạo file trên máy chủ cho đến khi người dùng xác nhận bước cuối.

### Fix 4 – Làm nổi bật trạng thái hợp đồng
- Tạo component dùng chung `ContractStatusChip`.
- Chuẩn hóa các trạng thái chính:
  - `Phát xuống cho tài xế`: màu Primary, dạng Filled, chữ đậm.
  - `Chờ khách ký`: màu Warning, dạng Filled, chữ đậm.
  - `Hoàn tất`: màu Success, dạng Filled, chữ đậm.
- Áp dụng trên danh sách và trang chi tiết/cập nhật hợp đồng của Owner, Admin và Driver.
