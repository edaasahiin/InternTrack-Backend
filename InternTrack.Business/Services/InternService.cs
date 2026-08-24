using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Entities.DTOs;
using InternTrack.Entities.Models;

namespace InternTrack.Business.Services;

public class InternService : IInternService
{
    private readonly IInternRepository _repository;

    public InternService(IInternRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Intern>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Intern?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<string> AddAsync(CreateInternDto dto)
    {
        var interns = await _repository.GetAllAsync();

        var existing = interns
            .FirstOrDefault(x => x.Email == dto.Email);

        if (existing != null)
        {
            return "Bu email adresi zaten kayıtlı.";
        }

        var intern = new Intern
        {
            Name = dto.Name,
            Email = dto.Email,
            DepartmentId = dto.DepartmentId
        };

        await _repository.AddAsync(intern);

        return "Stajyer eklendi.";
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var intern = await _repository.GetByIdAsync(id);

        if (intern == null)
        {
            return false;
        }

        await _repository.DeleteAsync(intern);

        return true;
    }
}