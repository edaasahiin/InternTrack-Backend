using InternTrack.DataAccess.Repositories;
using InternTrack.Entities.Models;

namespace InternTrack.Business.Services;

public class DepartmentService
{
    private readonly DepartmentRepository _repository;

    public DepartmentService(DepartmentRepository repository)
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