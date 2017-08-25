using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RimDev.Stuntman.Core;
using System;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace UsageSample.AspNetCore
{
    public class Startup
    {
        public static readonly StuntmanOptions StuntmanOptions = new StuntmanOptions();

        public Startup()
        {
            StuntmanOptions
                .EnableServer()
                .AddUser(new StuntmanUser("user-1", "User 1")
                    .SetAccessToken("user-1-token")
                    .SetDescription("This is User 1.")
                    .AddClaim("given_name", "John")
                    .AddClaim("family_name", "Doe"))
                .AddUser(new StuntmanUser("user-2", "User 2")
                    .AddClaim("given_name", "Jane")
                    .AddClaim("family_name", "Doe"))
                .AddUser(new StuntmanUser("user-3", "User 3")
                    .AddClaim("given_name", "Sam")
                    .AddClaim("family_name", "Smith"))
            //.AddUsersFromJson("https://raw.githubusercontent.com/ritterim/stuntman/master/samples/UsageSample.AspNetCore/test-server-response-1.json")
            .AddUsersFromJson(Path.Combine(GetBinPath(), "test-server-response-2.json"));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStuntman(StuntmanOptions);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStuntman(StuntmanOptions);

            app.Map("/secure", secure =>
            {
                AuthenticateAllRequests(secure, new[] { "StuntmanAuthentication" });

                secure.Run(context =>
                {
                    var userName = context.User.Identity.Name;

                    if (string.IsNullOrEmpty(userName))
                        userName = "Anonymous / Unknown";

                    context.Response.ContentType = "text/html";
                    context.Response.WriteAsync(
                        $"Hello, {userName}. This is the /secure endpoint.");

                    context.Response.WriteAsync(
                        StuntmanOptions.UserPicker(context.User));

                    return Task.FromResult(true);
                });
            });

            app.Map("/secure-json", secure =>
            {
                AuthenticateAllRequests(secure, new[] { "StuntmanAuthentication" });

                secure.Run(context =>
                {
                    var userName = context.User.Identity.Name;

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
                logout.Run(async context =>
                {
                    await context.SignOutAsync("StuntmanAuthentication");
                });
            });

            app.Map("", nonSecure =>
            {
                nonSecure.Run(context =>
                {
                    var userName = context.User?.Identity.Name;

                    if (string.IsNullOrEmpty(userName))
                        userName = "Anonymous / Unknown";

                    context.Response.ContentType = "text/html";

                    context.Response.WriteAsync(
@"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""utf-8"">
        <title>Stuntman - UsageSample.AspNetCore</title>
    </head>
<body>");

                    context.Response.WriteAsync(
                        $@"Hello, {userName}. <a href=""/secure"">Secure page</a>");

                    context.Response.WriteAsync(
                        StuntmanOptions.UserPicker(context.User ?? new ClaimsPrincipal()));

                    context.Response.WriteAsync(
@"</body>
</html>");

                    return Task.FromResult(true);
                });
            });
        }

        // Adapted from https://stackoverflow.com/a/26265757
        private static void AuthenticateAllRequests(IApplicationBuilder app, params string[] authenticationTypes)
        {
            app.Use(async (context, continuation) =>
            {
                if (context.User != null &&
                    context.User.Identity != null &&
                    context.User.Identity.IsAuthenticated)
                {
                    await continuation();
                }
                else
                {
                    await context.ChallengeAsync(Constants.StuntmanAuthenticationType);
                }
            });
        }

        private static string GetBinPath()
        {
            const string FilePrefix = @"file:\";

            // https://stackoverflow.com/a/3461871/941536
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);

            if (!path.StartsWith(FilePrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException($"Expected path to begin with {FilePrefix}.");
            }

            return path.Substring(FilePrefix.Length);
        }
    }
}
