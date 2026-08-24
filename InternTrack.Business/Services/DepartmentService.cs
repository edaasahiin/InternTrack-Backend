using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Entities.Models;

namespace InternTrack.Business.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;

    public DepartmentService(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Department>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddAsync(Department department)
    {
        await _repository.AddAsync(department);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _repository.GetByIdAsync(id);

        if (department == null)
        {
            return false;
        }

        await _repository.DeleteAsync(department);

        return true;
    }
}