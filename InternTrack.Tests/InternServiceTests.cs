using InternTrack.Business.Interfaces;
using InternTrack.Business.Common;
using InternTrack.Business.Services;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Constants;
using Moq;

namespace InternTrack.Tests;

public class InternServiceTests
{
    [Fact]
    public async Task DeactivateInternAsync_InternDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 99;

        internRepositoryMock.Setup(repository => repository.GetByIdAsync(internId)).ReturnsAsync((Intern? )null);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());
        // ACT
        var result = await service.DeactivateInternAsync(internId);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.NotFound, result.Type);

        Assert.Equal("Stajyer bulunamadı.", result.Message);

        internRepositoryMock.Verify(repository => repository.DeactivateInternAsync(It.IsAny<Intern>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateInternAsync_InternExists_ShouldDeactivateSuccessfully()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 1;

        var intern = new Intern
        {
            Id = internId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            DepartmentId = 1,
            UserId = 10,
            IsActive = true
        };

        internRepositoryMock.Setup(repository => repository.GetByIdAsync(internId)).ReturnsAsync(intern);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());
        // ACT
        var result = await service.DeactivateInternAsync(internId);
        // ASSERT
        Assert.True(result.Success);

        Assert.Equal(ResultType.Success, result.Type);

        Assert.Equal("Stajyer pasif hale getirildi.", result.Message);

        internRepositoryMock.Verify(repository => repository.DeactivateInternAsync(intern), Times.Once);

        userRepositoryMock.Verify(repository => repository.DeleteAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ReactivateInternAsync_InternDoesNotExist_ShouldReturnNotFound()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 99;

        internRepositoryMock.Setup(
            repository => repository.GetByIdIncludingInactiveAsync(internId)).ReturnsAsync(
            (Intern? )null);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());
        // ACT
        var result = await service.ReactivateInternAsync(internId);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.NotFound, result.Type);

        Assert.Equal("Stajyer bulunamadı.", result.Message);

        internRepositoryMock.Verify(repository => repository.ReactivateInternAsync(It.IsAny<Intern>()), Times.Never);
    }

    [Fact]
    public async Task ReactivateInternAsync_InternAlreadyActive_ShouldReturnConflict()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 1;

        var intern = new Intern
        {
            Id = internId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            DepartmentId = 1,
            UserId = 10,
            IsActive = true
        };

        internRepositoryMock.Setup(
            repository => repository.GetByIdIncludingInactiveAsync(internId)).ReturnsAsync(
            intern);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());
        // ACT
        var result = await service.ReactivateInternAsync(internId);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.Conflict, result.Type);

        Assert.Equal("Stajyer zaten aktif.", result.Message);

        internRepositoryMock.Verify(repository => repository.ReactivateInternAsync(It.IsAny<Intern>()), Times.Never);
    }

    [Fact]
    public async Task ReactivateInternAsync_DepartmentIsInactive_ShouldReturnConflict()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 1;
        var departmentId = 5;

        var intern = new Intern
        {
            Id = internId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            DepartmentId = departmentId,
            UserId = 10,
            IsActive = false
        };

        internRepositoryMock.Setup(
            repository => repository.GetByIdIncludingInactiveAsync(internId)).ReturnsAsync(
            intern);

        departmentRepositoryMock.Setup(
            repository => repository.GetByIdAsync(departmentId)).ReturnsAsync(
            (Department? )null);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());
        // ACT
        var result = await service.ReactivateInternAsync(internId);
        // ASSERT
        Assert.False(result.Success);

        Assert.Equal(ResultType.Conflict, result.Type);

        Assert.Equal(
            "Stajyerin bağlı olduğu departman pasif veya bulunamadı. Önce departmanı aktif hale getirin.",
            result.Message);

        internRepositoryMock.Verify(repository => repository.ReactivateInternAsync(It.IsAny<Intern>()), Times.Never);
    }

    [Fact]
    public async Task ReactivateInternAsync_InactiveInternWithActiveDepartment_ShouldReactivateSuccessfully()
    {
        // ARRANGE
        var internRepositoryMock = new Mock<IInternRepository>();

        var departmentRepositoryMock = new Mock<IDepartmentRepository>();

        var userRepositoryMock = new Mock<IUserRepository>();

        var internId = 1;
        var departmentId = 5;

        var intern = new Intern
        {
            Id = internId,
            Name = "Test",
            Surname = "Intern",
            Email = "test@example.com",
            DepartmentId = departmentId,
            UserId = 10,
            IsActive = false
        };

        var department = new Department
        {
            Id = departmentId,
            Name = "Software Development",
            IsActive = true
        };

        internRepositoryMock.Setup(
            repository => repository.GetByIdIncludingInactiveAsync(internId)).ReturnsAsync(
            intern);

        departmentRepositoryMock.Setup(repository => repository.GetByIdAsync(departmentId)).ReturnsAsync(department);

        var service = new InternService(
            internRepositoryMock.Object,
            departmentRepositoryMock.Object,
            userRepositoryMock.Object,
            logger: Mock.Of<IAppLogger>());
        // ACT
        var result = await service.ReactivateInternAsync(internId);
        // ASSERT
        Assert.True(result.Success);

        Assert.Equal(ResultType.Success, result.Type);

        Assert.Equal("Stajyer tekrar aktif hale getirildi.", result.Message);

        internRepositoryMock.Verify(repository => repository.ReactivateInternAsync(intern), Times.Once);
    }


[Fact]
public async Task UpdateAsync_DepartmentDoesNotExist_ShouldReturnValidationError()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 1;
    var departmentId = 999;

    var intern = new Intern
    {
        Id = internId,
        Name = "Test",
        Surname = "Intern",
        Email = "test@example.com",
        DepartmentId = 1,
        UserId = 10,
        IsActive = true
    };

    var dto = new UpdateInternDto
    {
        Name = "Updated",
        Surname = "Intern",
        Email = "updated@example.com",
        DepartmentId = departmentId,
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync(intern);

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync((Department?)null);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.UpdateAsync(
        internId,
        dto);

    // ASSERT
    Assert.False(result.Success);

    internRepositoryMock.Verify(
        repository => repository.UpdateAsync(It.IsAny<Intern>()),
        Times.Never);
}

[Fact]
public async Task UpdateAsync_InternDoesNotExist_ShouldReturnNotFound()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 999;

    var dto = new UpdateInternDto
    {
        Name = "Updated",
        Surname = "Intern",
        Email = "updated@example.com",
        DepartmentId = 1
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync((Intern?)null);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.UpdateAsync(
        internId,
        dto);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.NotFound, result.Type);

    internRepositoryMock.Verify(
        repository => repository.UpdateAsync(It.IsAny<Intern>()),
        Times.Never);

    departmentRepositoryMock.Verify(
        repository => repository.GetByIdAsync(It.IsAny<int>()),
        Times.Never);
}

[Fact]
public async Task UpdateAsync_ValidIntern_ShouldUpdateSuccessfully()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 1;
    var userId = 10;
    var departmentId = 2;

    var existingIntern = new Intern
    {
        Id = internId,
        Name = "Old",
        Surname = "Name",
        Email = "old@example.com",
        DepartmentId = 1,
        UserId = userId,
        IsActive = true
    };

    var user = new User
    {
        Id = userId,
        Name = "Old",
        Surname = "Name",
        Email = "old@example.com",
        PasswordHash = "dummy-hash",
        Role = "Intern"
    };

    var department = new Department
    {
        Id = departmentId,
        Name = "Human Resources",
        IsActive = true
    };

    var dto = new UpdateInternDto
    {
        Name = "Updated",
        Surname = "Intern",
        Email = "updated@example.com",
        DepartmentId = departmentId
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync(existingIntern);

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(department);

    userRepositoryMock
        .Setup(repository => repository.GetByIdAsync(userId))
        .ReturnsAsync(user);

    userRepositoryMock
        .Setup(repository => repository.EmailExistsAsync(dto.Email))
        .ReturnsAsync(false);

    internRepositoryMock
        .Setup(repository => repository.EmailExistsAsync(dto.Email))
        .ReturnsAsync(false);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.UpdateAsync(
        internId,
        dto);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);

    Assert.Equal(dto.Name, existingIntern.Name);
    Assert.Equal(dto.Surname, existingIntern.Surname);
    Assert.Equal(dto.Email, existingIntern.Email);
    Assert.Equal(dto.DepartmentId, existingIntern.DepartmentId);

    Assert.Equal(dto.Name, user.Name);
    Assert.Equal(dto.Surname, user.Surname);
    Assert.Equal(dto.Email, user.Email);

    internRepositoryMock.Verify(
        repository => repository.UpdateAsync(existingIntern),
        Times.Once);

    userRepositoryMock.Verify(
        repository => repository.UpdateAsync(user),
        Times.Once);
}

[Fact]
public async Task UpdateAsync_EmailAlreadyUsedByAnotherUser_ShouldReturnConflict()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 1;
    var userId = 10;
    var departmentId = 2;

    var existingIntern = new Intern
    {
        Id = internId,
        Name = "Test",
        Surname = "Intern",
        Email = "old@example.com",
        DepartmentId = 1,
        UserId = userId,
        IsActive = true
    };

    var user = new User
    {
        Id = userId,
        Name = "Test",
        Surname = "Intern",
        Email = "old@example.com",
        PasswordHash = "dummy-hash",
        Role = "Intern"
    };

    var department = new Department
    {
        Id = departmentId,
        Name = "Human Resources",
        IsActive = true
    };

    var dto = new UpdateInternDto
    {
        Name = "Test",
        Surname = "Intern",
        Email = "used@example.com",
        DepartmentId = departmentId
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync(existingIntern);

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(department);

    userRepositoryMock
        .Setup(repository => repository.GetByIdAsync(userId))
        .ReturnsAsync(user);

    userRepositoryMock
        .Setup(repository => repository.EmailExistsAsync(dto.Email))
        .ReturnsAsync(true);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.UpdateAsync(
        internId,
        dto);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.Conflict, result.Type);

    Assert.Equal(
        "Bu email adresi başka bir kullanıcı tarafından kullanılıyor.",
        result.Message);

    internRepositoryMock.Verify(
        repository => repository.UpdateAsync(It.IsAny<Intern>()),
        Times.Never);

    userRepositoryMock.Verify(
        repository => repository.UpdateAsync(It.IsAny<User>()),
        Times.Never);
}

[Fact]
public async Task UpdateAsync_EmailAlreadyUsedByAnotherIntern_ShouldReturnConflict()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 1;
    var userId = 10;
    var departmentId = 2;

    var existingIntern = new Intern
    {
        Id = internId,
        Name = "Test",
        Surname = "Intern",
        Email = "old@example.com",
        DepartmentId = 1,
        UserId = userId,
        IsActive = true
    };

    var user = new User
    {
        Id = userId,
        Name = "Test",
        Surname = "Intern",
        Email = "old@example.com",
        PasswordHash = "dummy-hash",
        Role = "Intern"
    };

    var department = new Department
    {
        Id = departmentId,
        Name = "Human Resources",
        IsActive = true
    };

    var dto = new UpdateInternDto
    {
        Name = "Test",
        Surname = "Intern",
        Email = "usedintern@example.com",
        DepartmentId = departmentId
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync(existingIntern);

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(department);

    userRepositoryMock
        .Setup(repository => repository.GetByIdAsync(userId))
        .ReturnsAsync(user);

    userRepositoryMock
        .Setup(repository => repository.EmailExistsAsync(dto.Email))
        .ReturnsAsync(false);

    internRepositoryMock
        .Setup(repository => repository.EmailExistsAsync(dto.Email))
        .ReturnsAsync(true);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.UpdateAsync(
        internId,
        dto);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.Conflict, result.Type);

    Assert.Equal(
        "Bu email adresi başka bir stajyer tarafından kullanılıyor.",
        result.Message);

    internRepositoryMock.Verify(
        repository => repository.UpdateAsync(It.IsAny<Intern>()),
        Times.Never);

    userRepositoryMock.Verify(
        repository => repository.UpdateAsync(It.IsAny<User>()),
        Times.Never);
}

[Fact]
public async Task UpdateAsync_LinkedUserDoesNotExist_ShouldReturnNotFound()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 1;
    var userId = 10;
    var departmentId = 2;

    var existingIntern = new Intern
    {
        Id = internId,
        Name = "Test",
        Surname = "Intern",
        Email = "old@example.com",
        DepartmentId = 1,
        UserId = userId,
        IsActive = true
    };

    var department = new Department
    {
        Id = departmentId,
        Name = "Human Resources",
        IsActive = true
    };

    var dto = new UpdateInternDto
    {
        Name = "Updated",
        Surname = "Intern",
        Email = "updated@example.com",
        DepartmentId = departmentId
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync(existingIntern);

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(department);

    userRepositoryMock
        .Setup(repository => repository.GetByIdAsync(userId))
        .ReturnsAsync((User?)null);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.UpdateAsync(
        internId,
        dto);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.NotFound, result.Type);

    Assert.Equal(
        "Stajyere bağlı kullanıcı hesabı bulunamadı.",
        result.Message);

    internRepositoryMock.Verify(
        repository => repository.UpdateAsync(It.IsAny<Intern>()),
        Times.Never);

    userRepositoryMock.Verify(
        repository => repository.UpdateAsync(It.IsAny<User>()),
        Times.Never);
}

[Fact]
public async Task UpdateAsync_EmailUnchanged_ShouldSkipDuplicateEmailChecksAndUpdateSuccessfully()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 1;
    var userId = 10;
    var departmentId = 2;
    var email = "same@example.com";

    var existingIntern = new Intern
    {
        Id = internId,
        Name = "Old",
        Surname = "Name",
        Email = email,
        DepartmentId = 1,
        UserId = userId,
        IsActive = true
    };

    var user = new User
    {
        Id = userId,
        Name = "Old",
        Surname = "Name",
        Email = email,
        PasswordHash = "dummy-hash",
        Role = "Intern"
    };

    var department = new Department
    {
        Id = departmentId,
        Name = "Human Resources",
        IsActive = true
    };

    var dto = new UpdateInternDto
    {
        Name = "Updated",
        Surname = "Intern",
        Email = email,
        DepartmentId = departmentId
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync(existingIntern);

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(department);

    userRepositoryMock
        .Setup(repository => repository.GetByIdAsync(userId))
        .ReturnsAsync(user);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.UpdateAsync(
        internId,
        dto);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);

    Assert.Equal("Updated", existingIntern.Name);
    Assert.Equal("Intern", existingIntern.Surname);
    Assert.Equal(email, existingIntern.Email);
    Assert.Equal(departmentId, existingIntern.DepartmentId);

    Assert.Equal("Updated", user.Name);
    Assert.Equal("Intern", user.Surname);
    Assert.Equal(email, user.Email);

    userRepositoryMock.Verify(
        repository => repository.EmailExistsAsync(It.IsAny<string>()),
        Times.Never);

    internRepositoryMock.Verify(
        repository => repository.EmailExistsAsync(It.IsAny<string>()),
        Times.Never);

    internRepositoryMock.Verify(
        repository => repository.UpdateAsync(existingIntern),
        Times.Once);

    userRepositoryMock.Verify(
        repository => repository.UpdateAsync(user),
        Times.Once);
}

[Fact]
public async Task UpdateAsync_ValuesWithWhitespace_ShouldTrimAndUpdateSuccessfully()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 1;
    var userId = 10;
    var departmentId = 2;

    var existingIntern = new Intern
    {
        Id = internId,
        Name = "Old",
        Surname = "Name",
        Email = "old@example.com",
        DepartmentId = 1,
        UserId = userId,
        IsActive = true
    };

    var user = new User
    {
        Id = userId,
        Name = "Old",
        Surname = "Name",
        Email = "old@example.com",
        PasswordHash = "dummy-hash",
        Role = "Intern"
    };

    var department = new Department
    {
        Id = departmentId,
        Name = "Human Resources",
        IsActive = true
    };

    var dto = new UpdateInternDto
    {
        Name = "  Updated  ",
        Surname = "  Intern  ",
        Email = "  updated@example.com  ",
        DepartmentId = departmentId
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync(existingIntern);

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(departmentId))
        .ReturnsAsync(department);

    userRepositoryMock
        .Setup(repository => repository.GetByIdAsync(userId))
        .ReturnsAsync(user);

    userRepositoryMock
        .Setup(repository => repository.EmailExistsAsync("updated@example.com"))
        .ReturnsAsync(false);

    internRepositoryMock
        .Setup(repository => repository.EmailExistsAsync("updated@example.com"))
        .ReturnsAsync(false);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.UpdateAsync(
        internId,
        dto);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);

    Assert.Equal("Updated", existingIntern.Name);
    Assert.Equal("Intern", existingIntern.Surname);
    Assert.Equal("updated@example.com", existingIntern.Email);

    Assert.Equal("Updated", user.Name);
    Assert.Equal("Intern", user.Surname);
    Assert.Equal("updated@example.com", user.Email);

    internRepositoryMock.Verify(
        repository => repository.UpdateAsync(existingIntern),
        Times.Once);

    userRepositoryMock.Verify(
        repository => repository.UpdateAsync(user),
        Times.Once);
}

[Fact]
public async Task GetByIdAsync_InternDoesNotExist_ShouldReturnNotFound()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 999;
    var userId = 10;

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync((Intern?)null);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetByIdAsync(
        internId,
        userId,
        Roles.Intern);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.NotFound, result.Type);
    Assert.Equal(
        "Stajyer bulunamadı.",
        result.Message);

    internRepositoryMock.Verify(
        repository => repository.GetByUserIdAsync(It.IsAny<int>()),
        Times.Never);
}

[Fact]
public async Task GetByIdAsync_InternTriesToAccessAnotherIntern_ShouldReturnForbidden()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var requestedInternId = 2;
    var currentUserId = 10;

    var requestedIntern = new Intern
    {
        Id = requestedInternId,
        Name = "Ayşe",
        Surname = "Yılmaz",
        Email = "ayse@example.com",
        DepartmentId = 1,
        UserId = 20,
        IsActive = true
    };

    var currentIntern = new Intern
    {
        Id = 1,
        Name = "Eda",
        Surname = "Şahin",
        Email = "eda@example.com",
        DepartmentId = 1,
        UserId = currentUserId,
        IsActive = true
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(requestedInternId))
        .ReturnsAsync(requestedIntern);

    internRepositoryMock
        .Setup(repository => repository.GetByUserIdAsync(currentUserId))
        .ReturnsAsync(currentIntern);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetByIdAsync(
        requestedInternId,
        currentUserId,
        Roles.Intern);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.Forbidden, result.Type);

    Assert.Equal(
        "Bu stajyer profiline erişim yetkiniz yok.",
        result.Message);
}

[Fact]
public async Task GetByIdAsync_InternAccessesOwnProfile_ShouldReturnSuccessfully()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 1;
    var userId = 10;

    var intern = new Intern
    {
        Id = internId,
        Name = "Eda",
        Surname = "Şahin",
        Email = "eda@example.com",
        DepartmentId = 2,
        UserId = userId,
        IsActive = true
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync(intern);

    internRepositoryMock
        .Setup(repository => repository.GetByUserIdAsync(userId))
        .ReturnsAsync(intern);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetByIdAsync(
        internId,
        userId,
        Roles.Intern);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);
    Assert.NotNull(result.Data);

    Assert.Equal(internId, result.Data.Id);
    Assert.Equal("Eda", result.Data.Name);
    Assert.Equal("Şahin", result.Data.Surname);
    Assert.Equal("eda@example.com", result.Data.Email);
    Assert.Equal(2, result.Data.DepartmentId);
    Assert.True(result.Data.IsActive);

    internRepositoryMock.Verify(
        repository => repository.GetByUserIdAsync(userId),
        Times.Once);
}

[Fact]
public async Task GetByIdAsync_CurrentInternProfileDoesNotExist_ShouldReturnNotFound()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var requestedInternId = 2;
    var currentUserId = 999;

    var requestedIntern = new Intern
    {
        Id = requestedInternId,
        Name = "Ayşe",
        Surname = "Yılmaz",
        Email = "ayse@example.com",
        DepartmentId = 1,
        UserId = 20,
        IsActive = true
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(requestedInternId))
        .ReturnsAsync(requestedIntern);

    internRepositoryMock
        .Setup(repository => repository.GetByUserIdAsync(currentUserId))
        .ReturnsAsync((Intern?)null);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetByIdAsync(
        requestedInternId,
        currentUserId,
        Roles.Intern);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.NotFound, result.Type);

    Assert.Equal(
        "Stajyer profili bulunamadı.",
        result.Message);
}

[Fact]
public async Task GetByIdAsync_Admin_ShouldReturnInternWithoutCurrentInternCheck()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 2;
    var adminUserId = 100;

    var intern = new Intern
    {
        Id = internId,
        Name = "Ayşe",
        Surname = "Yılmaz",
        Email = "ayse@example.com",
        DepartmentId = 1,
        UserId = 20,
        IsActive = true
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync(intern);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetByIdAsync(
        internId,
        adminUserId,
        Roles.Admin);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);
    Assert.NotNull(result.Data);

    Assert.Equal(internId, result.Data.Id);
    Assert.Equal("Ayşe", result.Data.Name);
    Assert.Equal("Yılmaz", result.Data.Surname);

    internRepositoryMock.Verify(
        repository => repository.GetByUserIdAsync(It.IsAny<int>()),
        Times.Never);
}

[Fact]
public async Task GetAllAsync_Admin_ShouldReturnAllInterns()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var interns = new List<Intern>
    {
        new()
        {
            Id = 1,
            Name = "Eda",
            Surname = "Şahin",
            Email = "eda@example.com",
            DepartmentId = 1,
            UserId = 10,
            IsActive = true
        },
        new()
        {
            Id = 2,
            Name = "Ayşe",
            Surname = "Yılmaz",
            Email = "ayse@example.com",
            DepartmentId = 2,
            UserId = 20,
            IsActive = true
        }
    };

    internRepositoryMock
        .Setup(repository => repository.GetAllAsync())
        .ReturnsAsync(interns);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetAllAsync(
        100,
        Roles.Admin);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);
    Assert.NotNull(result.Data);
    Assert.Equal(2, result.Data.Count);

    Assert.Equal("Eda", result.Data[0].Name);
    Assert.Equal("Ayşe", result.Data[1].Name);

    internRepositoryMock.Verify(
        repository => repository.GetAllAsync(),
        Times.Once);

    internRepositoryMock.Verify(
        repository => repository.GetByUserIdAsync(It.IsAny<int>()),
        Times.Never);
}

[Fact]
public async Task GetAllAsync_Intern_ShouldReturnOnlyOwnProfile()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var userId = 10;

    var intern = new Intern
    {
        Id = 1,
        Name = "Eda",
        Surname = "Şahin",
        Email = "eda@example.com",
        DepartmentId = 1,
        UserId = userId,
        IsActive = true
    };

    internRepositoryMock
        .Setup(repository => repository.GetByUserIdAsync(userId))
        .ReturnsAsync(intern);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetAllAsync(
        userId,
        Roles.Intern);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);
    Assert.NotNull(result.Data);

    Assert.Single(result.Data);

    Assert.Equal(1, result.Data[0].Id);
    Assert.Equal("Eda", result.Data[0].Name);
    Assert.Equal("Şahin", result.Data[0].Surname);
    Assert.Equal("eda@example.com", result.Data[0].Email);

    internRepositoryMock.Verify(
        repository => repository.GetByUserIdAsync(userId),
        Times.Once);

    internRepositoryMock.Verify(
        repository => repository.GetAllAsync(),
        Times.Never);
}

[Fact]
public async Task GetAllAsync_InternProfileDoesNotExist_ShouldReturnNotFound()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var userId = 999;

    internRepositoryMock
        .Setup(repository => repository.GetByUserIdAsync(userId))
        .ReturnsAsync((Intern?)null);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetAllAsync(
        userId,
        Roles.Intern);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.NotFound, result.Type);

    Assert.Equal(
        "Stajyer profili bulunamadı.",
        result.Message);

    internRepositoryMock.Verify(
        repository => repository.GetByUserIdAsync(userId),
        Times.Once);

    internRepositoryMock.Verify(
        repository => repository.GetAllAsync(),
        Times.Never);
}

[Fact]
public async Task GetAllIncludingInactiveAsync_ShouldReturnActiveAndInactiveInterns()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var interns = new List<Intern>
    {
        new()
        {
            Id = 1,
            Name = "Eda",
            Surname = "Şahin",
            Email = "eda@example.com",
            DepartmentId = 1,
            UserId = 10,
            IsActive = true
        },
        new()
        {
            Id = 2,
            Name = "Ayşe",
            Surname = "Yılmaz",
            Email = "ayse@example.com",
            DepartmentId = 2,
            UserId = 20,
            IsActive = false
        }
    };

    internRepositoryMock
        .Setup(repository => repository.GetAllIncludingInactiveAsync())
        .ReturnsAsync(interns);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetAllIncludingInactiveAsync();

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);
    Assert.NotNull(result.Data);

    Assert.Equal(2, result.Data.Count);

    Assert.Contains(
        result.Data,
        intern => intern.Id == 1 && intern.IsActive);

    Assert.Contains(
        result.Data,
        intern => intern.Id == 2 && !intern.IsActive);

    internRepositoryMock.Verify(
        repository => repository.GetAllIncludingInactiveAsync(),
        Times.Once);
}

[Fact]
public async Task CreateInternWithAccountAsync_DepartmentDoesNotExist_ShouldNotCheckEmailsOrCreateUser()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var dto = new CreateInternDto
    {
        Name = "Eda",
        Surname = "Şahin",
        Email = "eda@example.com",
        Password = "Test123!",
        DepartmentId = 999
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(dto.DepartmentId))
        .ReturnsAsync((Department?)null);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.CreateInternWithAccountAsync(dto);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.ValidationError, result.Type);

    Assert.Equal(
        "Departman bulunamadı.",
        result.Message);

    userRepositoryMock.Verify(
        repository => repository.EmailExistsAsync(It.IsAny<string>()),
        Times.Never);

    internRepositoryMock.Verify(
        repository => repository.EmailExistsAsync(It.IsAny<string>()),
        Times.Never);

    userRepositoryMock.Verify(
        repository => repository.AddAsync(It.IsAny<User>()),
        Times.Never);
}

[Fact]
public async Task CreateInternWithAccountAsync_ValidData_ShouldCreateLinkedUserAndInternWithNormalizedValues()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var dto = new CreateInternDto
    {
        Name = "  Eda  ",
        Surname = "  Şahin  ",
        Email = "  eda@example.com  ",
        Password = "Test123!",
        DepartmentId = 1
    };

    var department = new Department
    {
        Id = 1,
        Name = "Software Development",
        IsActive = true
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(dto.DepartmentId))
        .ReturnsAsync(department);

    userRepositoryMock
        .Setup(repository => repository.EmailExistsAsync("eda@example.com"))
        .ReturnsAsync(false);

    internRepositoryMock
        .Setup(repository => repository.EmailExistsAsync("eda@example.com"))
        .ReturnsAsync(false);

    User? createdUser = null;

    userRepositoryMock
        .Setup(repository => repository.AddAsync(It.IsAny<User>()))
        .Callback<User>(user => createdUser = user)
        .Returns(Task.CompletedTask);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.CreateInternWithAccountAsync(dto);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);

    Assert.NotNull(createdUser);
    var user = createdUser!;

    Assert.Equal("Eda", createdUser.Name);
    Assert.Equal("Şahin", createdUser.Surname);
    Assert.Equal("eda@example.com", createdUser.Email);

    Assert.Equal(Roles.Intern, createdUser.Role);
    Assert.True(user.MustChangePassword);

    Assert.False(string.IsNullOrWhiteSpace(createdUser.PasswordHash));

    Assert.NotNull(user.Intern);
    var createdIntern = user.Intern!;

    Assert.Equal("Eda", createdIntern.Name);
    Assert.Equal("Şahin", createdIntern.Surname);
    Assert.Equal("eda@example.com", createdIntern.Email);
    Assert.Equal(1, createdIntern.DepartmentId);

    Assert.Same(user, createdIntern.User);
    Assert.Same(createdIntern, user.Intern);

    userRepositoryMock.Verify(
        repository => repository.AddAsync(It.IsAny<User>()),
        Times.Once);
}

[Fact]
public async Task CreateInternWithAccountAsync_UserEmailAlreadyExists_ShouldReturnConflictAndSkipInternEmailCheck()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var dto = new CreateInternDto
    {
        Name = "Eda",
        Surname = "Şahin",
        Email = "eda@example.com",
        Password = "Test123!",
        DepartmentId = 1
    };

    var department = new Department
    {
        Id = 1,
        Name = "Software Development",
        IsActive = true
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(dto.DepartmentId))
        .ReturnsAsync(department);

    userRepositoryMock
        .Setup(repository => repository.EmailExistsAsync("eda@example.com"))
        .ReturnsAsync(true);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.CreateInternWithAccountAsync(dto);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.Conflict, result.Type);

    Assert.Equal(
        "Bu email adresi zaten kayıtlı.",
        result.Message);

    internRepositoryMock.Verify(
        repository => repository.EmailExistsAsync(It.IsAny<string>()),
        Times.Never);

    userRepositoryMock.Verify(
        repository => repository.AddAsync(It.IsAny<User>()),
        Times.Never);
}

[Fact]
public async Task CreateInternWithAccountAsync_InternEmailAlreadyExists_ShouldReturnConflictAndNotCreateUser()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var dto = new CreateInternDto
    {
        Name = "Eda",
        Surname = "Şahin",
        Email = "eda@example.com",
        Password = "Test123!",
        DepartmentId = 1
    };

    var department = new Department
    {
        Id = 1,
        Name = "Software Development",
        IsActive = true
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(dto.DepartmentId))
        .ReturnsAsync(department);

    userRepositoryMock
        .Setup(repository => repository.EmailExistsAsync("eda@example.com"))
        .ReturnsAsync(false);

    internRepositoryMock
        .Setup(repository => repository.EmailExistsAsync("eda@example.com"))
        .ReturnsAsync(true);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.CreateInternWithAccountAsync(dto);

    // ASSERT
    Assert.False(result.Success);
    Assert.Equal(ResultType.Conflict, result.Type);

    Assert.Equal(
        "Bu email adresine ait stajyer kaydı zaten mevcut.",
        result.Message);

    userRepositoryMock.Verify(
        repository => repository.AddAsync(It.IsAny<User>()),
        Times.Never);
}

[Fact]
public async Task CreateInternWithAccountAsync_EmailWithWhitespace_ShouldCheckDuplicatesUsingTrimmedEmail()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var dto = new CreateInternDto
    {
        Name = "Eda",
        Surname = "Şahin",
        Email = "  eda@example.com  ",
        Password = "Test123!",
        DepartmentId = 1
    };

    var department = new Department
    {
        Id = 1,
        Name = "Software Development",
        IsActive = true
    };

    departmentRepositoryMock
        .Setup(repository => repository.GetByIdAsync(dto.DepartmentId))
        .ReturnsAsync(department);

    userRepositoryMock
        .Setup(repository => repository.EmailExistsAsync("eda@example.com"))
        .ReturnsAsync(false);

    internRepositoryMock
        .Setup(repository => repository.EmailExistsAsync("eda@example.com"))
        .ReturnsAsync(false);

    userRepositoryMock
        .Setup(repository => repository.AddAsync(It.IsAny<User>()))
        .Returns(Task.CompletedTask);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.CreateInternWithAccountAsync(dto);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);

    userRepositoryMock.Verify(
        repository => repository.EmailExistsAsync("eda@example.com"),
        Times.Once);

    internRepositoryMock.Verify(
        repository => repository.EmailExistsAsync("eda@example.com"),
        Times.Once);

    userRepositoryMock.Verify(
        repository => repository.AddAsync(
            It.Is<User>(user =>
                user.Email == "eda@example.com")),
        Times.Once);
}

[Fact]
public async Task GetAllAsync_HR_ShouldReturnAllInterns()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var interns = new List<Intern>
    {
        new()
        {
            Id = 1,
            Name = "Eda",
            Surname = "Şahin",
            Email = "eda@example.com",
            DepartmentId = 1,
            UserId = 10,
            IsActive = true
        },
        new()
        {
            Id = 2,
            Name = "Ayşe",
            Surname = "Yılmaz",
            Email = "ayse@example.com",
            DepartmentId = 2,
            UserId = 20,
            IsActive = true
        }
    };

    internRepositoryMock
        .Setup(repository => repository.GetAllAsync())
        .ReturnsAsync(interns);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetAllAsync(
        200,
        Roles.HR);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);
    Assert.NotNull(result.Data);
    Assert.Equal(2, result.Data.Count);

    internRepositoryMock.Verify(
        repository => repository.GetAllAsync(),
        Times.Once);

    internRepositoryMock.Verify(
        repository => repository.GetByUserIdAsync(It.IsAny<int>()),
        Times.Never);
}

[Fact]
public async Task GetByIdAsync_HR_ShouldReturnInternWithoutCurrentInternCheck()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var internId = 2;
    var hrUserId = 200;

    var intern = new Intern
    {
        Id = internId,
        Name = "Ayşe",
        Surname = "Yılmaz",
        Email = "ayse@example.com",
        DepartmentId = 1,
        UserId = 20,
        IsActive = true
    };

    internRepositoryMock
        .Setup(repository => repository.GetByIdAsync(internId))
        .ReturnsAsync(intern);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetByIdAsync(
        internId,
        hrUserId,
        Roles.HR);

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);
    Assert.NotNull(result.Data);

    Assert.Equal(internId, result.Data.Id);
    Assert.Equal("Ayşe", result.Data.Name);
    Assert.Equal("Yılmaz", result.Data.Surname);

    internRepositoryMock.Verify(
        repository => repository.GetByUserIdAsync(It.IsAny<int>()),
        Times.Never);
}

[Fact]
public async Task GetAllIncludingInactiveAsync_ShouldMapAvatarFromLinkedUser()
{
    // ARRANGE
    var internRepositoryMock = new Mock<IInternRepository>();
    var departmentRepositoryMock = new Mock<IDepartmentRepository>();
    var userRepositoryMock = new Mock<IUserRepository>();

    var interns = new List<Intern>
    {
        new()
        {
            Id = 1,
            Name = "Eda",
            Surname = "Şahin",
            Email = "eda@example.com",
            DepartmentId = 1,
            UserId = 10,
            IsActive = true,
            User = new User
            {
                Id = 10,
                Name = "Eda",
                Surname = "Şahin",
                Email = "eda@example.com",
                PasswordHash = "dummy-hash",
                Role = Roles.Intern,
                Avatar = "avatar.png"
            }
        }
    };

    internRepositoryMock
        .Setup(repository => repository.GetAllIncludingInactiveAsync())
        .ReturnsAsync(interns);

    var service = new InternService(
        internRepositoryMock.Object,
        departmentRepositoryMock.Object,
        userRepositoryMock.Object,
        logger: Mock.Of<IAppLogger>());

    // ACT
    var result = await service.GetAllIncludingInactiveAsync();

    // ASSERT
    Assert.True(result.Success);
    Assert.Equal(ResultType.Success, result.Type);
    Assert.NotNull(result.Data);

    var intern = Assert.Single(result.Data);

    Assert.Equal("avatar.png", intern.Avatar);
}
}