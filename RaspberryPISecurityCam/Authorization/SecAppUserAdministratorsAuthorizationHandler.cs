using System.Threading.Tasks;
using RaspberryPISecurityCam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace RaspberryPISecurityCam.Authorization
{
    public class SecAppUserAdministratorsAuthorizationHandler
                    : AuthorizationHandler<OperationAuthorizationRequirement, SecAppUser>
    {
        protected override Task HandleRequirementAsync(
                                              AuthorizationHandlerContext context,
                                    OperationAuthorizationRequirement requirement,
                                     SecAppUser resource)
        {
            if (context.User == null)
            {
                return Task.FromResult(0);
            }

            // Administrators can do anything.
            if (context.User.IsInRole(Constants.SecAppUserAdministratorsRole))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}
