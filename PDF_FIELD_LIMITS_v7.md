# Giới hạn dữ liệu PDF hợp đồng hành khách - Layout v7

Áp dụng cho:

- `HopDongVanChuyenHanhKhach.template.pdf` bản ngày 16/07/2026.
- `HopDongVanChuyenHanhKhach.layout.fixed-v7.json`.
- Font dữ liệu mặc định 11 pt, tự giảm đến `minFontSize` 7-9 pt tùy trường.

## Nguyên tắc

1. Giới hạn trong database không phải là giới hạn hiển thị PDF. Ví dụ `RouteDescription` đang cho phép 2.000 ký tự trong database, nhưng vùng PDF chỉ nên nhận khoảng 220 ký tự.
2. Khi vượt giới hạn khuyến nghị, renderer v7 sẽ giảm cỡ chữ. Nếu vẫn không đủ chỗ ở `minFontSize`, nội dung được kết thúc bằng `...`, không tràn sang nhãn bên cạnh.
3. `CONTRACT_TIME` phải truyền theo dạng `HH:mm`, dài đúng 5 ký tự. Không truyền `HH giờ mm phút` vì vùng trên mẫu chỉ rộng khoảng 46 point.
4. Trang 2 chỉ có 20 dòng. Hợp đồng chỉ nên lưu/in tối đa 20 hành khách trên mẫu này.

## Giới hạn khuyến nghị

| Dữ liệu | PDF key | Giới hạn khuyến nghị | Ghi chú |
|---|---|---:|---|
| Số hợp đồng | `CONTRACT_NUMBER` | 12 ký tự | Ví dụ `00125/2026`; không tính `/HĐVC-HTX` có sẵn trên mẫu |
| Giờ lập hợp đồng | `CONTRACT_TIME` | 5 ký tự | Bắt buộc `HH:mm` |
| Tên văn phòng/chi nhánh | `COMPANY_OFFICE_NAME` | 24 ký tự | Viết hoa; vùng hẹp |
| Tên HTX/doanh nghiệp | `COMPANY_NAME` | 40 ký tự | Dài hơn sẽ tự co chữ |
| Mã số thuế | `COMPANY_TAX_CODE`, `CUSTOMER_TAX_CODE` | 14 ký tự | Chỉ số và dấu gạch nếu có |
| Số GPKDVT | `COMPANY_LICENSE` | 20 ký tự | Ví dụ `92240166/GPKDVT` |
| Địa chỉ doanh nghiệp | `COMPANY_ADDRESS` | 65 ký tự | Một dòng |
| Số điện thoại | `COMPANY_PHONE`, `CUSTOMER_PHONE` | 15 ký tự | Nên chuẩn hóa chỉ số hoặc `+84` |
| Người đại diện doanh nghiệp | `COMPANY_REPRESENTATIVE` | 30 ký tự | Họ tên |
| Chủ phương tiện | `OWNER_NAME` | 28 ký tự | Họ tên |
| CCCD | các key `*_CITIZEN_ID` | 20 ký tự | CCCD Việt Nam thực tế thường 12 số |
| Ngày cấp CCCD | các key `*_ISSUED_DATE` | 10 ký tự | `dd/MM/yyyy` |
| Tên cơ quan/người thuê xe | `CUSTOMER_NAME` | 40 ký tự | Nếu là tên công ty dài, nên dùng tên viết tắt hợp lệ |
| Địa chỉ khách hàng | `CUSTOMER_ADDRESS` | 60 ký tự | Một dòng |
| Người đại diện khách hàng | `CUSTOMER_REPRESENTATIVE` | 28 ký tự | Họ tên |
| Biển kiểm soát | `VEHICLE_PLATE` | 12 ký tự | Ví dụ `65B-123.45` |
| Nhãn hiệu và model xe | `VEHICLE_BRAND_MODEL` | 48 ký tự | Một dòng |
| Sức chứa | `SEAT_COUNT` | 3 chữ số | Giá trị 1-999 |
| Số lượng khách | `PASSENGER_COUNT` | 3 chữ số | Mẫu hiện chỉ in tối đa 20 người |
| Họ tên tài xế | `DRIVER_NAME`, `SECOND_DRIVER_NAME` | 40 ký tự | Một dòng |
| Hạng GPLX | `DRIVER_LICENSE_CLASS`, `SECOND_DRIVER_LICENSE_CLASS` | 10 ký tự | Ví dụ `D2`, `FC` |
| Điểm đón | `PICKUP_DATETIME_LOCATION` | 100 ký tự | Đã bao gồm `dd/MM/yyyy HH:mm - `; tối đa 2 dòng |
| Điểm trả | `DROPOFF_DATETIME_LOCATION` | 100 ký tự | Đã bao gồm `dd/MM/yyyy HH:mm - `; tối đa 2 dòng |
| Hành trình | `ROUTE_DESCRIPTION` | 220 ký tự | Tối đa 3 dòng |
| Tổng km | `TOTAL_KILOMETERS` | 12 ký tự | Chỉ số, dấu phẩy/chấm thập phân |
| Giá trị hợp đồng | `CONTRACT_VALUE` | 18 ký tự | Database hiện dùng `decimal(18,2)` |
| Giá trị bằng chữ | `CONTRACT_VALUE_WORDS` | 70 ký tự sinh ra | Nếu dài hơn sẽ co chữ hoặc thêm `...` |
| Hình thức thanh toán | `PAYMENT_METHOD` | 22 ký tự | Ví dụ `Chuyển khoản` |
| Thời gian thanh toán | `PAYMENT_TIME` | 45 ký tự | Một dòng |
| Họ tên hành khách | `P01_NAME` - `P20_NAME` | 40 ký tự/người | Tối đa 20 người |
| Năm sinh | `P01_BIRTH_YEAR` - `P20_BIRTH_YEAR` | 4 chữ số | Nên kiểm tra trong khoảng hợp lệ |
| Ghi chú hành khách | `P01_NOTE` - `P20_NOTE` | 35 ký tự/người | Một dòng |
| Tên dưới chữ ký | các key `SIG_*_NAME` | 35 ký tự | Viết hoa |

## Giới hạn cứng cần kiểm tra ở nghiệp vụ

- `Passengers.Count <= 20`.
- `BirthYear`: 4 chữ số và không lớn hơn năm hiện tại.
- `SeatCount >= 1`.
- `PassengerCount <= SeatCount` và đồng thời `PassengerCount <= 20` đối với mẫu PDF này.
- `StartTime < EndTime`.
- `ContractValue >= 0`, `TotalKilometers >= 0`.

## Thay đổi renderer đi kèm

- Sửa giờ từ `HH giờ mm phút` thành `HH:mm`.
- Sửa thuật toán xuống dòng: không còn làm mất các từ phía sau khi chạm `maxLines`.
- Renderer đo toàn bộ nội dung trước, giảm dần từ 11 pt đến `minFontSize`, sau đó mới thêm `...` nếu vẫn không đủ chỗ.
