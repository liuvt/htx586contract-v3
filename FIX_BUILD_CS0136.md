# Fix build ContractService.cs

## Lỗi đã sửa

- `CS0136` tại `ContractService.cs`: đổi biến `customer` trong nhánh tài xế thành `driverCustomer` để không trùng với biến `customer` ở nhánh Owner/Admin.
- Bổ sung kiểm tra `driver.CompanyProfileId` và `driver.CompanyProfile` sau khi phân công xe/tài xế.
- Dùng biến cục bộ `companyProfileId` và `companyProfile`, loại các cảnh báo nullable tại luồng tạo/cập nhật hợp đồng.

## Cảnh báo còn lại

Hai cảnh báo `CS9113` trong `CompanyProfileService.cs` chỉ báo constructor parameter chưa được sử dụng, không làm build thất bại.
