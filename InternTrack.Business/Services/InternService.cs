using InternTrack.Core.Helpers;
using InternTrack.Core.Constants;
using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;

namespace InternTrack.Business.Services;

public class InternService : IInternService
{
    private readonly IInternRepository _internRepository;

    private readonly IDepartmentRepository _departmentRepository;

    private readonly IUserRepository _userRepository;

    private readonly IAppLogger _logger;

    public InternService(
        IInternRepository internRepository,
        IDepartmentRepository departmentRepository,
        IUserRepository userRepository,
        IAppLogger logger)
    {
        _internRepository = internRepository;

        _departmentRepository = departmentRepository;

        _userRepository = userRepository;

        _logger = logger;
    }

    private static InternResponseDto ToResponseDto(Intern intern)
    {
        return new InternResponseDto
        {
            Id = intern.Id,

            Name = intern.Name,

            Surname = intern.Surname,

            Email = intern.Email,

            DepartmentId = intern.DepartmentId,

            Department = intern.Department,

            UserId = intern.UserId,

            Avatar = intern.User?.Avatar,

            IsActive = intern.IsActive
        };
    }

    public async Task<ServiceResult<List<InternResponseDto>>> GetAllAsync(int userId, string role)
    {
        if (RoleHelper.IsAdminClaim(role) || RoleHelper.IsHrClaim(role))
        {
            var interns = await _internRepository.GetAllAsync();

            var result = interns.Select(ToResponseDto).ToList();

            return ServiceResult<List<InternResponseDto>>.Ok(result);
        }

        var intern = await _internRepository.GetByUserIdAsync(userId);

        if (intern == null)
        {
            _logger.LogWarning(
                "Intern list could not be retrieved because intern profile was not found. UserId: {UserId}",
                userId);

            return ServiceResult<List<InternResponseDto>>.NotFound("Stajyer profili bulunamadı.");
        }

        var currentInternDto = ToResponseDto(intern);

        var currentInternResult = new List<InternResponseDto>
        {
            currentInternDto
        };

        return ServiceResult<List<InternResponseDto>>.Ok(currentInternResult);
    }

    public async Task<ServiceResult<List<InternResponseDto>>> GetAllIncludingInactiveAsync()
    {
        var interns = await _internRepository.GetAllIncludingInactiveAsync();

        var result = interns.Select(ToResponseDto).ToList();

        return ServiceResult<List<InternResponseDto>>.Ok(result);
    }

    public async Task<ServiceResult<InternResponseDto>> GetByIdAsync(int id, int userId, string role)
    {
        var intern = await _internRepository.GetByIdAsync(id);

        if (intern == null)
        {
            _logger.LogWarning("Intern was not found. InternId: {InternId}, UserId: {UserId}", id, userId);

            return ServiceResult<InternResponseDto>.NotFound("Stajyer bulunamadı.");
        }

        if (RoleHelper.IsInternClaim(role))
        {
            var currentIntern = await _internRepository.GetByUserIdAsync(userId);

            if (currentIntern == null)
            {
                _logger.LogWarning(
                    "Intern profile access failed because current intern profile was not found. RequestedInternId: {RequestedInternId}, UserId: {UserId}",
                    id,
                    userId);

                return ServiceResult<InternResponseDto>.NotFound("Stajyer profili bulunamadı.");
            }

            if (currentIntern.Id != intern.Id)
            {
                _logger.LogWarning(
                    "Unauthorized intern profile access attempt. RequestedInternId: {RequestedInternId}, CurrentInternId: {CurrentInternId}, UserId: {UserId}",
                    id,
                    currentIntern.Id,
                    userId);

                return ServiceResult<InternResponseDto>.Forbidden("Bu stajyer profiline erişim yetkiniz yok.");
            }
        }

        var result = ToResponseDto(intern);

        return ServiceResult<InternResponseDto>.Ok(result);
    }

    public async Task<ServiceResult> CreateInternWithAccountAsync(CreateInternDto dto)
    {
        var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);

        if (department == null)
        {
            _logger.LogWarning(
                "Intern creation rejected because department was not found. DepartmentId: {DepartmentId}",
                dto.DepartmentId);

            return ServiceResult.ValidationError("Departman bulunamadı.");
        }

        var normalizedEmail = dto.Email.Trim();

        var userEmailExists = await _userRepository.EmailExistsAsync(normalizedEmail);

        if (userEmailExists)
        {
            _logger.LogWarning("Intern creation rejected because user email already exists.");

            return ServiceResult.Conflict("Bu email adresi zaten kayıtlı.");
        }

        var internEmailExists = await _internRepository.EmailExistsAsync(normalizedEmail);

        if (internEmailExists)
        {
            _logger.LogWarning("Intern creation rejected because intern email already exists.");

            return ServiceResult.Conflict("Bu email adresine ait stajyer kaydı zaten mevcut.");
        }

        var user = new User
        {
            Name = dto.Name.Trim(),

            Surname = dto.Surname.Trim(),

            Email = normalizedEmail,

            PasswordHash = PasswordHasher.Hash(dto.Password),

            Role = Roles.Intern,

            MustChangePassword = true
        };

        var intern = new Intern
        {
            Name = dto.Name.Trim(),

            Surname = dto.Surname.Trim(),

            Email = normalizedEmail,

            DepartmentId = dto.DepartmentId,

            User = user
        };

        user.Intern = intern;

        await _userRepository.AddAsync(user);

        _logger.LogInformation(
            "Intern and user account created successfully. InternId: {InternId}, UserId: {UserId}, DepartmentId: {DepartmentId}",
            intern.Id,
            user.Id,
            intern.DepartmentId);

        return ServiceResult.Ok("Stajyer ve kullanıcı hesabı başarıyla oluşturuldu.");
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateInternDto dto)
    {
        var intern = await _internRepository.GetByIdAsync(id);

        if (intern == null)
        {
            _logger.LogWarning("Intern update failed because intern was not found. InternId: {InternId}", id);

            return ServiceResult.NotFound("Stajyer bulunamadı.");
        }

        var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);

        if (department == null)
        {
            _logger.LogWarning(
                "Intern update rejected because department was not found. InternId: {InternId}, DepartmentId: {DepartmentId}",
                id,
                dto.DepartmentId);

            return ServiceResult.ValidationError("Departman bulunamadı.");
        }

        var user = await _userRepository.GetByIdAsync(intern.UserId);

        if (user == null)
        {
            _logger.LogWarning(
                "Intern update failed because linked user account was not found. InternId: {InternId}, UserId: {UserId}",
                id,
                intern.UserId);

            return ServiceResult.NotFound("Stajyere bağlı kullanıcı hesabı bulunamadı.");
        }

        var normalizedNewEmail = dto.Email.Trim();

        var emailChanged = !user.Email.Equals(normalizedNewEmail, StringComparison.OrdinalIgnoreCase);

        if (emailChanged)
        {
            var userEmailExists = await _userRepository.EmailExistsAsync(normalizedNewEmail);

            if (userEmailExists)
            {
                _logger.LogWarning(
                    "Intern update rejected because email is already used by another user. InternId: {InternId}",
                    id);

                return ServiceResult.Conflict("Bu email adresi başka bir kullanıcı tarafından kullanılıyor.");
            }

            var internEmailExists = await _internRepository.EmailExistsAsync(normalizedNewEmail);

            if (internEmailExists)
            {
                _logger.LogWarning(
                    "Intern update rejected because email is already used by another intern. InternId: {InternId}",
                    id);

                return ServiceResult.Conflict("Bu email adresi başka bir stajyer tarafından kullanılıyor.");
            }
        }

        intern.Name = dto.Name.Trim();

        intern.Surname = dto.Surname.Trim();

        intern.Email = normalizedNewEmail;

        intern.DepartmentId = dto.DepartmentId;

        user.Name = dto.Name.Trim();

        user.Surname = dto.Surname.Trim();

        user.Email = normalizedNewEmail;

        await _internRepository.UpdateAsync(intern);

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation(
            "Intern updated successfully. InternId: {InternId}, UserId: {UserId}, DepartmentId: {DepartmentId}",
            intern.Id,
            user.Id,
            intern.DepartmentId);

        return ServiceResult.Ok("Stajyer bilgileri başarıyla güncellendi.");
    }

    public async Task<ServiceResult> DeactivateInternAsync(int id)
    {
        var intern = await _internRepository.GetByIdAsync(id);

        if (intern == null)
        {
            _logger.LogWarning("Intern deletion failed because intern was not found. InternId: {InternId}", id);

            return ServiceResult.NotFound("Stajyer bulunamadı.");
        }

        await _internRepository.DeactivateInternAsync(intern);

        _logger.LogInformation(
            "Intern soft deleted successfully. InternId: {InternId}, UserId: {UserId}",
            intern.Id,
            intern.UserId);

        return ServiceResult.Ok("Stajyer pasif hale getirildi.");
    }

    public async Task<ServiceResult> ReactivateInternAsync(int id)
    {
        var intern = await _internRepository.GetByIdIncludingInactiveAsync(id);

        if (intern == null)
        {
            _logger.LogWarning("Intern restore failed because intern was not found. InternId: {InternId}", id);

            return ServiceResult.NotFound("Stajyer bulunamadı.");
        }

        if (intern.IsActive)
        {
            _logger.LogWarning("Intern restore rejected because intern is already active. InternId: {InternId}", id);

            return ServiceResult.Conflict("Stajyer zaten aktif.");
        }

        var department = await _departmentRepository.GetByIdAsync(intern.DepartmentId);

        if (department == null)
        {
            _logger.LogWarning(
                "Intern restore rejected because linked department is inactive or unavailable. InternId: {InternId}, DepartmentId: {DepartmentId}",
                intern.Id,
                intern.DepartmentId);

            return ServiceResult.Conflict(
                "Stajyerin bağlı olduğu departman pasif veya bulunamadı. Önce departmanı aktif hale getirin.");
        }

        await _internRepository.ReactivateInternAsync(intern);

        _logger.LogInformation(
            "Intern restored successfully. InternId: {InternId}, DepartmentId: {DepartmentId}",
            intern.Id,
            intern.DepartmentId);

        return ServiceResult.Ok("Stajyer tekrar aktif hale getirildi.");
    }
}