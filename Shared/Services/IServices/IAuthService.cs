using Shared.Dtos;
using Shared.Dtos.Auth;

namespace Shared.Services.IServices
{
    public interface IAuthService
    {
        Task<ResponseDto> Register(RegisterRequestDto dto);
        Task<LoginResponseDto> Login(LoginRequestDto dto);
        Task Logout();
    }
}
