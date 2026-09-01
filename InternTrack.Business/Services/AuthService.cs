using InternTrack.Business.Common;
using InternTrack.Business.Interfaces;
using InternTrack.Core.DTOs;
using InternTrack.Core.Models;
using InternTrack.DataAccess.Interfaces;

namespace InternTrack.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IDepartmentRepository departmentRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult> RegisterAsync(RegisterDto dto)
    {
        var department =
            await _departmentRepository.GetByIdAsync(
                dto.DepartmentId
            );

        if (department == null)
        {
            return ServiceResult.ValidationError(
                "Departman bulunamadı."
            );
        }

        var emailExists =
            await _userRepository.EmailExistsAsync(
                dto.Email
            );

        if (emailExists)
        {
            return ServiceResult.Conflict(
                "Bu email adresi zaten kayıtlı."
            );
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            PasswordHash =
                PasswordHasher.Hash(dto.Password),

            Role = "Intern"
        };

        var intern = new Intern
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            DepartmentId = dto.DepartmentId,
            User = user
        };

        user.Intern = intern;

        await _userRepository.AddAsync(user);

        return ServiceResult.Ok(
            "Stajyer hesabı başarıyla oluşturuldu."
        );
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(
        LoginDto dto)
    {
        var user =
            await _userRepository.GetByEmailAsync(
                dto.Email
            );

        if (user == null)
        {
            return ServiceResult<LoginResponseDto>
                .ValidationError(
                    "Email veya şifre hatalı."
                );
        }

        var passwordIsCorrect =
            PasswordHasher.Verify(
                dto.Password,
                user.PasswordHash
            );

        if (!passwordIsCorrect)
        {
            return ServiceResult<LoginResponseDto>
                .ValidationError(
                    "Email veya şifre hatalı."
                );
        }

        var token =
            _tokenService.CreateToken(user);

        var response = new LoginResponseDto
        {
            Token = token,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };

        return ServiceResult<LoginResponseDto>
            .Ok(response);
    }
}