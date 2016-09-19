using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RimDev.Stuntman.Core
{
    public static class IAppBuilderExtensions
    {
        /// <summary>
        /// Enable Stuntman on this application.
        /// </summary>
        public static void UseStuntman(this IAppBuilder app, StuntmanOptions options)
        {
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
            {
                AuthenticationType = Constants.StuntmanAuthenticationType,
                Provider = new StuntmanOAuthBearerProvider(options),
                AccessTokenFormat = new StuntmanOAuthAccessTokenFormat()
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.StuntmanAuthenticationType,
                LoginPath = new PathString(options.SignInUri),
                LogoutPath = new PathString(options.SignOutUri),
                ReturnUrlParameter = Constants.StuntmanOptions.ReturnUrlQueryStringKey,
            });

            app.Map(options.SignInUri, signin =>
            {
                signin.Use(async (context, next) =>
                {
                    var claims = new List<Claim>();

                    var overrideUserId = context.Request.Query[Constants.StuntmanOptions.OverrideQueryStringKey];

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
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync(
                                $"options provided does not include the requested '{overrideUserId}' user.");

                            return;
                        }

                        claims.Add(new Claim(ClaimTypes.Name, user.Name));
                        claims.AddRange(user.Claims);

                        var identity = new ClaimsIdentity(claims, Constants.StuntmanAuthenticationType);

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
                    authManager.SignOut(Constants.StuntmanAuthenticationType);

                    return next.Invoke();
                });

                RedirectToReturnUrl(signout);
            });

            if (options.ServerEnabled)
            {
                app.Map(options.ServerUri, server =>
                {
                    server.Use(async (context, next) =>
                    {
                        var response = new StuntmanServerResponse { Users = options.Users };
                        var json = JsonConvert.SerializeObject(response);

                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(json);
                    });
                });
            }
        }

        private static string GetUsersLoginUI(
            IOwinContext context,
            StuntmanOptions options)
        {
            var usersHtml = new StringBuilder();

            foreach (var user in options.Users)
            {
                var href = $"{options.SignInUri}?OverrideUserId={user.Id}&{Constants.StuntmanOptions.ReturnUrlQueryStringKey}={WebUtility.UrlEncode(context.Request.Query[Constants.StuntmanOptions.ReturnUrlQueryStringKey])}";

                usersHtml.Append($@"<li><a href=""{href}"" title=""{(string.IsNullOrEmpty(user.Description) ? null : WebUtility.HtmlEncode(user.Description) + " ")}Source: {user.Source}"">{user.Name}</a></li>");
            }

            return usersHtml.ToString();
        }

        private static void RedirectToReturnUrl(IAppBuilder app)
        {
            app.Run(context =>
            {
                var returnUrl = context.Request.Query[Constants.StuntmanOptions.ReturnUrlQueryStringKey];

                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    returnUrl = context.Request.Headers["Referer"];
                }

                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    throw new InvalidOperationException(
                        "ReturnUrl was not specified via query string or Referer header.");
                }

                context.Response.Headers.Add("Location", new[] { returnUrl });

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

            var css = Resources.GetCss();
            var logoForInlining = Resources.GetLogoForInlining();
            var usersHtml = GetUsersLoginUI(context, options);

            context.Response.Write($@"
<!DOCTYPE html>
<html>
    <head>
        <meta charset=""UTF-8"">
        <title>Select a user</title>
        <style>
            {css}
        </style>
    </head>
    <body>
        <div class=""stuntman-login-ui-container"">
            <h2><img src=""{logoForInlining}"" alt=""Welcome to Stuntman"" /></h2>
            <h2>Please select a user to continue authentication.</h2>
            <ul>
                {usersHtml}
            </ul>
        </div>
    </body>
</html>");
        }

        private class StuntmanOAuthBearerProvider : OAuthBearerAuthenticationProvider
        {
            public StuntmanOAuthBearerProvider(StuntmanOptions options)
            {
                this.options = options;
            }

            private readonly StuntmanOptions options;

            public override Task ValidateIdentity(OAuthValidateIdentityContext context)
            {
                var authorizationBearerToken = context.Request.Headers["Authorization"];

                if (string.IsNullOrWhiteSpace(authorizationBearerToken))
                {
                    context.Rejected();

                    return Task.FromResult(false);
                }
                else
                {
                    var authorizationBearerTokenParts = authorizationBearerToken
                        .Split(' ');

                    var accessToken = authorizationBearerTokenParts
                        .LastOrDefault();

                    var claims = new List<Claim>();
                    StuntmanUser user = null;

                    if (authorizationBearerTokenParts.Count() != 2 ||
                        string.IsNullOrWhiteSpace(accessToken))
                    {
                        context.Response.StatusCode = 400;
                        context.Response.ReasonPhrase = "Authorization header is not in correct format.";

                        context.Rejected();

                        return Task.FromResult(false);
                    }
                    else
                    {
                        user = options.Users
                            .Where(x => x.AccessToken == accessToken)
                            .FirstOrDefault();

                        if (user == null)
                        {
                            context.Response.StatusCode = 403;
                            context.Response.ReasonPhrase =
                                $"options provided does not include the requested '{accessToken}' user.";

                            context.Rejected();

                            return Task.FromResult(false);
                        }
                        else
                        {
                            claims.Add(new Claim("access_token", accessToken));
                        }
                    }

                    claims.Add(new Claim(ClaimTypes.Name, user.Name));
                    claims.AddRange(user.Claims);

                    var identity = new ClaimsIdentity(claims, Constants.StuntmanAuthenticationType);

                    context.Validated(identity);

                    var authManager = context.OwinContext.Authentication;

                    authManager.SignIn(identity);

                    if (options.AfterBearerValidateIdentity != null)
                    {
                        options.AfterBearerValidateIdentity(context);
                    }

                    return Task.FromResult(true);
                }
            }
        }

        private class StuntmanOAuthAccessTokenFormat : ISecureDataFormat<AuthenticationTicket>
        {
            public string Protect(AuthenticationTicket data)
            {
                throw new NotSupportedException(
                    "Stuntman does not protect data.");
            }

            public AuthenticationTicket Unprotect(string protectedText)
            {
                return new AuthenticationTicket(
                    identity: new ClaimsIdentity(),
                    properties: new AuthenticationProperties());
            }
        }
    }
}
