namespace HTX586CONTRACT.Application.Admins.DriverAccounts;
public sealed class DriverAccountDto
{
    public string Id {get;set;}=""; 
    public string UserName {get;set;}=""; 
    public string FullName {get;set;}=""; 
    public string? EmployeeCode {get;set;}
    public string? PhoneNumber {get;set;} 
    public string? Email {get;set;} 
    public string? CompanyName {get;set;} 
    public string? CitizenId {get;set;}
    public string? DriverLicenseNumber {get;set;} 
    public string? DriverLicenseClass {get;set;} 

    public string? DriverSignatureFileUrl {get;set;} 
    // Trạng thái chữ ký.
    public bool DriverSignatureIsActive { get; set; }
    // Trang thái chữ ký (Đang sử dụng / Đã hủy / chưa có chữ ký)
    public string DriverSignatureStatusText =>
        DriverSignatureIsActive ? "Đang sử dụng" : "Đã hủy / chưa có chữ ký";
    //Thơi gian vô hiệu hóa chữ ký
    public DateTime? DriverSignatureInactiveAt { get; set; }

    public bool IsActive {get;set;} 
    public bool MustChangePassword {get;set;}
    public DateTime CreatedAt {get;set;} 
    public DateTime? UpdatedAt {get;set;}
}
