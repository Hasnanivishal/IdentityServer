using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyIdentityServer.Authorization;

public class LocationRequirement(string location) : IAuthorizationRequirement
{
    public string Location { get; set; } = location;
}

public class LocationAuthorizationHandler : AuthorizationHandler<LocationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        LocationRequirement requirement)
    {
        var location = context.User.FindFirst(x => x.Type == ClaimTypes.Locality);

        if (location is not null)
        {
            if (location.Value == requirement.Location)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
