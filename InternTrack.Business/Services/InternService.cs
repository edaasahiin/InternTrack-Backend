using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using Microsoft.Extensions.Logging;

namespace InternTrack.Business.Services;

public class InternService : IInternService
{
    private readonly IInternRepository _internRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<InternService>? _logger;

    public InternService(
        IInternRepository internRepository,
        IDepartmentRepository departmentRepository,
        IUserRepository userRepository,
        ILogger<InternService>? logger = null)
    {
        _internRepository = internRepository;
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<List<Intern>>> GetAllAsync(
        int userId,
        string role)
    {
        if (role == "Admin" || role == "HR")
        {
            var interns =
                await _internRepository.GetAllAsync();

            return ServiceResult<List<Intern>>
                .Ok(interns);
        }

        var intern =
            await _internRepository.GetByUserIdAsync(
                userId
            );

        if (intern == null)
        {
            _logger?.LogWarning(
                "Intern list could not be retrieved because intern profile was not found. UserId: {UserId}",
                userId
            );

            return ServiceResult<List<Intern>>
                .NotFound(
                    "Stajyer profili bulunamadı."
                );
        }

        var result = new List<Intern>
        {
            intern
        };

        return ServiceResult<List<Intern>>
            .Ok(result);
    }

    public async Task<ServiceResult<Intern>> GetByIdAsync(
        int id,
        int userId,
        string role)
    {
        var intern =
            await _internRepository.GetByIdAsync(id);

        if (intern == null)
        {
            _logger?.LogWarning(
                "Intern was not found. InternId: {InternId}, UserId: {UserId}",
                id,
                userId
            );

            return ServiceResult<Intern>.NotFound(
                "Stajyer bulunamadı."
            );
        }

        if (role == "Intern")
        {
            var currentIntern =
                await _internRepository.GetByUserIdAsync(
                    userId
                );

            if (currentIntern == null)
            {
                _logger?.LogWarning(
                    "Intern profile access failed because current intern profile was not found. RequestedInternId: {RequestedInternId}, UserId: {UserId}",
                    id,
                    userId
                );

                return ServiceResult<Intern>.NotFound(
                    "Stajyer profili bulunamadı."
                );
            }

            if (currentIntern.Id != intern.Id)
            {
                _logger?.LogWarning(
                    "Unauthorized intern profile access attempt. RequestedInternId: {RequestedInternId}, CurrentInternId: {CurrentInternId}, UserId: {UserId}",
                    id,
                    currentIntern.Id,
                    userId
                );

                return ServiceResult<Intern>.Forbidden(
                    "Bu stajyer profiline erişim yetkiniz yok."
                );
            }
        }

        return ServiceResult<Intern>.Ok(
            intern
        );
    }

    public async Task<ServiceResult> AddAsync(
        CreateInternDto dto)
    {
        var department =
            await _departmentRepository.GetByIdAsync(
                dto.DepartmentId
            );

        if (department == null)
        {
            _logger?.LogWarning(
                "Intern creation rejected because department was not found. DepartmentId: {DepartmentId}",
                dto.DepartmentId
            );

            return ServiceResult.ValidationError(
                "Departman bulunamadı."
            );
        }

        var normalizedEmail =
            dto.Email.Trim();

        var userEmailExists =
            await _userRepository.EmailExistsAsync(
                normalizedEmail
            );

        if (userEmailExists)
        {
            _logger?.LogWarning(
                "Intern creation rejected because user email already exists."
            );

            return ServiceResult.Conflict(
                "Bu email adresi zaten kayıtlı."
            );
        }

        var internEmailExists =
            await _internRepository.EmailExistsAsync(
                normalizedEmail
            );

        if (internEmailExists)
        {
            _logger?.LogWarning(
                "Intern creation rejected because intern email already exists."
            );

            return ServiceResult.Conflict(
                "Bu email adresine ait stajyer kaydı zaten mevcut."
            );
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Surname = dto.Surname.Trim(),
            Email = normalizedEmail,
            PasswordHash =
                PasswordHasher.Hash(dto.Password),
            Role = "Intern",
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

        _logger?.LogInformation(
            "Intern and user account created successfully. InternId: {InternId}, UserId: {UserId}, DepartmentId: {DepartmentId}",
            intern.Id,
            user.Id,
            intern.DepartmentId
        );

        return ServiceResult.Ok(
            "Stajyer ve kullanıcı hesabı başarıyla oluşturuldu."
        );
    }

    public async Task<ServiceResult> UpdateAsync(
        int id,
        UpdateInternDto dto)
    {
        var intern =
            await _internRepository.GetByIdAsync(
                id
            );

        if (intern == null)
        {
            _logger?.LogWarning(
                "Intern update failed because intern was not found. InternId: {InternId}",
                id
            );

            return ServiceResult.NotFound(
                "Stajyer bulunamadı."
            );
        }

        var department =
            await _departmentRepository.GetByIdAsync(
                dto.DepartmentId
            );

        if (department == null)
        {
            _logger?.LogWarning(
                "Intern update rejected because department was not found. InternId: {InternId}, DepartmentId: {DepartmentId}",
                id,
                dto.DepartmentId
            );

            return ServiceResult.ValidationError(
                "Departman bulunamadı."
            );
        }

        var user =
            await _userRepository.GetByIdAsync(
                intern.UserId
            );

        if (user == null)
        {
            _logger?.LogWarning(
                "Intern update failed because linked user account was not found. InternId: {InternId}, UserId: {UserId}",
                id,
                intern.UserId
            );

            return ServiceResult.NotFound(
                "Stajyere bağlı kullanıcı hesabı bulunamadı."
            );
        }

        var normalizedNewEmail =
            dto.Email.Trim();

        var emailChanged =
            !user.Email.Equals(
                normalizedNewEmail,
                StringComparison.OrdinalIgnoreCase
            );

        if (emailChanged)
        {
            var userEmailExists =
                await _userRepository.EmailExistsAsync(
                    normalizedNewEmail
                );

            if (userEmailExists)
            {
                _logger?.LogWarning(
                    "Intern update rejected because email is already used by another user. InternId: {InternId}",
                    id
                );

                return ServiceResult.Conflict(
                    "Bu email adresi başka bir kullanıcı tarafından kullanılıyor."
                );
            }

            var internEmailExists =
                await _internRepository.EmailExistsAsync(
                    normalizedNewEmail
                );

            if (internEmailExists)
            {
                _logger?.LogWarning(
                    "Intern update rejected because email is already used by another intern. InternId: {InternId}",
                    id
                );

                return ServiceResult.Conflict(
                    "Bu email adresi başka bir stajyer tarafından kullanılıyor."
                );
            }
        }

        intern.Name =
            dto.Name.Trim();

        intern.Surname =
            dto.Surname.Trim();

        intern.Email =
            normalizedNewEmail;

        intern.DepartmentId =
            dto.DepartmentId;

        user.Name =
            dto.Name.Trim();

        user.Surname =
            dto.Surname.Trim();

        user.Email =
            normalizedNewEmail;

        await _internRepository.UpdateAsync(
            intern
        );

        await _userRepository.UpdateAsync(
            user
        );

        _logger?.LogInformation(
            "Intern updated successfully. InternId: {InternId}, UserId: {UserId}, DepartmentId: {DepartmentId}",
            intern.Id,
            user.Id,
            intern.DepartmentId
        );

        return ServiceResult.Ok(
            "Stajyer bilgileri başarıyla güncellendi."
        );
    }

    public async Task<ServiceResult> DeleteAsync(
        int id)
    {
        var intern =
            await _internRepository.GetByIdAsync(id);

        if (intern == null)
        {
            _logger?.LogWarning(
                "Intern deletion failed because intern was not found. InternId: {InternId}",
                id
            );

            return ServiceResult.NotFound(
                "Stajyer bulunamadı."
            );
        }

        var user =
            await _userRepository.GetByIdAsync(
                intern.UserId
            );

        if (user == null)
        {
            _logger?.LogWarning(
                "Intern deletion failed because linked user account was not found. InternId: {InternId}, UserId: {UserId}",
                id,
                intern.UserId
            );

            return ServiceResult.NotFound(
                "Stajyere bağlı kullanıcı hesabı bulunamadı."
            );
        }

        await _userRepository.DeleteAsync(
            user
        );

        _logger?.LogInformation(
            "Intern and linked user account deleted successfully. InternId: {InternId}, UserId: {UserId}",
            intern.Id,
            user.Id
        );

        return ServiceResult.Ok(
            "Stajyer ve kullanıcı hesabı başarıyla silindi."
        );
    }
}