using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
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
                signin.Use(async (context, next) =>
                {
                    var claims = new List<Claim>();

                    var overrideUserId = context.Request.Query[StuntmanOptions.OverrideQueryStringKey];

                    if (string.IsNullOrWhiteSpace(overrideUserId))
                    {
                        await next.Invoke();

                        ShowLoginUI(context, options);
                    }
                    else
                    {
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

                        var authManager = context.Authentication;

                        authManager.SignIn(identity);

                        await next.Invoke();
                    }
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

        private static string GetResource(string resourceName)
        {
            var resource = string.Empty;

            using (Stream stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream(resourceName))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    resource = streamReader.ReadToEnd();
                }
            }

            return resource;
        }

        private static string GetUsersLoginUI(
            IOwinContext context,
            StuntmanOptions options)
        {
            var usersHtml = new StringBuilder();

            foreach (var user in options.Users)
            {
                var href = string.Format("{0}?OverrideUserId={1}&{2}={3}",
                    options.SignInUri,
                    user.Id,
                    StuntmanOptions.ReturnUrlQueryStringKey,
                    WebUtility.UrlEncode(context.Request.Query[StuntmanOptions.ReturnUrlQueryStringKey]));

                usersHtml.Append(string.Format(@"
<div class=""row"">
    <div class=""col-xs-12"">
        <a href=""{0}"" class=""btn btn-default col-xs-12"">{1}</a>
    </div>
</div>
", href, user.Name));
            }

            return usersHtml.ToString();
        }

        private static void RedirectToReturnUrl(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.Headers.Add("Location", new[]
                {
                    context.Request.Query[StuntmanOptions.ReturnUrlQueryStringKey]
                });

                context.Response.StatusCode = 302;

                return Task.FromResult(true);
            });
        }

        private static void ShowLoginUI(
            IOwinContext context,
            StuntmanOptions options)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;

            var css = GetResource("RimDev.Stuntman.Core.resources.stuntman.pure.css");
            var usersHtml = GetUsersLoginUI(context, options);

            context.Response.Write(string.Format(@"
<html>
    <head>
        <style>
            {0}
        </style>
    </head>
    <body>
        <div class=""container"">
            <h2 class=""text-center"">Please select a user to continue authentication.</h2>
            <div class=""row"">
                <div class=""col-md-6 col-md-offset-3"">
                    {1}
                </div>
            </div>
        </div>
    </body>
</html>
", css, usersHtml));
        }
    }
}
