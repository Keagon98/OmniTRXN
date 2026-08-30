using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace OzowTransactionService.Api.Handlers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "BasicAuthentication";
        private readonly string _username;
        private readonly string _password;

        public BasicAuthenticationHandler(
            IConfiguration config,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder) : base(options, logger, encoder)
        {
            _username = config["Auth:Username"] ?? throw new ArgumentNullException(nameof(_username), "Username not found");
            _password = config["Auth:Password"] ?? throw new ArgumentNullException(nameof(_password), "Password not found");
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization",
                out var authHeaderValue))
            {
                return Task.FromResult(AuthenticateResult.Fail(
                    "'Authorization' is missing from the request header")
                );
            }

            if (!AuthenticationHeaderValue.TryParse(authHeaderValue.ToString(), out var authHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail(
                    "Unable to convert to a authentication header value"));
            }

            if (!authHeader.Scheme.Equals("Basic",
                StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail(
                    "Authentication scheme is not 'Basic'"));
            }

            if (!Base64.IsValid(authHeader.Parameter!))
            {
                return Task.FromResult(AuthenticateResult.Fail(
                    "'Authorization' header value isn't formatted correctly")
                );
            }

            var credentialsDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter!));
            var credentials = credentialsDecoded.Split(':', 2);

            if (credentials.Length != 2)
            {
                return Task.FromResult(AuthenticateResult.Fail(
                    "'Authorization' header value isn't formatted correctly")
                );
            }

            var username = credentials[0];
            var password = credentials[1];

            if (username != _username || password != _password)
            {
                return Task.FromResult(AuthenticateResult.Fail(
                    "Invalid username or password")
                );
            }

            var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.AuthenticationMethod, authHeader.Scheme)
                ],
                SchemeName
                );

            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(identity),
                    SchemeName
                )));
        }
    }
}
