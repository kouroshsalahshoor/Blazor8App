namespace Shared.Dtos.Auth
{
    public class LoginResponseDto
    {
        public bool IsSuccessful { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public string? Token { get; set; }
        public UserDto? UserDto { get; set; }
    }
}
