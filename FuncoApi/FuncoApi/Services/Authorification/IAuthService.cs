using FuncoApi.Dto.UserDto;

namespace FuncoApi.UserTools;

public interface IAuthService
{
    Task<AutentificationResponseDto> RegisterAsync(RegisterDto dto);

    Task<AutentificationResponseDto> LoginAsync(LogginDto dto);
}