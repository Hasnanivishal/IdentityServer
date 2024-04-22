using Microsoft.AspNetCore.Identity;

namespace MyIdentityServer.Models;

public class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public int Age { get; set; }

    public string FullName
    {
        get => FirstName + " " + LastName;
    }

    public string Location { get; set; } = string.Empty;
}
