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

    public async Task<string> AddAsync(CreateDepartmentDto dto)
    {
        var departmentName = dto.Name.Trim();

        var nameExists = await _departmentRepository.NameExistsAsync(departmentName);

        if (nameExists)
        {
            return "Bu departman zaten kayıtlı.";
        }

        var department = new Department
        {
            Name = departmentName
        };

        await _departmentRepository.AddAsync(department);

        return "Departman oluşturuldu.";
    }

    public async Task<string> DeleteAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);

        if (department == null)
        {
            return "Departman bulunamadı.";
        }

        var hasInterns = await _internRepository.ExistsByDepartmentIdAsync(id);

        if (hasInterns)
        {
            return "Bu departmana bağlı stajyerler olduğu için departman silinemez.";
        }

        await _departmentRepository.DeleteAsync(department);

        return "Departman silindi.";
    }
}