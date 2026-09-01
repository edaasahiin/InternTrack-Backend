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

    public InternService(
        IInternRepository internRepository,
        IDepartmentRepository departmentRepository)
    {
        _internRepository = internRepository;
        _departmentRepository = departmentRepository;
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

        var emailExists =
            await _internRepository.EmailExistsAsync(
                dto.Email.Trim()
            );

        if (emailExists)
        {
            return ServiceResult.Conflict(
                "Bu email adresi zaten kayıtlı."
            );
        }

        var intern = new Intern
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            DepartmentId = dto.DepartmentId
        };

        await _internRepository.AddAsync(
            intern
        );

        return ServiceResult.Ok(
            "Stajyer eklendi."
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