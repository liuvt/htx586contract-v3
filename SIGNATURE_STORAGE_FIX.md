# Đồng bộ lưu chữ ký

## Cấu trúc thống nhất

- CompanyProfile: `HTX586CONTRACT_Data/uploads/master-signatures/companies/{companyProfileId}/`
- Driver: `HTX586CONTRACT_Data/uploads/master-signatures/drivers/{driverId}/`
- Vehicle owner: `HTX586CONTRACT_Data/uploads/master-signatures/vehicles/{vehicleId}/`

URL lưu trong database tương ứng bắt đầu bằng `/uploads/master-signatures/...` (hoặc `FileStorage:PublicRequestPath`).

## Luồng đã đồng bộ

- Ký trực tiếp bằng SignaturePad.
- Upload file PNG tại màn hình CompanyProfile.
- Upload file PNG tại màn hình Driver do Owner/Admin thao tác.
- Lưu lại chữ ký Driver sẽ đặt `DriverSignatureIsActive = true` và xóa thời điểm vô hiệu hóa.

## Tương thích dữ liệu cũ

Các URL cũ trong database vẫn được `LocalUploadFileStorage.ToPhysicalPath` hỗ trợ. File mới sẽ chỉ được ghi vào cấu trúc `master-signatures` thống nhất.
