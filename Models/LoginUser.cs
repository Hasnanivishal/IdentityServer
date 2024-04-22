using System.ComponentModel.DataAnnotations;

namespace MyIdentityServer.Models
{
    public class LoginUser
    {
        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
