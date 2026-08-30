using Microsoft.AspNetCore.Authorization;
using OzowTransactionService.Api.Handlers;

namespace OzowTransactionService.Api.Attributes
{
    public class BasicAuthorizationAttribute : AuthorizeAttribute
    {
        public BasicAuthorizationAttribute()
        {
            AuthenticationSchemes = BasicAuthenticationHandler.SchemeName;
        }
    }
}
