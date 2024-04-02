using Blazor8App.Client.Services.IServices;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Shared;
using Shared.Dtos;
using Shared.Dtos.Auth;
using System.Net.Http.Headers;
using System.Text;

namespace Blazor8App.Client.Services
{
    public class AuthenticationService(HttpClient _httpClient, IJSRuntime _js, AuthenticationStateProvider _authStateProvider) : IAuthenticationService
    {
        public async Task<LoginResponseDto> Login(LoginRequestDto dto)
        {
            var content = JsonConvert.SerializeObject(dto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("login", bodyContent);
            var contentStr = await response.Content.ReadAsStringAsync();
            var loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(contentStr);

            if (response.IsSuccessStatusCode)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", Constants.Local_Token, loginResponseDto!.Token);
                await _js.InvokeVoidAsync("localStorage.setItem", Constants.Local_UserDetails, loginResponseDto!.UserDto);
                ((AuthStateProvider)_authStateProvider).NotifyUserLoggedIn(loginResponseDto.Token!);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResponseDto.Token);
                return new LoginResponseDto() { IsSuccessful = true };
            }
            else
            {
                return loginResponseDto!;
            }
        }

        public async Task Logout()
        {
            await _js.InvokeAsync<string>("localStorage.removeItem", Constants.Local_Token);
            await _js.InvokeAsync<string>("localStorage.removeItem", Constants.Local_UserDetails);

            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();

            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<ResponseDto> RegisterUser(RegisterRequestDto dto)
        {
            var content = JsonConvert.SerializeObject(dto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("register", bodyContent);
            var contentStr = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResponseDto>(contentStr);

            if (response.IsSuccessStatusCode)
            {
                return new ResponseDto { IsSuccessful = true };
            }

            return new ResponseDto { IsSuccessful = false, Errors = result!.Errors };
        }
    }
}
