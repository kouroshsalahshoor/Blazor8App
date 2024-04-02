using Shared.Dtos;
using Shared.Dtos.Auth;

namespace BlazorWasm.Services.IServices
{
    public interface IAuthenticationService
    {
        Task<ResponseDto> Register(RegisterRequestDto dto);
        Task<LoginResponseDto> Login(LoginRequestDto dto);
        Task Logout();
    }
}
