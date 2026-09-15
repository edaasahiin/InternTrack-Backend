using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using Microsoft.Extensions.Logging;

namespace InternTrack.Business.Services;

public class DepartmentService :
    IDepartmentService
{
    private readonly IDepartmentRepository
        _departmentRepository;

    private readonly IInternRepository
        _internRepository;

    private readonly ILogger<DepartmentService>?
        _logger;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        IInternRepository internRepository,
        ILogger<DepartmentService>? logger = null)
    {
        _departmentRepository =
            departmentRepository;

        _internRepository =
            internRepository;

        _logger =
            logger;
    }

    public async Task<List<Department>> GetAllAsync()
    {
        return await _departmentRepository
            .GetAllAsync();
    }

    public async Task<ServiceResult<Department>>
        GetByIdAsync(
            int id)
    {
        var department =
            await _departmentRepository
                .GetByIdAsync(id);

        if (department == null)
        {
            _logger?.LogWarning(
                "Department was not found. DepartmentId: {DepartmentId}",
                id
            );

            return ServiceResult<Department>
                .NotFound(
                    "Departman bulunamadı."
                );
        }

        return ServiceResult<Department>
            .Ok(department);
    }

    public async Task<ServiceResult> AddAsync(
        CreateDepartmentDto dto)
    {
        var departmentName =
            dto.Name.Trim();

        if (
            string.IsNullOrWhiteSpace(
                departmentName
            )
        )
        {
            _logger?.LogWarning(
                "Department creation rejected because department name is empty."
            );

            return ServiceResult
                .ValidationError(
                    "Departman adı boş bırakılamaz."
                );
        }

        var nameExists =
            await _departmentRepository
                .NameExistsAsync(
                    departmentName
                );

        if (nameExists)
        {
            _logger?.LogWarning(
                "Department creation rejected because department name already exists. DepartmentName: {DepartmentName}",
                departmentName
            );

            return ServiceResult
                .Conflict(
                    "Bu departman zaten kayıtlı."
                );
        }

        var department =
            new Department
            {
                Name =
                    departmentName
            };

        await _departmentRepository
            .AddAsync(
                department
            );

        _logger?.LogInformation(
            "Department created successfully. DepartmentId: {DepartmentId}, DepartmentName: {DepartmentName}",
            department.Id,
            department.Name
        );

        return ServiceResult.Ok(
            "Departman oluşturuldu."
        );
    }

    public async Task<ServiceResult> UpdateAsync(
        int id,
        CreateDepartmentDto dto)
    {
        var department =
            await _departmentRepository
                .GetByIdAsync(
                    id
                );

        if (department == null)
        {
            _logger?.LogWarning(
                "Department update failed because department was not found. DepartmentId: {DepartmentId}",
                id
            );

            return ServiceResult
                .NotFound(
                    "Departman bulunamadı."
                );
        }

        var departmentName =
            dto.Name.Trim();

        if (
            string.IsNullOrWhiteSpace(
                departmentName
            )
        )
        {
            _logger?.LogWarning(
                "Department update rejected because department name is empty. DepartmentId: {DepartmentId}",
                id
            );

            return ServiceResult
                .ValidationError(
                    "Departman adı boş bırakılamaz."
                );
        }

        var nameExists =
            await _departmentRepository
                .NameExistsAsync(
                    departmentName,
                    id
                );

        if (nameExists)
        {
            _logger?.LogWarning(
                "Department update rejected because department name already exists. DepartmentId: {DepartmentId}, DepartmentName: {DepartmentName}",
                id,
                departmentName
            );

            return ServiceResult
                .Conflict(
                    "Bu departman zaten kayıtlı."
                );
        }

        department.Name =
            departmentName;

        await _departmentRepository
            .UpdateAsync(
                department
            );

        _logger?.LogInformation(
            "Department updated successfully. DepartmentId: {DepartmentId}, DepartmentName: {DepartmentName}",
            department.Id,
            department.Name
        );

        return ServiceResult.Ok(
            "Departman güncellendi."
        );
    }

    public async Task<ServiceResult> DeleteAsync(
        int id)
    {
        var department =
            await _departmentRepository
                .GetByIdAsync(
                    id
                );

        if (department == null)
        {
            _logger?.LogWarning(
                "Department deletion failed because department was not found. DepartmentId: {DepartmentId}",
                id
            );

            return ServiceResult
                .NotFound(
                    "Departman bulunamadı."
                );
        }

        var hasInterns =
            await _internRepository
                .ExistsByDepartmentIdAsync(
                    id
                );

        if (hasInterns)
        {
            _logger?.LogWarning(
                "Department deletion rejected because department still has interns. DepartmentId: {DepartmentId}",
                id
            );

            return ServiceResult
                .Conflict(
                    "Bu departmana bağlı stajyerler olduğu için departman silinemez."
                );
        }

        await _departmentRepository
            .DeleteAsync(
                department
            );

        _logger?.LogInformation(
            "Department deleted successfully. DepartmentId: {DepartmentId}, DepartmentName: {DepartmentName}",
            department.Id,
            department.Name
        );

        return ServiceResult.Ok(
            "Departman silindi."
        );
    }
}