#if NET461

using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
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
        /// <summary>
        /// Enable Stuntman on this application.
        /// </summary>
        public static void UseStuntman(this IAppBuilder app, StuntmanOptions options)
        {
            if (options.AllowBearerTokenAuthentication)
            {
                app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
                {
                    AuthenticationType = Constants.StuntmanAuthenticationType,
                    Provider = new StuntmanOAuthBearerProvider(options),
                    AccessTokenFormat = new StuntmanOAuthAccessTokenFormat()
                });
            }

            if (options.AllowCookieAuthentication)
            {
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

                            var authManager = context.Authentication;

                            authManager.SignIn(identity);

                            await next.Invoke();
                        }
                    });

                    IAppBuilderShared.RedirectToReturnUrl(signin);
                });

                app.Map(options.SignOutUri, signout =>
                {
                    signout.Use((context, next) =>
                    {
                        var authManager = context.Authentication;
                        authManager.SignOut(Constants.StuntmanAuthenticationType);

                        return next.Invoke();
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
                            if (!options.AllowBearerTokenPassthrough)
                            {
                                context.Response.StatusCode = 403;
                                context.Response.ReasonPhrase =
                                    $"options provided does not include the requested '{accessToken}' user.";

                                context.Rejected();
                            }

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

#endif
