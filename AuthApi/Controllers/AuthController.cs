using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.Auth;
using Shared.Dtos;
using Shared.Models.Auth;
using Shared;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        UserManager<ApplicationUser> _userManager,
        SignInManager<ApplicationUser> _signInManager,
        RoleManager<IdentityRole> _roleManager,
        IOptions<JwtOptions> _jwtOptions
        ) :
        ControllerBase
    {
        [HttpPost("register")]
        [SwaggerResponse(StatusCodes.Status201Created)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult<ResponseDto>> Register([FromBody] RegisterRequestDto dto)
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (dto == null || ModelState.IsValid == false)
            {
                return BadRequest();
            }

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, dto.Password!);
            if (result.Succeeded)
            {
                if (dto.Role != Constants.Role_Admin && dto.Role != Constants.Role_User)
                {
                    dto.Role = Constants.Role_User;
                }

                if (await _roleManager.RoleExistsAsync(dto.Role) == false)
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = dto.Role });
                }

                result = await _userManager.AddToRoleAsync(user, dto.Role!);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status201Created);
                }
            }

            return BadRequest(new ResponseDto
            {
                IsSuccessful = false,
                Errors = result.Errors.Select(x => x.Description).ToList()
            });
        }

        [HttpPost("login")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            if (dto == null || ModelState.IsValid == false)
            {
                return BadRequest();
            }

            var result = await _signInManager.PasswordSignInAsync(dto.UserName!, dto.Password!, true, false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(dto.UserName!);
                if (user != null)
                {
                    var jwtOptions = _jwtOptions.Value;
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key!));
                    var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName!),
                        new Claim(ClaimTypes.Email, user.Email!),
                        new Claim("Id", user.Id),
                        new Claim("PhoneNumber", user.PhoneNumber!),
                    };

                    var roles = await _userManager.GetRolesAsync(user);
                    claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));

                    var tokenOptions = new JwtSecurityToken(
                        issuer: jwtOptions.Issuer,
                        audience: jwtOptions.Audience,
                        claims: claims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: signingCredentials
                        );

                    var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                    return Ok(new LoginResponseDto
                    {
                        IsSuccessful = true,
                        Token = token,
                        UserDto = new UserDto
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            PhoneNumber = user.PhoneNumber,
                        }
                    });
                }
            }

            return Unauthorized(new LoginResponseDto
            {
                IsSuccessful = false,
                ErrorMessage = "Invalid Login"
            });
        }
    }
}
