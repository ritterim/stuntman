#if NETCOREAPP2_0

using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace RimDev.Stuntman.Core
{
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Use Stuntman on this application.
        /// </summary>
        public static void UseStuntman(this IApplicationBuilder app, StuntmanOptions options)
        {
            app.UseAuthentication();

            if (options.AllowCookieAuthentication)
            {
                app.Map(options.SignInUri, signin =>
                {
                    signin.Use(async (context, next) =>
                    {
                        var claims = new List<Claim>();

                        var overrideUserId = context.Request.Query[Constants.StuntmanOptions.OverrideQueryStringKey];

                        if (string.IsNullOrWhiteSpace(overrideUserId))
                        {
                            await next.Invoke();

                            IAppBuilderShared.ShowLoginUI(context, options);
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

                            await context.SignInAsync(Constants.StuntmanAuthenticationType, new ClaimsPrincipal(identity));

                            await next.Invoke();
                        }
                    });

                    IAppBuilderShared.RedirectToReturnUrl(signin);
                });

                app.Map(options.SignOutUri, signout =>
                {
                    signout.Use(async (context, next) =>
                    {
                        await context.SignOutAsync(Constants.StuntmanAuthenticationType);

                        await next.Invoke();
                    });

                    IAppBuilderShared.RedirectToReturnUrl(signout);
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
    }
}

#endif
