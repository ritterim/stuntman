using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Newtonsoft.Json;

namespace RimDev.Stuntman.Core
{
    public static class IApplicationBuilderExtensions
    {
        public static void UseStuntman(this IApplicationBuilder app, StuntmanOptions options)
        {
            if (options.AllowBearerTokenAuthentication)
            {
                app.UseJwtBearerAuthentication(new JwtBearerOptions
                {
                    AuthenticationScheme = Constants.StuntmanAuthenticationType,
                    Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = context => StuntmanOnMessageReceived(options, context),
                    }
                });
            }

            if (options.AllowCookieAuthentication)
            {
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationScheme = Constants.StuntmanAuthenticationType,
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
                            var principal = new ClaimsPrincipal(identity);

                            var authManager = context.Authentication;

                            await authManager.SignInAsync(Constants.StuntmanAuthenticationType, principal);

                            await next.Invoke();
                        }
                    });

                    RedirectToReturnUrl(signin);
                });

                app.Map(options.SignOutUri, signout =>
                {
                    signout.Use(async (context, next) =>
                    {
                        var authManager = context.Authentication;
                        await authManager.SignOutAsync(Constants.StuntmanAuthenticationType);

                        await next.Invoke();
                    });

                    RedirectToReturnUrl(signout);
                });
            }

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
            HttpContext context,
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

        private static void RedirectToReturnUrl(IApplicationBuilder app)
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

                context.Response.Headers.Add("Location", returnUrl);

                context.Response.StatusCode = 302;

                return Task.FromResult(true);
            });
        }

        private static async void ShowLoginUI(
            HttpContext context,
            StuntmanOptions options)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;

            var css = Resources.GetCss();
            var logoForInlining = Resources.GetLogoForInlining();
            var usersHtml = GetUsersLoginUI(context, options);

            await context.Response.WriteAsync($@"
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

        private static async Task StuntmanOnMessageReceived(
            StuntmanOptions options,
            MessageReceivedContext context)
        {
            var authorizationBearerToken = context.HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorizationBearerToken))
            {
                context.Ticket = null;
                context.SkipToNextMiddleware();
                return;
            }
            else
            {
                var authorizationBearerTokenParts = authorizationBearerToken.ToString().Split(' ');

                var accessToken = authorizationBearerTokenParts
                    .LastOrDefault();

                var claims = new List<Claim>();
                StuntmanUser user = null;

                if (authorizationBearerTokenParts.Count() != 2 ||
                    string.IsNullOrWhiteSpace(accessToken))
                {
                    context.HttpContext.Response.StatusCode = 400;
                    await context.HttpContext.Response.WriteAsync(
                        "Authorization header is not in correct format.");

                    context.Ticket = null;
                    context.SkipToNextMiddleware();
                    return;
                }
                else
                {
                    user = options.Users
                        .Where(x => x.AccessToken == accessToken)
                        .FirstOrDefault();

                    if (user == null)
                    {
                        if (!options.AllowBearerTokenPassthrough)
                        {
                            context.Response.StatusCode = 403;
                            await context.HttpContext.Response.WriteAsync(
                                $"options provided does not include the requested '{accessToken}' user.");
                        }

                        context.Ticket = null;
                        context.SkipToNextMiddleware();
                        return;
                    }
                    else
                    {
                        claims.Add(new Claim("access_token", accessToken));
                    }
                }

                claims.Add(new Claim(ClaimTypes.Name, user.Name));
                claims.AddRange(user.Claims);

                var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Constants.StuntmanAuthenticationType));

                context.Ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Constants.StuntmanAuthenticationType);

                context.HandleResponse();

                if (options.AfterBearerValidateIdentity != null)
                {
                    options.AfterBearerValidateIdentity(context);
                }
            }
        }
    }
}
