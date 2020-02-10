
using CarpoolAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace CarpoolAPI.AuthenticationHandler
{   
    public class TokenOptions : AuthenticationSchemeOptions
    {
        public string Token { get; set; }

    }
    public class TokenHandler : AuthenticationHandler<TokenOptions>
    {
        private readonly CarpoolContext _context;
        public TokenHandler(
            IOptionsMonitor<TokenOptions> options,ILoggerFactory logger,UrlEncoder encoder,ISystemClock clock,CarpoolContext context)
            : base(options, logger, encoder, clock) {
            this._context = context;
        }

        //protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        //{
        //    Response.Headers["Token"] = $"Basic realm=\"{Options.Token}\", charset=\"UTF-8\"";
        //    return HandleAuthenticateAsync();
        //}
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 如果帶有`Token`標頭
            if (Context.Request.Headers.TryGetValue("Token", out StringValues token))
            {
                var emails = Context.Request.Query.Where(x => x.Key == "Email");
                if (!emails.Any()) { return AuthenticateResult.Fail("Email Error");  }
                var email = emails.First().Value;
                var tokenInfo = token;
                var result = from c in _context.Customers
                                    where c.Email.Equals(email)
                                    select new Customer() { 
                                        Email = c.Email,
                                        Token = c.Token
                                    };
                if (!result.Any()) { return AuthenticateResult.Fail("Email Error"); }
                Customer customer = result.First<Customer>();
                if (customer.Token.Equals(tokenInfo)) {

                    var claims = new ClaimsPrincipal(new ClaimsIdentity[]{
                        new ClaimsIdentity(
                            new Claim[] {
                                new Claim(ClaimsIdentity.DefaultNameClaimType, tokenInfo) // 直接回使用者ID
                            },
                            "Token" // 必須要加入authenticationType，否則會被作為未登入
                        )
                    });
                    return AuthenticateResult.Success(new AuthenticationTicket(claims, "Token"));
                }
                else
                {
                    return AuthenticateResult.Fail("Token Error");
                }
            }
            else
            {
                return AuthenticateResult.NoResult();
            }
        }
    }
}
