using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyIdentityServer.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyIdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("registraion")]
        public async Task<IActionResult> Register(RegisterUser registerUser)
        {
            // validate if user already exits
            var user = await _userManager.FindByEmailAsync(registerUser.Email!);

            if (user is not null)
            {
                return BadRequest("User Already Exists!");
            }

            // if no create a new
            ApplicationUser applicationUser = new()
            {
                FirstName = registerUser.FirstName,
                LastName = registerUser.LastName,
                Age = registerUser.Age,
                Email = registerUser.Email,
                UserName = registerUser.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
                Location = registerUser.Location,
                PhoneNumber = registerUser.PhoneNumber,
                EmailConfirmed = true
            };

            await _userManager.CreateAsync(applicationUser, registerUser.Password);

            await _userManager.AddToRoleAsync(applicationUser, "User");

            return Ok("User created Successfully!!!");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginUser loginUser)
        {
            var user = await _userManager.FindByEmailAsync(loginUser.Email);

            if (user is null) return BadRequest(loginUser);

            if (user is not null)
            {
                var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginUser.Password);

                if (!isPasswordCorrect) return BadRequest(loginUser);

                var userRoles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>()
                {
                    new(ClaimTypes.Name, user.FullName),
                    new(ClaimTypes.Email, user.Email!),
                    new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new(ClaimTypes.Locality, user.Location),
                    new("Age", user.Age.ToString())
                };

                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                }

                if (userRoles.FirstOrDefault(role => role == "Admin") is not null)
                {
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    return Ok("Signed In successfully!");
                }


                var token = GenerateJWTToken(claims);

                return Ok(token);
            }

            return BadRequest(loginUser);
        }

        private string GenerateJWTToken(IEnumerable<Claim> claims)
        {
            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

            var tokenObject = new JwtSecurityToken(
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(1),
                    claims: claims,
                    signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256)
                );

            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

            return "Bearer " + token;
        }
    }
}
