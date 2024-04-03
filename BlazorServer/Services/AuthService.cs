using BlazorWasm.Services.IServices;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Shared;
using Shared.Dtos;
using Shared.Dtos.Auth;
using System.Net.Http.Headers;
using System.Text;

namespace BlazorWasm.Services
{
    public class AuthService(HttpClient _httpClient, IJSRuntime _js, AuthenticationStateProvider _authStateProvider) : IAuthService
    {
        public async Task<ResponseDto> Register(RegisterRequestDto dto)
        {
            var content = JsonConvert.SerializeObject(dto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("register", bodyContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResponseDto>(responseContent);
            if (response.IsSuccessStatusCode)
            {
                return new ResponseDto { IsSuccessful = true };
            }

            return new ResponseDto { IsSuccessful = false, Errors = result!.Errors };
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto dto)
        {
            var content = JsonConvert.SerializeObject(dto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("login", bodyContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LoginResponseDto>(responseContent);
            if (response.IsSuccessStatusCode)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", Constants.Local_Token, result!.Token);
                await _js.InvokeVoidAsync("localStorage.setItem", Constants.Local_UserDetails, result.UserDto);

                ((AuthStateProvider)_authStateProvider).NotifyUserLoggedIn(result.Token!);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);
                return new LoginResponseDto() { IsSuccessful = true };
            }

            return result!;
        }

        public async Task Logout()
        {
            await _js.InvokeAsync<string>("localStorage.removeItem", Constants.Local_Token);
            await _js.InvokeAsync<string>("localStorage.removeItem", Constants.Local_UserDetails);

            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();

            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
