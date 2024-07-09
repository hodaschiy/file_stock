using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using NLog;
using Server.Models;
using Server.Data;
using static Server.Controllers.FileModelsController;

namespace Server.Services
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ICryptService _crypt;
        private readonly ServerContext _context;

        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            ICryptService crypt,
            ServerContext context)
            : base(options, logger, encoder, clock) 
        {
            _crypt = crypt;
            _context = context; 
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            string authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            if (!authorizationHeader.StartsWith("basic ", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var credentialAsString = authorizationHeader.Substring(6);

            var credentials = credentialAsString.Split(":");
            if (credentials?.Length != 2)
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var username = credentials[0];
            User usr = _context.User.Where<User>(x => x.Name == username).FirstOrDefault();
            var token = credentials[1];

            byte[] f_step = Convert.FromBase64String(token.Replace(' ', '+'));
            byte[] s_step = _crypt.DecryptData(f_step, usr.PublicKey);
            string t_step = Encoding.UTF8.GetString(s_step);

            try
            {
                if (Encoding.UTF8.GetString(_crypt.DecryptData(Convert.FromBase64String(usr.Token))) != t_step)
                {
                    return AuthenticateResult.Fail("Wrong token");
                }
            }
            catch
            {
                return AuthenticateResult.Fail("Wrong key");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username)
            };

            var identity = new ClaimsIdentity(claims, "Basic");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name));
        }
    }
}
