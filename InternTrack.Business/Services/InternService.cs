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

    public InternService(
        IInternRepository internRepository,
        IDepartmentRepository departmentRepository,
        IUserRepository userRepository)
    {
        _internRepository = internRepository;
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
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
                return ServiceResult<Intern>.NotFound(
                    "Stajyer profili bulunamadı."
                );
            }

            if (currentIntern.Id != intern.Id)
            {
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
            return ServiceResult.ValidationError(
                "Departman bulunamadı."
            );
        }

        var userEmailExists =
            await _userRepository.EmailExistsAsync(
                dto.Email.Trim()
            );

        if (userEmailExists)
        {
            return ServiceResult.Conflict(
                "Bu email adresi zaten kayıtlı."
            );
        }

        var internEmailExists =
            await _internRepository.EmailExistsAsync(
                dto.Email.Trim()
            );

        if (internEmailExists)
        {
            return ServiceResult.Conflict(
                "Bu email adresine ait stajyer kaydı zaten mevcut."
            );
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Surname = dto.Surname.Trim(),
            Email = dto.Email.Trim(),
            PasswordHash =
                PasswordHasher.Hash(dto.Password),
            Role = "Intern",
            MustChangePassword = true
        };

        var intern = new Intern
        {
            Name = dto.Name.Trim(),
            Surname = dto.Surname.Trim(),
            Email = dto.Email.Trim(),
            DepartmentId = dto.DepartmentId,
            User = user
        };

        user.Intern = intern;

        await _userRepository.AddAsync(user);

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
            return ServiceResult.NotFound(
                "Stajyere bağlı kullanıcı hesabı bulunamadı."
            );
        }

        await _userRepository.DeleteAsync(
            user
        );

        return ServiceResult.Ok(
            "Stajyer ve kullanıcı hesabı başarıyla silindi."
        );
    }
}