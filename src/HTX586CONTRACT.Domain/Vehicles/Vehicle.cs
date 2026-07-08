using HTX586CONTRACT.Domain.Common;
using HTX586CONTRACT.Domain.Companies;
using HTX586CONTRACT.Domain.Contracts;
using HTX586CONTRACT.Domain.Identity;

namespace HTX586CONTRACT.Domain.Vehicles;

/// <summary>
/// Xe và thông tin chủ sở hữu xe. Owner gán xe vào CompanyProfile và tài xế phụ trách.
/// </summary>
public class Vehicle : BaseEntity
{
    public string PlateNumber { get; set; } = string.Empty;
    public string? VehicleCode { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? VehicleType { get; set; }
    public int? SeatCount { get; set; }
    public string? Color { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }

    public string OwnerName { get; set; } = string.Empty;
    public string? OwnerCitizenId { get; set; }
    public DateTime? OwnerCitizenIdIssuedDate { get; set; }
    public string? OwnerCitizenIdIssuedPlace { get; set; }
    public string? OwnerAddress { get; set; }
    public string? OwnerPhoneNumber { get; set; }

    // CompanyProfile quản lý xe. Owner gán; Admin/Driver chỉ dùng xe trong CompanyProfile của mình.
    public Guid? CompanyProfileId { get; set; }
    public CompanyProfile? CompanyProfile { get; set; }

    // Tài xế được giao xe. Driver chỉ được tự tạo hợp đồng khi có xe được gán.
    public string? AssignedDriverId { get; set; }
    public ApplicationUser? AssignedDriver { get; set; }

    // Chữ ký cố định của chủ sở hữu xe.
    // Contract tự lấy chữ ký này theo VehicleId tại thời điểm xuất PDF.
    public string? OwnerSignatureFileUrl { get; set; }
    public string? OwnerSignatureHash { get; set; }
    public DateTime? OwnerSignedAt { get; set; }

    public bool IsActive { get; set; } = true;
    public ICollection<Contract> Contracts { get; set; } = [];
}
