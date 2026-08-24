using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Entities.DTOs;
using InternTrack.Entities.Models;

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

    public async Task<List<Intern>> GetAllAsync()
    {
        return await _internRepository.GetAllAsync();
    }

    public async Task<Intern?> GetByIdAsync(int id)
    {
        return await _internRepository.GetByIdAsync(id);
    }

    public async Task<string> AddAsync(CreateInternDto dto)
    {
        var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);

        if (department == null)
        {
            return "Departman bulunamadı.";
        }

        var interns = await _internRepository.GetAllAsync();

        var existing = interns
            .FirstOrDefault(x => x.Email == dto.Email);

        if (existing != null)
        {
            return "Bu email adresi zaten kayıtlı.";
        }

        var intern = new Intern
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            DepartmentId = dto.DepartmentId
        };

        await _internRepository.AddAsync(intern);

        return "Stajyer eklendi.";
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var intern = await _internRepository.GetByIdAsync(id);

        if (intern == null)
        {
            return false;
        }

        await _internRepository.DeleteAsync(intern);

        return true;
    }
}