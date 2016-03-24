using Owin;
using RimDev.Stuntman.Core;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace RimDev.Stuntman.UsageSample
{
    public class Startup
    {
        public readonly StuntmanOptions StuntmanOptions = new StuntmanOptions();

        public void Configuration(IAppBuilder app)
        {
            StuntmanOptions
                .AddUser(new StuntmanUser("user-1", "User 1")
                    .SetAccessToken("user-1-token")
                    .AddClaim("given_name", "John")
                    .AddClaim("family_name", "Doe"))
                .AddUser(new StuntmanUser("user-2", "User 2")
                    .AddClaim("given_name", "Jane")
                    .AddClaim("family_name", "Doe"))
                .AddUser(new StuntmanUser("user-3", "User 3")
                    .AddClaim("given_name", "Sam")
                    .AddClaim("family_name", "Smith"))
                .AddUsersFromJson("https://raw.githubusercontent.com/ritterim/stuntman/master/samples/UsageSample/test-users-1.json") // Tried this using OWIN locally, didn't get it working.
                .AddUsersFromJson(Path.Combine(GetBinPath(), "test-users-2.json"));

            if (System.Web.HttpContext.Current.IsDebuggingEnabled)
            {
                app.UseStuntman(StuntmanOptions);
            }

            app.Map("/secure", secure =>
            {
                AuthenticateAllRequests(secure, new[] { "StuntmanAuthentication" });

                secure.Run(context =>
                {
                    var userName = context.Request.User.Identity.Name;

                    if (string.IsNullOrEmpty(userName))
                        userName = "Anonymous / Unknown";

                    context.Response.ContentType = "text/html";
                    context.Response.WriteAsync(
                        $"Hello, {userName}. This is the /secure endpoint.");

                    if (System.Web.HttpContext.Current.IsDebuggingEnabled)
                    {
                        context.Response.WriteAsync(
                            StuntmanOptions.UserPicker(context.Request.User));
                    }

                    return Task.FromResult(true);
                });
            });

            app.Map("/secure-json", secure =>
            {
                AuthenticateAllRequests(secure, new[] { "StuntmanAuthentication" });

                secure.Run(context =>
                {
                    var userName = context.Request.User.Identity.Name;

                    if (string.IsNullOrEmpty(userName))
                        userName = "Anonymous / Unknown";

                    context.Response.ContentType = "application/json";
                    context.Response.WriteAsync(
                        $@"{{""message"":""Hello, {userName}. This is the /secure-json endpoint.""}}");

                    return Task.FromResult(true);
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

                    context.Response.ContentType = "text/html";

                    context.Response.WriteAsync(
@"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""utf-8"">
        <title>Stuntman - UsageSample</title>
    </head>
<body>");

                    context.Response.WriteAsync(
                        $"Hello, {userName}.");

                    if (System.Web.HttpContext.Current.IsDebuggingEnabled)
                    {
                        context.Response.WriteAsync(
                            StuntmanOptions.UserPicker(context.Request.User));
                    }

                    context.Response.WriteAsync(
@"</body>
</html>");

                    return Task.FromResult(true);
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

        private static string GetBinPath()
        {
            const string FilePrefix = @"file:\";

            // http://stackoverflow.com/a/3461871/941536
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);

            if (!path.StartsWith(FilePrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException($"Expected path to begin with {FilePrefix}.");
            }

            return path.Substring(FilePrefix.Length);
        }
    }
}
