using Owin;
using RimDev.Stuntman.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RimDev.Stuntman.UsageSample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseStuntman(new StuntmanOptions
            {
                Users = new[]
                {
                    new StuntmanUser
                    {
                        Id = "user-1",
                        Name = "User 1",
                        Claims = new[]
                        {
                            new Claim("given_name", "John"),
                            new Claim("family_name", "Doe")
                        }
                    },
                }
            });

            app.Map("/secure", secure =>
            {
                AuthenticateAllRequests(secure, new[] { "StuntmanAuthentication" });

                secure.Run(context =>
                {
                    var userName = context.Request.User.Identity.Name;

                    if (string.IsNullOrEmpty(userName))
                        userName = "Anonymous / Unknown";

                    context.Response.ContentType = "text/plain";
                    return context.Response.WriteAsync(string.Format(
                        "Hello, {0}. This is the /secure endpoint.",
                        userName));
                });
            });

            app.Map("/logout", logout =>
            {
                logout.Run(context =>
                {
                    context.Authentication.SignOut();
                    return Task.FromResult(true);
                });
            });

            app.Map("", nonSecure =>
            {
                nonSecure.Run(context =>
                {
                    var userName = context.Request.User.Identity.Name;

                    if (string.IsNullOrEmpty(userName))
                        userName = "Anonymous / Unknown";

                    context.Response.ContentType = "text/plain";
                    return context.Response.WriteAsync(string.Format(
                        "Hello, {0}.",
                        userName));
                });
            });
        }

        // http://stackoverflow.com/a/26265757
        private static void AuthenticateAllRequests(IAppBuilder app, params string[] authenticationTypes)
        {
            app.Use((context, continuation) =>
            {
                if (context.Authentication.User != null &&
                    context.Authentication.User.Identity != null &&
                    context.Authentication.User.Identity.IsAuthenticated)
                {
                    return continuation();
                }
                else
                {
                    context.Authentication.Challenge(authenticationTypes);
                    return Task.Delay(0);
                }
            });
        }
    }
}
