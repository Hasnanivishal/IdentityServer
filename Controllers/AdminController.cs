using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyIdentityServer.Models;

namespace MyIdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Route("GetUsersInUserRole")]
        public async Task<IActionResult> GetUsersInUserRole()
        {
            var users = await _userManager.GetUsersInRoleAsync("User");

            return Ok(users);
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.GetUsersInRoleAsync("User").Result.ToList();

            users.AddRange([.. _userManager.GetUsersInRoleAsync("Admin").Result]);

            return Ok(users);
        }

        [HttpGet("GetAllRoles")]
        [Authorize(Roles = "User, Admin")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.Select(role => role.Name);
            return Ok(roles);
        }
    }
}
