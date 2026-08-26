using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;

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

    public async Task<ServiceResult<Department>> GetByIdAsync(int id)
    {
        var department =
            await _departmentRepository.GetByIdAsync(id);

        if (department == null)
        {
            return ServiceResult<Department>.NotFound(
                "Departman bulunamadı."
            );
        }

        return ServiceResult<Department>.Ok(department);
    }

    public async Task<ServiceResult> AddAsync(CreateDepartmentDto dto)
    {
        var departmentName = dto.Name.Trim();

        var nameExists =
            await _departmentRepository.NameExistsAsync(departmentName);

        if (nameExists)
        {
            return ServiceResult.Conflict(
                "Bu departman zaten kayıtlı."
            );
        }

        var department = new Department
        {
            Name = departmentName
        };

        await _departmentRepository.AddAsync(department);

        return ServiceResult.Ok(
            "Departman oluşturuldu."
        );
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var department =
            await _departmentRepository.GetByIdAsync(id);

        if (department == null)
        {
            return ServiceResult.NotFound(
                "Departman bulunamadı."
            );
        }

        var hasInterns =
            await _internRepository.ExistsByDepartmentIdAsync(id);

        if (hasInterns)
        {
            return ServiceResult.Conflict(
                "Bu departmana bağlı stajyerler olduğu için departman silinemez."
            );
        }

        await _departmentRepository.DeleteAsync(department);

        return ServiceResult.Ok(
            "Departman silindi."
        );
    }
}
