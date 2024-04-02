using Blazor8App.Client.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Shared;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Blazor8App.Client.Services
{
    public class AuthStateProvider(HttpClient _httpClient, IJSRuntime _js) : AuthenticationStateProvider
    {
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", Constants.Local_Token);
            if (token == null)
            {
                //test
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "xxx")
                })));
                //return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType")));
        }

        public void NotifyUserLoggedIn(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
