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
            Role = "Intern"
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

        await _internRepository.DeleteAsync(
            intern
        );

        return ServiceResult.Ok(
            "Stajyer silindi."
        );
    }
}