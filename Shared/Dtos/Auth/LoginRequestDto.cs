using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.Auth
{
    public class LoginRequestDto
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
