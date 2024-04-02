using Shared.Dtos;
using Shared.Dtos.Auth;

namespace Blazor8App.Client.Services.IServices
{
    public interface IAuthenticationService
    {
        Task<ResponseDto> RegisterUser(RegisterRequestDto dto);
        Task<LoginResponseDto> Login(LoginRequestDto dto);
        Task Logout();
    }
}
