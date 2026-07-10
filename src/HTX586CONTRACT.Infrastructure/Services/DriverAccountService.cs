using HTX586CONTRACT.Application.Abstractions;
using HTX586CONTRACT.Application.Admins.DriverAccounts;
using HTX586CONTRACT.Domain.Identity;
using HTX586CONTRACT.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HTX586CONTRACT.Application.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components.Forms;
using System.Security.Cryptography;

namespace HTX586CONTRACT.Infrastructure.Services;

public sealed class DriverAccountService( 
    UserManager<ApplicationUser> userManager,
    IDbContextFactory<ApplicationDbContext> factory,
    IHostEnvironment environment,
    IOptions<FileStorageOptions> fileStorageOptions) : IDriverAccountService
{
    public async Task<string> CreateAsync(CreateDriverAccountRequest request, CancellationToken ct = default)
    {
        ValidateCompany(request.CompanyProfileId);
        if (string.IsNullOrWhiteSpace(request.UserName)) throw new InvalidOperationException("Vui lòng nhập tên đăng nhập.");
        if (string.IsNullOrWhiteSpace(request.Password)) throw new InvalidOperationException("Vui lòng nhập mật khẩu.");
        if (string.IsNullOrWhiteSpace(request.FullName)) throw new InvalidOperationException("Vui lòng nhập họ tên tài xế.");
        await EnsureCompanyAsync(request.CompanyProfileId, ct);

        var user = new ApplicationUser
        {
            UserName = request.UserName.Trim(),
            FullName = request.FullName.Trim(),
            EmployeeCode = N(request.EmployeeCode),
            PhoneNumber = N(request.PhoneNumber),
            Email = N(request.Email),
            CompanyProfileId = request.CompanyProfileId,
            CitizenId = N(request.CitizenId),
            CitizenIdIssuedDate = request.CitizenIdIssuedDate,
            CitizenIdIssuedPlace = N(request.CitizenIdIssuedPlace),
            DateOfBirth = request.DateOfBirth,
            Address = N(request.Address),
            AreaCode = N(request.AreaCode),
            DriverLicenseNumber = N(request.DriverLicenseNumber),
            DriverLicenseClass = N(request.DriverLicenseClass),
            DriverLicenseIssuedDate = request.DriverLicenseIssuedDate,
            DriverLicenseExpiryDate = request.DriverLicenseExpiryDate,
            MustChangePassword = request.MustChangePassword,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password);
        Ensure(result);
        var roleResult = await userManager.AddToRoleAsync(user, "Driver");
        Ensure(roleResult);
        return user.Id;
    }

    public async Task UpdateAsync(string id, UpdateDriverAccountRequest request, CancellationToken ct = default)
    {
        ValidateCompany(request.CompanyProfileId);
        await EnsureCompanyAsync(request.CompanyProfileId, ct);
        var user = await userManager.FindByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy tài xế.");
        await EnsureDriverRoleAsync(user);

        user.CompanyProfileId = request.CompanyProfileId;
        user.FullName = request.FullName.Trim();
        user.EmployeeCode = N(request.EmployeeCode);
        user.PhoneNumber = N(request.PhoneNumber);
        user.Email = N(request.Email);
        user.CitizenId = N(request.CitizenId);
        user.CitizenIdIssuedDate = request.CitizenIdIssuedDate;
        user.CitizenIdIssuedPlace = N(request.CitizenIdIssuedPlace);
        user.DateOfBirth = request.DateOfBirth;
        user.Address = N(request.Address);
        user.AreaCode = N(request.AreaCode);
        user.DriverLicenseNumber = N(request.DriverLicenseNumber);
        user.DriverLicenseClass = N(request.DriverLicenseClass);
        user.DriverLicenseIssuedDate = request.DriverLicenseIssuedDate;
        user.DriverLicenseExpiryDate = request.DriverLicenseExpiryDate;
        user.IsActive = request.IsActive;
        user.MustChangePassword = request.MustChangePassword;
        user.UpdatedAt = DateTime.UtcNow;
        if (!request.IsActive)
        {
            user.DriverSignatureIsActive = false;
            user.DriverSignatureInactiveAt = DateTime.UtcNow;
        }
        Ensure(await userManager.UpdateAsync(user));
    }

    public async Task<DriverAccountDetailDto?> GetDetailAsync(string id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await (from user in db.Users.AsNoTracking()
                      join userRole in db.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
                      join role in db.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                      where user.Id == id && role.Name == "Driver"
                      select user)
            .Select(x => new DriverAccountDetailDto
            {
                UserId = x.Id,
                UserName = x.UserName ?? string.Empty,
                FullName = x.FullName,
                EmployeeCode = x.EmployeeCode,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                CompanyProfileId = x.CompanyProfileId,
                CompanyName = x.CompanyProfile != null ? x.CompanyProfile.CompanyName : null,
                CitizenId = x.CitizenId,
                CitizenIdIssuedDate = x.CitizenIdIssuedDate,
                CitizenIdIssuedPlace = x.CitizenIdIssuedPlace,
                DateOfBirth = x.DateOfBirth,
                Address = x.Address,
                AreaCode = x.AreaCode,
                DriverLicenseNumber = x.DriverLicenseNumber,
                DriverLicenseClass = x.DriverLicenseClass,
                DriverLicenseIssuedDate = x.DriverLicenseIssuedDate,
                DriverLicenseExpiryDate = x.DriverLicenseExpiryDate,

                DriverSignatureFileUrl = x.DriverSignatureFileUrl,
                DriverSignedAt = x.DriverSignedAt,
                DriverSignatureIsActive = x.DriverSignatureIsActive,
                DriverSignatureInactiveAt = x.DriverSignatureInactiveAt,

                IsActive = x.IsActive,
                MustChangePassword = x.MustChangePassword,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<DriverAccountDto>> GetListAsync(DriverAccountFilter filter, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        // Lấy danh sách tài xế (Driver) từ bảng Users, kết hợp với bảng UserRoles và Roles để lọc theo vai trò "Driver".
        var query = from user in db.Users.AsNoTracking()
                    join userRole in db.UserRoles on user.Id equals userRole.UserId
                    join role in db.Roles on userRole.RoleId equals role.Id
                    where role.Name == "Driver"
                    select user;

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            // Tìm kiếm theo họ tên, tên đăng nhập, mã nhân viên, số điện thoại hoặc số CMND/CCCD.
            query = query.Where(x =>
                x.FullName.Contains(keyword) ||
                (x.UserName ?? string.Empty).Contains(keyword) ||
                (x.EmployeeCode ?? string.Empty).Contains(keyword) ||
                (x.PhoneNumber ?? string.Empty).Contains(keyword) ||
                (x.CitizenId ?? string.Empty).Contains(keyword));
        }
        if (filter.IsActive.HasValue) query = query.Where(x => x.IsActive == filter.IsActive.Value);
        if (filter.CompanyProfileId.HasValue) query = query.Where(x => x.CompanyProfileId == filter.CompanyProfileId.Value);

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);
        return await query.OrderBy(x => x.FullName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new DriverAccountDto
            {
                Id = x.Id,
                UserName = x.UserName ?? string.Empty,
                FullName = x.FullName,
                EmployeeCode = x.EmployeeCode,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                CompanyName = x.CompanyProfile != null ? x.CompanyProfile.CompanyName : null,
                CitizenId = x.CitizenId,
                DriverLicenseNumber = x.DriverLicenseNumber,
                DriverLicenseClass = x.DriverLicenseClass,

                DriverSignatureFileUrl = x.DriverSignatureFileUrl,
                DriverSignatureIsActive = x.DriverSignatureIsActive,
                DriverSignatureInactiveAt = x.DriverSignatureInactiveAt,

                IsActive = x.IsActive,
                MustChangePassword = x.MustChangePassword,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToListAsync(ct);
    }

    public async Task SetActiveAsync(string id, bool active, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id) 
            ?? throw new KeyNotFoundException("Không tìm thấy tài xế.");

        await EnsureDriverRoleAsync(user);

        user.IsActive = active;
        user.UpdatedAt = DateTime.UtcNow;

        if (!active)
        {
            user.DriverSignatureIsActive = false;
            user.DriverSignatureInactiveAt = DateTime.UtcNow;
        }

        Ensure(await userManager.UpdateAsync(user));
    }

    public async Task ResetPasswordAsync(string id, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy tài xế.");
        await EnsureDriverRoleAsync(user);
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        Ensure(await userManager.ResetPasswordAsync(user, token, password));
        user.MustChangePassword = true;
        Ensure(await userManager.UpdateAsync(user));
    }

    public async Task RequirePasswordChangeAsync(string id, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy tài xế.");
        await EnsureDriverRoleAsync(user);
        user.MustChangePassword = true;
        Ensure(await userManager.UpdateAsync(user));
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        if (await db.Contracts.AnyAsync(x => x.DriverId == id, ct))
            throw new InvalidOperationException("Không thể xóa tài xế đã có hợp đồng. Hãy khóa tài khoản thay vì xóa.");
        var user = await userManager.FindByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy tài xế.");
        await EnsureDriverRoleAsync(user);
        Ensure(await userManager.DeleteAsync(user));
    }

    private async Task EnsureDriverRoleAsync(ApplicationUser user)
    {
        if (!await userManager.IsInRoleAsync(user, "Driver"))
            throw new KeyNotFoundException("Không tìm thấy tài xế.");
    }

    private async Task EnsureCompanyAsync(Guid companyId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        if (!await db.CompanyProfiles.AnyAsync(x => x.Id == companyId && x.IsActive, ct))
            throw new InvalidOperationException("Công ty/văn phòng đại diện không tồn tại hoặc đã ngừng hoạt động.");
    }

    private static void ValidateCompany(Guid companyId)
    {
        if (companyId == Guid.Empty) throw new InvalidOperationException("Tài xế bắt buộc phải được gán công ty/văn phòng đại diện.");
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    }

    private static string? N(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public async Task<ServiceResult> UploadDriverSignatureAsync(
    string driverId,
    IBrowserFile file,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(driverId))
            return ServiceResult.Failure("DriverId không hợp lệ.");

        if (file is null)
            return ServiceResult.Failure("Vui lòng chọn file chữ ký.");

        var extension = Path.GetExtension(file.Name).ToLowerInvariant();

        if (extension != ".png")
            return ServiceResult.Failure("Chỉ cho phép upload file PNG.");

        const long maxSize = 2 * 1024 * 1024;

        if (file.Size <= 0)
            return ServiceResult.Failure("File không hợp lệ.");

        if (file.Size > maxSize)
            return ServiceResult.Failure("File chữ ký không được vượt quá 2MB.");

        var user = await userManager.FindByIdAsync(driverId)
            ?? throw new KeyNotFoundException("Không tìm thấy tài xế.");

        await EnsureDriverRoleAsync(user);

        if (!user.IsActive)
            return ServiceResult.Failure("Tài xế đang ngừng hoạt động, không thể upload chữ ký mới.");

        byte[] fileBytes;

        await using (var input = file.OpenReadStream(maxSize, ct))
        using (var memory = new MemoryStream())
        {
            await input.CopyToAsync(memory, ct);
            fileBytes = memory.ToArray();
        }

        if (fileBytes.Length < 8 ||
            fileBytes[0] != 0x89 ||
            fileBytes[1] != 0x50 ||
            fileBytes[2] != 0x4E ||
            fileBytes[3] != 0x47)
        {
            return ServiceResult.Failure("File không đúng định dạng PNG.");
        }

        var uploadRoot = GetUploadPhysicalRoot();

        var folder = Path.Combine(
            uploadRoot,
            "driver-signatures");

        Directory.CreateDirectory(folder);

        var fileName =
            $"driver-{driverId}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.png";

        var physicalPath = Path.Combine(folder, fileName);

        await File.WriteAllBytesAsync(physicalPath, fileBytes, ct);

        var hash = Convert.ToHexString(SHA256.HashData(fileBytes));
        var publicRoot = fileStorageOptions.Value.PublicRequestPath.TrimEnd('/');

        user.DriverSignatureFileUrl = $"{publicRoot}/driver-signatures/{fileName}";
        user.DriverSignatureHash = hash;
        user.DriverSignedAt = DateTime.UtcNow;

        user.DriverSignatureIsActive = true;
        user.DriverSignatureInactiveAt = null;

        user.UpdatedAt = DateTime.UtcNow;

        Ensure(await userManager.UpdateAsync(user));

        return ServiceResult.Success("Đã upload chữ ký tài xế thành công.");
    }
    private string GetUploadPhysicalRoot()
    {
        var rootPath = fileStorageOptions.Value.UploadRootPath;

        return Path.IsPathRooted(rootPath)
            ? rootPath
            : Path.GetFullPath(Path.Combine(
                environment.ContentRootPath,
                rootPath));
    }
}
