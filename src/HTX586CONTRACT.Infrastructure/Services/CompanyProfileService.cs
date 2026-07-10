using HTX586CONTRACT.Application.Abstractions;
using HTX586CONTRACT.Application.Admins.CompanyProfiles;
using HTX586CONTRACT.Domain.Companies;
using HTX586CONTRACT.Infrastructure.Persistence;
using HTX586CONTRACT.Application.Common;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace HTX586CONTRACT.Infrastructure.Services;
public sealed class CompanyProfileService(
    IDbContextFactory<ApplicationDbContext> factory,
    IHostEnvironment environment,
    IOptions<FileStorageOptions> fileStorageOptions) : ICompanyProfileService
{

    public async Task<IReadOnlyList<CompanyProfileListItemDto>> GetListAsync(CompanyProfileFilter filter, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var query = db.CompanyProfiles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(x =>
                x.CompanyName.Contains(keyword) ||
                (x.BranchName != null && x.BranchName.Contains(keyword)) ||
                x.TaxCode.Contains(keyword) ||
                x.RepresentativeName.Contains(keyword) ||
                (x.PhoneNumber != null && x.PhoneNumber.Contains(keyword)));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive.Value);

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);

        return await query
            .OrderBy(x => x.CompanyName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CompanyProfileListItemDto
            {
                Id = x.Id,
                CompanyName = x.CompanyName,
                BranchName = x.BranchName,
                TaxCode = x.TaxCode,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                RepresentativeName = x.RepresentativeName,
                RepresentativeSignatureFileUrl = x.RepresentativeSignatureFileUrl,
                IsActive = x.IsActive,
                AdminCount = db.Users.Count(u => u.CompanyProfileId == x.Id && db.UserRoles.Any(ur => ur.UserId == u.Id && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Admin"))),
                DriverCount = db.Users.Count(u => u.CompanyProfileId == x.Id && db.UserRoles.Any(ur => ur.UserId == u.Id && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Driver"))),
                VehicleCount = db.Vehicles.Count(v => v.CompanyProfileId == x.Id),
                ContractCount = x.Contracts.Count,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<CompanyProfileDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.CompanyProfiles.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CompanyProfileDto
            {
                Id = x.Id,
                CompanyName = x.CompanyName,
                BranchName = x.BranchName,
                TaxCode = x.TaxCode,
                BusinessLicenseNumber = x.BusinessLicenseNumber,
                Address = x.Address,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                RepresentativeName = x.RepresentativeName,
                RepresentativePosition = x.RepresentativePosition,
                RepresentativeCitizenId = x.RepresentativeCitizenId,
                RepresentativeCitizenIdIssuedDate = x.RepresentativeCitizenIdIssuedDate,
                RepresentativeCitizenIdIssuedPlace = x.RepresentativeCitizenIdIssuedPlace,
                BankAccountNumber = x.BankAccountNumber,
                BankName = x.BankName,
                RepresentativeSignatureFileUrl = x.RepresentativeSignatureFileUrl,
                RepresentativeSignedAt = x.RepresentativeSignedAt,
                IsActive = x.IsActive,
                AdminCount = db.Users.Count(u => u.CompanyProfileId == x.Id && db.UserRoles.Any(ur => ur.UserId == u.Id && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Admin"))),
                DriverCount = db.Users.Count(u => u.CompanyProfileId == x.Id && db.UserRoles.Any(ur => ur.UserId == u.Id && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Driver"))),
                VehicleCount = db.Vehicles.Count(v => v.CompanyProfileId == x.Id),
                ContractCount = x.Contracts.Count,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<CompanyProfileOptionDto>> GetActiveOptionsAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.CompanyProfiles.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.CompanyName)
            .Select(x => new CompanyProfileOptionDto
            {
                Id = x.Id,
                CompanyName = x.CompanyName,
                BranchName = x.BranchName,
                TaxCode = x.TaxCode
            })
            .ToListAsync(ct);
    }

    public async Task<Guid> CreateAsync(CreateCompanyProfileRequest request, CancellationToken ct = default)
    {
        Validate(request.CompanyName, request.TaxCode, request.Address, request.RepresentativeName, request.RepresentativeCitizenId);
        await using var db = await factory.CreateDbContextAsync(ct);
        var taxCode = request.TaxCode.Trim();
        if (await db.CompanyProfiles.AnyAsync(x => x.TaxCode == taxCode, ct))
            throw new InvalidOperationException("Mã số thuế đã tồn tại.");

        var entity = new CompanyProfile { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        Map(entity, request);
        db.CompanyProfiles.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateCompanyProfileRequest request, CancellationToken ct = default)
    {
        Validate(request.CompanyName, request.TaxCode, request.Address, request.RepresentativeName, request.RepresentativeCitizenId);
        await using var db = await factory.CreateDbContextAsync(ct);
        var entity = await db.CompanyProfiles.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy công ty/văn phòng đại diện.");

        var taxCode = request.TaxCode.Trim();
        if (await db.CompanyProfiles.AnyAsync(x => x.Id != id && x.TaxCode == taxCode, ct))
            throw new InvalidOperationException("Mã số thuế đã tồn tại.");

        Map(entity, request);
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var entity = await db.CompanyProfiles.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy công ty/văn phòng đại diện.");

        if (await db.Users.AnyAsync(x => x.CompanyProfileId == id, ct))
            throw new InvalidOperationException("Không thể xóa vì CompanyProfile đang được gán cho tài khoản Admin/Driver.");
        if (await db.Vehicles.AnyAsync(x => x.CompanyProfileId == id, ct))
            throw new InvalidOperationException("Không thể xóa vì CompanyProfile đang được gán cho xe.");
        if (await db.Contracts.AnyAsync(x => x.CompanyProfileId == id, ct))
            throw new InvalidOperationException("Không thể xóa vì đơn vị đã được sử dụng trong hợp đồng.");

        db.CompanyProfiles.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    private static void Map(CompanyProfile e, CreateCompanyProfileRequest r)
    {
        e.CompanyName = r.CompanyName.Trim();
        e.BranchName = N(r.BranchName);
        e.TaxCode = r.TaxCode.Trim();
        e.BusinessLicenseNumber = N(r.BusinessLicenseNumber);
        e.Address = r.Address.Trim();
        e.PhoneNumber = N(r.PhoneNumber);
        e.Email = N(r.Email);
        e.RepresentativeName = r.RepresentativeName.Trim();
        e.RepresentativePosition = N(r.RepresentativePosition);
        e.RepresentativeCitizenId = r.RepresentativeCitizenId.Trim();
        e.RepresentativeCitizenIdIssuedDate = r.RepresentativeCitizenIdIssuedDate;
        e.RepresentativeCitizenIdIssuedPlace = N(r.RepresentativeCitizenIdIssuedPlace);
        e.BankAccountNumber = N(r.BankAccountNumber);
        e.BankName = N(r.BankName);
        e.IsActive = r.IsActive;
    }

    private static void Map(CompanyProfile e, UpdateCompanyProfileRequest r)
    {
        e.CompanyName = r.CompanyName.Trim();
        e.BranchName = N(r.BranchName);
        e.TaxCode = r.TaxCode.Trim();
        e.BusinessLicenseNumber = N(r.BusinessLicenseNumber);
        e.Address = r.Address.Trim();
        e.PhoneNumber = N(r.PhoneNumber);
        e.Email = N(r.Email);
        e.RepresentativeName = r.RepresentativeName.Trim();
        e.RepresentativePosition = N(r.RepresentativePosition);
        e.RepresentativeCitizenId = r.RepresentativeCitizenId.Trim();
        e.RepresentativeCitizenIdIssuedDate = r.RepresentativeCitizenIdIssuedDate;
        e.RepresentativeCitizenIdIssuedPlace = N(r.RepresentativeCitizenIdIssuedPlace);
        e.BankAccountNumber = N(r.BankAccountNumber);
        e.BankName = N(r.BankName);
        e.IsActive = r.IsActive;
    }

    private static void Validate(string name, string taxCode, string address, string representative, string citizenId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Vui lòng nhập tên đơn vị.");
        if (string.IsNullOrWhiteSpace(taxCode)) throw new InvalidOperationException("Vui lòng nhập mã số thuế.");
        if (string.IsNullOrWhiteSpace(address)) throw new InvalidOperationException("Vui lòng nhập địa chỉ.");
        if (string.IsNullOrWhiteSpace(representative)) throw new InvalidOperationException("Vui lòng nhập người đại diện.");
        if (string.IsNullOrWhiteSpace(citizenId)) throw new InvalidOperationException("Vui lòng nhập CCCD người đại diện.");
    }

    private static string? N(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    //Update chân ký đại diện HTX/văn phòng. Chữ ký này sẽ được dùng mặc định trên Contract, khách hàng không cần yêu cầu bên HTX ký lại.
    public async Task<ServiceResult> UploadRepresentativeSignatureAsync(
        Guid companyProfileId,
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        if (companyProfileId == Guid.Empty)
            return ServiceResult.Failure("CompanyProfileId không hợp lệ.");

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

        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        var company = await db.CompanyProfiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == companyProfileId, cancellationToken);

        if (company is null)
            return ServiceResult.Failure($"Không tìm thấy CompanyProfile. Id = {companyProfileId}");

        byte[] fileBytes;

        await using (var input = file.OpenReadStream(maxSize, cancellationToken))
        using (var memory = new MemoryStream())
        {
            await input.CopyToAsync(memory, cancellationToken);
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
            "company-signatures");

        Directory.CreateDirectory(folder);

        var fileName =
            $"company-{companyProfileId:N}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.png";

        var physicalPath = Path.Combine(folder, fileName);

        await File.WriteAllBytesAsync(
            physicalPath,
            fileBytes,
            cancellationToken);

        var hash = Convert.ToHexString(SHA256.HashData(fileBytes));

        var publicRoot = fileStorageOptions.Value.PublicRequestPath.TrimEnd('/');

        company.RepresentativeSignatureFileUrl =
            $"{publicRoot}/company-signatures/{fileName}";

        company.RepresentativeSignatureHash = hash;
        company.RepresentativeSignedAt = DateTime.UtcNow;
        company.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success("Đã upload chữ ký PNG thành công.");
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
