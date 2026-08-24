using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Entities.DTOs;
using InternTrack.Entities.Models;

namespace InternTrack.Business.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IInternRepository _internRepository;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        IInternRepository internRepository)
    {
        _departmentRepository = departmentRepository;
        _internRepository = internRepository;
    }

    public async Task<List<Department>> GetAllAsync()
    {
        return await _departmentRepository.GetAllAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _departmentRepository.GetByIdAsync(id);
    }

    public async Task AddAsync(CreateDepartmentDto dto)
    {
        var department = new Department
        {
            Name = dto.Name.Trim()
        };

        await _departmentRepository.AddAsync(department);
    }

    public async Task<string> DeleteAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);

        if (department == null)
        {
            return "Departman bulunamadı.";
        }

        var interns = await _internRepository.GetAllAsync();

        var hasInterns = interns.Any(x => x.DepartmentId == id);

        if (hasInterns)
        {
            return "Bu departmana bağlı stajyerler olduğu için departman silinemez.";
        }

        await _departmentRepository.DeleteAsync(department);

        return "Departman silindi.";
    }
}