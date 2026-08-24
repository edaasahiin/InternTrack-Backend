using InternTrack.DataAccess.Repositories;
using InternTrack.Entities.Models;

namespace InternTrack.Business.Services;

public class InternService
{
    private readonly InternRepository _repository;

    public InternService(InternRepository repository)
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

    public async Task<string> AddAsync(Intern intern)
    {
        var interns = await _repository.GetAllAsync();

        var existing = interns
            .FirstOrDefault(x => x.Email == intern.Email);

        if (existing != null)
        {
            return "Bu email adresi zaten kayıtlı.";
        }

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