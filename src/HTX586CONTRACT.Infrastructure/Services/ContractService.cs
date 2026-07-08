using HTX586CONTRACT.Application.Abstractions;
using HTX586CONTRACT.Application.Contracts;
using HTX586CONTRACT.Domain.Companies;
using HTX586CONTRACT.Domain.Contracts;
using HTX586CONTRACT.Domain.Customers;
using HTX586CONTRACT.Domain.Enums;
using HTX586CONTRACT.Domain.Identity;
using HTX586CONTRACT.Domain.Vehicles;
using HTX586CONTRACT.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HTX586CONTRACT.Infrastructure.Services;

public sealed class ContractService(
    IDbContextFactory<ApplicationDbContext> factory,
    UserManager<ApplicationUser> userManager) : IContractService
{
    public async Task<IReadOnlyList<ContractListItemDto>> GetAsync(ContractFilter filter, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var query = db.Contracts.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(x =>
                x.ContractNumber.Contains(search) ||
                x.CustomerNameSnapshot.Contains(search) ||
                x.DriverNameSnapshot.Contains(search) ||
                (x.VehiclePlateSnapshot != null && x.VehiclePlateSnapshot.Contains(search)));
        }

        if (filter.Status.HasValue) query = query.Where(x => x.Status == filter.Status.Value);
        if (filter.BusinessType.HasValue) query = query.Where(x => x.BusinessType == filter.BusinessType.Value);
        if (!string.IsNullOrWhiteSpace(filter.DriverId)) query = query.Where(x => x.DriverId == filter.DriverId);
        if (filter.CompanyProfileId.HasValue) query = query.Where(x => x.CompanyProfileId == filter.CompanyProfileId.Value);
        if (filter.FromDate.HasValue) query = query.Where(x => x.CreatedAt >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue) query = query.Where(x => x.CreatedAt < filter.ToDate.Value.Date.AddDays(1));

        return await query.OrderByDescending(x => x.CreatedAt)
            .Select(x => new ContractListItemDto
            {
                Id = x.Id,
                ContractNumber = x.ContractNumber,
                BusinessType = x.BusinessType,
                CompanyName = x.CompanyNameSnapshot,
                CustomerName = x.CustomerNameSnapshot,
                DriverName = x.DriverNameSnapshot,
                VehiclePlate = x.VehiclePlateSnapshot,
                StartTime = x.StartTime,
                ContractValue = x.ContractValue,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);
    }

    public Task<IReadOnlyList<ContractListItemDto>> GetDriverContractsAsync(string driverId, CancellationToken ct = default)
        => GetAsync(new ContractFilter { DriverId = driverId }, ct);

    public async Task<ContractDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Contracts.AsNoTracking()
            .Include(x => x.Passengers)
            .Include(x => x.Signatures)
            .Where(x => x.Id == id)
            .Select(x => new ContractDetailDto
            {
                Id = x.Id,
                ContractNumber = x.ContractNumber,
                BusinessType = x.BusinessType,
                ContractTypeId = x.ContractTypeId,
                Status = x.Status,
                CompanyProfileId = x.CompanyProfileId,
                CompanyName = x.CompanyNameSnapshot,
                DriverId = x.DriverId,
                DriverName = x.DriverNameSnapshot,
                DriverLicenseClass = x.DriverLicenseClassSnapshot,
                CustomerId = x.CustomerId,
                CustomerName = x.CustomerNameSnapshot,
                CustomerPhone = x.CustomerPhoneSnapshot,
                CustomerCitizenId = x.CustomerCitizenIdSnapshot,
                CustomerAddress = x.CustomerAddressSnapshot,
                AreaCode = x.AreaCode,
                VehicleId = x.VehicleId,
                VehiclePlate = x.VehiclePlateSnapshot,
                VehicleBrand = x.VehicleBrandSnapshot,
                ActualPassengerCount = x.ActualPassengerCount,
                OwnerName = x.VehicleOwnerNameSnapshot,
                OwnerCitizenId = x.VehicleOwnerCitizenIdSnapshot,
                CargoName = x.CargoName,
                CargoWeight = x.CargoWeight,
                CargoUnit = x.CargoUnit,
                SecondDriverName = x.SecondDriverName,
                SecondDriverLicenseClass = x.SecondDriverLicenseClass,
                PickupLocation = x.PickupLocation,
                DropoffLocation = x.DropoffLocation,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                RouteDescription = x.RouteDescription,
                TotalKilometers = x.TotalKilometers,
                ContractValue = x.ContractValue,
                PaymentMethod = x.PaymentMethod,
                PaymentTime = x.PaymentTime,
                Note = x.Note,
                PdfFileUrl = x.PdfFileUrl,
                CreatedAt = x.CreatedAt,
                Passengers = x.Passengers.OrderBy(p => p.SortOrder).Select(p => new ContractPassengerDto
                {
                    Id = p.Id,
                    SortOrder = p.SortOrder,
                    FullName = p.FullName,
                    BirthYear = p.BirthYear,
                    Note = p.Note
                }).ToList(),
                Signatures = x.Signatures.OrderBy(s => s.ServerSignedAt).Select(s => new ContractSignatureDto
                {
                    Id = s.Id,
                    Party = s.Party,
                    SignerName = s.SignerName,
                    SignatureFileUrl = s.SignatureFileUrl,
                    ServerSignedAt = s.ServerSignedAt
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SaveContractResult> CreateAsync(SaveContractRequest request, string currentUserId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var access = await GetAccessAsync(db, currentUserId, ct);
        var canManageContracts = access.IsOwner || access.IsAdmin;
        if (!canManageContracts && MissingCustomerInfo(request))
            return new(false, null, "Vui lòng nhập tên và số điện thoại khách hàng trước khi chốt hợp đồng.");

        if (canManageContracts && string.IsNullOrWhiteSpace(request.DriverId))
            return new(false, null, "Vui lòng chọn tài xế nhận hợp đồng.");

        var driverId = canManageContracts ? request.DriverId : currentUserId;

        var driver = await db.Users.Include(x => x.CompanyProfile).FirstOrDefaultAsync(x => x.Id == driverId && x.IsActive, ct);
        if (driver is null) return new(false, null, "Không tìm thấy tài xế hoặc tài xế đã bị khóa.");
        if (driver.CompanyProfileId is null || driver.CompanyProfile is null)
            return new(false, null, "Tài xế chưa được gán CompanyProfile.");
        if (!driver.CompanyProfile.IsActive)
            return new(false, null, "CompanyProfile của tài xế đã ngừng hoạt động.");
        if (access.IsAdmin && !access.IsOwner && driver.CompanyProfileId != access.CompanyProfileId)
            return new(false, null, "Admin chỉ được phát hợp đồng cho tài xế thuộc CompanyProfile được gán.");

        var type = await ResolveTypeAsync(db, request, ct);
        if (type is null) return new(false, null, "Chưa cấu hình loại hợp đồng phù hợp.");
        var template = await db.ContractTemplates.FirstOrDefaultAsync(x => x.ContractTypeId == type.Id && x.IsActive, ct);
        if (template is null) return new(false, null, "Chưa cấu hình mẫu hợp đồng đang hoạt động.");

        var vehicle = await ResolveVehicleAsync(db, request, driver, ct);
        if (vehicle is null)
            return new(false, null, canManageContracts
                ? "Vui lòng chọn xe đang hoạt động, cùng CompanyProfile và đã gán cho tài xế được chọn."
                : "Tài xế chỉ được tạo hợp đồng khi chọn xe đang hoạt động đã được gán cho mình.");

        var fixedSignatureError = ValidateFixedSignatures(driver, driver.CompanyProfile, vehicle);
        if (fixedSignatureError is not null) return new(false, null, fixedSignatureError);

        var customer = await ResolveCustomerAsync(db, request, driverId, currentUserId, allowPlaceholder: canManageContracts, ct);

        var entity = new Contract
        {
            Id = Guid.NewGuid(),
            ContractNumber = string.IsNullOrWhiteSpace(request.ContractNumber)
                ? $"{DateTime.Now:yyyyMMddHHmmss}/{BusinessCode(request.BusinessType)}"
                : request.ContractNumber.Trim(),
            BusinessType = request.BusinessType,
            ContractTypeId = type.Id,
            ContractTemplateId = template.Id,
            CompanyProfileId = driver.CompanyProfileId.Value,
            DriverId = driver.Id,
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            Status = ContractStatus.Draft,
            ContractContentSnapshot = template.HtmlContent,
            CreatedBy = currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        Apply(entity, request);
        ApplySnapshots(entity, driver, driver.CompanyProfile, customer, vehicle);
        AddPassengers(entity, request.Passengers, currentUserId);
        db.Contracts.Add(entity);
        await db.SaveChangesAsync(ct);
        return new(true, entity.Id, canManageContracts
            ? "Đã tạo hợp đồng cơ bản và phát xuống tài xế. Tài xế có thể bổ sung thông tin khách hàng, danh sách hành khách và chữ ký khách hàng."
            : "Đã tạo hợp đồng. Chỉ cần khách hàng ký để hoàn tất.");
    }

    public async Task<SaveContractResult> UpdateAsync(Guid id, SaveContractRequest request, string currentUserId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var entity = await db.Contracts
            .Include(x => x.Passengers)
            .Include(x => x.Signatures)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return new(false, null, "Không tìm thấy hợp đồng.");
        if (entity.Status is ContractStatus.Completed or ContractStatus.Cancelled or ContractStatus.Invalidated)
            return new(false, id, "Hợp đồng đã hoàn tất, đã hủy hoặc đã vô hiệu hóa và không thể chỉnh sửa.");

        var access = await GetAccessAsync(db, currentUserId, ct);
        var canManageContracts = access.IsOwner || access.IsAdmin;
        if (access.IsAdmin && !access.IsOwner && entity.CompanyProfileId != access.CompanyProfileId)
            return new(false, id, "Admin chỉ được cập nhật hợp đồng thuộc CompanyProfile được gán.");
        if (!canManageContracts && !string.Equals(entity.DriverId, currentUserId, StringComparison.Ordinal))
            return new(false, id, "Bạn không có quyền cập nhật hợp đồng này.");

        if (entity.Signatures.Count != 0)
            return new(false, id, "Hợp đồng đã có chữ ký nên không được cập nhật nội dung.");

        if (!canManageContracts && MissingCustomerInfo(request))
            return new(false, id, "Vui lòng nhập tên và số điện thoại khách hàng trước khi chốt hợp đồng.");

        var driverId = canManageContracts && !string.IsNullOrWhiteSpace(request.DriverId)
            ? request.DriverId
            : entity.DriverId;
        var driver = await db.Users.Include(x => x.CompanyProfile).FirstOrDefaultAsync(x => x.Id == driverId && x.IsActive, ct);
        if (driver?.CompanyProfileId is null || driver.CompanyProfile is null)
            return new(false, id, "Tài xế chưa được gán CompanyProfile hoặc tài khoản đã bị khóa.");
        if (!driver.CompanyProfile.IsActive)
            return new(false, id, "CompanyProfile của tài xế đã ngừng hoạt động.");
        if (access.IsAdmin && !access.IsOwner && driver.CompanyProfileId != access.CompanyProfileId)
            return new(false, id, "Admin chỉ được chọn tài xế thuộc CompanyProfile được gán.");

        var type = await ResolveTypeAsync(db, request, ct);
        if (type is null) return new(false, id, "Chưa cấu hình loại hợp đồng phù hợp.");
        var template = await db.ContractTemplates.FirstOrDefaultAsync(x => x.ContractTypeId == type.Id && x.IsActive, ct);
        if (template is null) return new(false, id, "Chưa cấu hình mẫu hợp đồng đang hoạt động.");

        var vehicle = await ResolveVehicleAsync(db, request, driver, ct);
        if (vehicle is null)
            return new(false, id, canManageContracts
                ? "Vui lòng chọn xe đang hoạt động, cùng CompanyProfile và đã gán cho tài xế được chọn."
                : "Tài xế chỉ được dùng xe đang hoạt động đã được gán cho mình.");

        var fixedSignatureError = ValidateFixedSignatures(driver, driver.CompanyProfile, vehicle);
        if (fixedSignatureError is not null) return new(false, id, fixedSignatureError);

        var customer = await ResolveCustomerAsync(db, request, driverId, currentUserId, allowPlaceholder: canManageContracts, ct);

        entity.DriverId = driver.Id;
        entity.ContractTypeId = type.Id;
        entity.ContractTemplateId = template.Id;
        entity.ContractContentSnapshot = template.HtmlContent;
        entity.CompanyProfileId = driver.CompanyProfileId.Value;
        entity.CustomerId = customer.Id;
        entity.VehicleId = vehicle.Id;
        entity.BusinessType = request.BusinessType;
        Apply(entity, request);
        ApplySnapshots(entity, driver, driver.CompanyProfile, customer, vehicle);
        db.ContractPassengers.RemoveRange(entity.Passengers);
        AddPassengers(entity, request.Passengers, currentUserId);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserId;
        await db.SaveChangesAsync(ct);
        return new(true, id, canManageContracts ? "Đã cập nhật hợp đồng cơ bản cho tài xế." : "Đã cập nhật hợp đồng.");
    }

    public async Task<SaveContractResult> CancelByDriverAsync(
        Guid id,
        string currentUserId,
        string? reason = null,
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var entity = await db.Contracts
            .Include(x => x.Signatures)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
            return new(false, null, "Không tìm thấy hợp đồng.");

        if (!string.Equals(entity.DriverId, currentUserId, StringComparison.Ordinal))
            return new(false, id, "Bạn không có quyền hủy hợp đồng này.");

        if (entity.Status == ContractStatus.Cancelled)
            return new(false, id, "Hợp đồng đã được hủy trước đó.");

        if (entity.Status is ContractStatus.Completed or ContractStatus.Invalidated)
            return new(false, id, "Hợp đồng đã hoàn tất hoặc đã vô hiệu hóa nên không thể hủy.");

        if (entity.Signatures.Count != 0)
            return new(false, id, "Hợp đồng đã có chân ký nên không thể hủy.");

        entity.Status = ContractStatus.Cancelled;
        entity.CancelledAt = DateTime.UtcNow;
        entity.CancelReason = string.IsNullOrWhiteSpace(reason)
            ? "Tài xế hủy trước khi các bên ký."
            : reason.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserId;

        await db.SaveChangesAsync(ct);
        return new(true, id, "Đã hủy hợp đồng.");
    }

    public async Task<bool> DeleteAsync(Guid id, string currentUserId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var entity = await db.Contracts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null || entity.Status == ContractStatus.Completed) return false;

        var access = await GetAccessAsync(db, currentUserId, ct);
        var canManageContracts = access.IsOwner || access.IsAdmin;
        if (access.IsAdmin && !access.IsOwner && entity.CompanyProfileId != access.CompanyProfileId) return false;
        if (!canManageContracts && !string.Equals(entity.DriverId, currentUserId, StringComparison.Ordinal)) return false;

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = currentUserId;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private sealed record UserAccess(bool IsOwner, bool IsAdmin, Guid? CompanyProfileId);

    private async Task<UserAccess> GetAccessAsync(ApplicationDbContext db, string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new(false, false, null);

        var companyProfileId = await db.Users.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.CompanyProfileId)
            .FirstOrDefaultAsync(ct);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return new(false, false, companyProfileId);

        var isOwner = await userManager.IsInRoleAsync(user, "Owner");
        var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
        return new(isOwner, isAdmin, companyProfileId);
    }

    private static async Task<ContractType?> ResolveTypeAsync(ApplicationDbContext db, SaveContractRequest request, CancellationToken ct)
    {
        if (request.ContractTypeId.HasValue)
            return await db.ContractTypes.FirstOrDefaultAsync(x => x.Id == request.ContractTypeId.Value && x.IsActive, ct);

        var code = request.BusinessType switch
        {
            ContractBusinessType.Cargo => "CARGO",
            ContractBusinessType.LongDistance => "LONG_DISTANCE",
            _ => "DRIVER"
        };
        return await db.ContractTypes.FirstOrDefaultAsync(x => x.Code == code && x.IsActive, ct);
    }

    private static async Task<Customer> ResolveCustomerAsync(
        ApplicationDbContext db,
        SaveContractRequest request,
        string driverId,
        string currentUserId,
        bool allowPlaceholder,
        CancellationToken ct)
    {
        var hasName = !string.IsNullOrWhiteSpace(request.CustomerName);
        var hasPhone = !string.IsNullOrWhiteSpace(request.CustomerPhone);

        Customer? customer = null;
        if (request.CustomerId.HasValue)
            customer = await db.Customers.FirstOrDefaultAsync(x => x.Id == request.CustomerId.Value, ct);
        if (customer is null && hasPhone)
            customer = await db.Customers.FirstOrDefaultAsync(x => x.PhoneNumber == request.CustomerPhone!.Trim() && x.CreatedByDriverId == driverId, ct);

        if (customer is null)
        {
            customer = new Customer { Id = Guid.NewGuid(), CreatedByDriverId = driverId, CreatedBy = currentUserId };
            db.Customers.Add(customer);
        }

        customer.FullName = hasName ? request.CustomerName.Trim() : "Chờ tài xế bổ sung";
        customer.PhoneNumber = hasPhone ? request.CustomerPhone.Trim() : PendingPhone();
        customer.CitizenId = N(request.CustomerCitizenId);
        customer.Address = N(request.CustomerAddress);
        customer.LastUsedAt = DateTime.UtcNow;

        if (!allowPlaceholder && MissingCustomerInfo(request))
            throw new InvalidOperationException("Vui lòng nhập tên và số điện thoại khách hàng.");

        return customer;
    }

    private static async Task<Vehicle?> ResolveVehicleAsync(
        ApplicationDbContext db,
        SaveContractRequest request,
        ApplicationUser driver,
        CancellationToken ct)
    {
        Vehicle? vehicle = null;
        if (request.VehicleId.HasValue)
            vehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == request.VehicleId.Value && x.IsActive, ct);
        if (vehicle is null && !string.IsNullOrWhiteSpace(request.VehiclePlate))
        {
            var plate = request.VehiclePlate.Trim().ToUpperInvariant();
            vehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.PlateNumber == plate && x.IsActive, ct);
        }

        if (vehicle is null || driver.CompanyProfileId is null)
            return null;

        if (vehicle.CompanyProfileId != driver.CompanyProfileId)
            return null;

        if (!string.Equals(vehicle.AssignedDriverId, driver.Id, StringComparison.Ordinal))
            return null;

        return vehicle;
    }

    private static string? ValidateFixedSignatures(ApplicationUser driver, CompanyProfile company, Vehicle vehicle)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(company.RepresentativeSignatureFileUrl))
            missing.Add("chữ ký cố định Company/văn phòng đại diện");

        if (string.IsNullOrWhiteSpace(vehicle.OwnerSignatureFileUrl))
            missing.Add("chữ ký cố định chủ sở hữu xe");

        if (string.IsNullOrWhiteSpace(driver.DriverSignatureFileUrl))
            missing.Add("chữ ký cố định tài xế");

        return missing.Count == 0
            ? null
            : $"Chưa thể tạo/cập nhật hợp đồng. Còn thiếu: {string.Join(", ", missing)}.";
    }

    private static void Apply(Contract e, SaveContractRequest r)
    {
        if (!string.IsNullOrWhiteSpace(r.ContractNumber)) e.ContractNumber = r.ContractNumber.Trim();
        e.AreaCode = string.IsNullOrWhiteSpace(r.AreaCode) ? "N/A" : r.AreaCode.Trim();
        e.CargoName = N(r.CargoName);
        e.CargoWeight = r.CargoWeight;
        e.CargoUnit = N(r.CargoUnit);
        e.ActualPassengerCount = r.ActualPassengerCount;
        e.SecondDriverName = N(r.SecondDriverName);
        e.SecondDriverLicenseClass = N(r.SecondDriverLicenseClass);
        e.PickupLocation = N(r.PickupLocation);
        e.DropoffLocation = N(r.DropoffLocation);
        e.StartTime = r.StartTime;
        e.EndTime = r.EndTime;
        e.RouteDescription = N(r.RouteDescription);
        e.TotalKilometers = r.TotalKilometers;
        e.ContractValue = r.ContractValue;
        e.PaymentMethod = N(r.PaymentMethod);
        e.PaymentTime = N(r.PaymentTime);
        e.Note = N(r.Note);
    }

    private static void ApplySnapshots(Contract e, ApplicationUser driver, CompanyProfile company, Customer customer, Vehicle vehicle)
    {
        e.CompanyNameSnapshot = company.CompanyName;
        e.CompanyTaxCodeSnapshot = company.TaxCode;
        e.CompanyAddressSnapshot = company.Address;
        e.CompanyRepresentativeSnapshot = company.RepresentativeName;
        e.CompanyRepresentativePositionSnapshot = company.RepresentativePosition;
        e.DriverNameSnapshot = driver.FullName;
        e.DriverLicenseNumberSnapshot = driver.DriverLicenseNumber;
        e.DriverLicenseClassSnapshot = driver.DriverLicenseClass;
        e.CustomerNameSnapshot = customer.FullName;
        e.CustomerPhoneSnapshot = customer.PhoneNumber;
        e.CustomerCitizenIdSnapshot = customer.CitizenId;
        e.CustomerAddressSnapshot = customer.Address;
        e.VehiclePlateSnapshot = vehicle.PlateNumber;
        e.VehicleBrandSnapshot = vehicle.Brand;
        e.VehicleOwnerNameSnapshot = vehicle.OwnerName;
        e.VehicleOwnerCitizenIdSnapshot = vehicle.OwnerCitizenId;
    }

    private static void AddPassengers(Contract e, IEnumerable<ContractPassengerDto> passengers, string userId)
    {
        foreach (var item in passengers.Where(x => !string.IsNullOrWhiteSpace(x.FullName)).Select((x, i) => (x, i)))
            e.Passengers.Add(new ContractPassenger { SortOrder = item.i + 1, FullName = item.x.FullName.Trim(), BirthYear = item.x.BirthYear, Note = N(item.x.Note), CreatedBy = userId });
    }

    private static bool MissingCustomerInfo(SaveContractRequest request)
        => string.IsNullOrWhiteSpace(request.CustomerName) || string.IsNullOrWhiteSpace(request.CustomerPhone);

    private static string PendingPhone()
        => ($"PEND{Guid.NewGuid():N}")[..20];

    private static string BusinessCode(ContractBusinessType type) => type switch
    {
        ContractBusinessType.Cargo => "HH",
        ContractBusinessType.LongDistance => "ĐD",
        _ => "TX"
    };

    private static string? N(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
