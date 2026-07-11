# Fix chữ ký tài xế khi gửi yêu cầu đăng ký

## Nguyên nhân
Component SignaturePad chỉ nghe PointerEvent. Một số trình duyệt/WebView mobile không phát PointerEvent nên canvas hiển thị nhưng không thể vẽ.

## Thay đổi
- Hỗ trợ PointerEvent cho trình duyệt mới.
- Fallback sang TouchEvent và MouseEvent cho WebView/iOS cũ.
- Chờ hai frame trước khi đo kích thước canvas ở bước 4.
- Xử lý ResizeObserver không tồn tại.
- Theo dõi xoay màn hình và resize.
- Thêm @key riêng cho SignaturePad tại trang đăng ký.
- Cache-bust js/signature-pad.js trong App.razor.

## Sau khi deploy
Khởi động lại service và hard refresh trình duyệt. URL script mới có `?v=20260711-2` nên trình duyệt sẽ lấy bản JavaScript mới.
