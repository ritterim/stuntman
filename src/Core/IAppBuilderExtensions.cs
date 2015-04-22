using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RimDev.Stuntman.Core
{
    public static class IAppBuilderExtensions
    {
        private const string StuntmanAuthenticationType = "StuntmanAuthentication";

        public static void UseStuntman(this IAppBuilder app, StuntmanOptions options)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = StuntmanAuthenticationType,
                LoginPath = new PathString(options.SignInUri),
                LogoutPath = new PathString(options.SignOutUri),
                ReturnUrlParameter = StuntmanOptions.ReturnUrlQueryStringKey,
            });

            app.Map(options.SignInUri, signin =>
            {
                signin.Use((context, next) =>
                {
                    var claims = new List<Claim>();

                    var overrideUserId = context.Request.Query[StuntmanOptions.OverrideQueryStringKey];

                    var user = options.Users
                        .Where(x => x.Id == overrideUserId)
                        .FirstOrDefault();

                    if (user == null)
                    {
                        throw new ArgumentException(string.Format(
                            "options provided does not include the requested '{0}' user.",
                            overrideUserId));
                    }

                    claims.Add(new Claim(ClaimTypes.Name, user.Name));
                    claims.AddRange(user.Claims);

                    var identity = new ClaimsIdentity(claims, StuntmanAuthenticationType);
                    var principal = new ClaimsPrincipal(identity);

                    var authManager = context.Authentication;
                    authManager.SignIn(new AuthenticationProperties(), identity);

                    return next.Invoke();
                });

                RedirectToReturnUrl(signin);
            });

            app.Map(options.SignOutUri, signout =>
            {
                signout.Use((context, next) =>
                {
                    var authManager = context.Authentication;
                    authManager.SignOut(StuntmanAuthenticationType);

                    return next.Invoke();
                });

                RedirectToReturnUrl(signout);
            });
        }

        private static void RedirectToReturnUrl(IAppBuilder app)
        {
            app.Run(c =>
            {
                c.Response.Headers.Add("Location", new[]
                {
                    c.Request.Query[StuntmanOptions.ReturnUrlQueryStringKey]
                });
                c.Response.StatusCode = 302;
                return Task.FromResult(true);
            });
        }
    }
}
