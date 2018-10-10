#if NETCOREAPP2_1

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RimDev.Stuntman.Core
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Add Stuntman to this application.
        /// </summary>
        public static void AddStuntman(this IServiceCollection services, StuntmanOptions options)
        {
            services.AddAuthentication(Constants.StuntmanAuthenticationType);

            var authBuilder = new AuthenticationBuilder(services);

            if (options.AllowBearerTokenAuthentication)
            {
                authBuilder.AddJwtBearer(
                    opts =>
                    {
                        opts.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context => StuntmanOnMessageReceived(options, context)
                        };
                    });
            }

            if (options.AllowCookieAuthentication)
            {
                authBuilder.AddCookie(
                    Constants.StuntmanAuthenticationType,
                    opts =>
                    {
                        opts.LoginPath = new PathString(options.SignInUri);
                        opts.LogoutPath = new PathString(options.SignOutUri);
                        opts.ReturnUrlParameter = Constants.StuntmanOptions.ReturnUrlQueryStringKey;
                    });
            }
        }

        private static async Task StuntmanOnMessageReceived(
            StuntmanOptions options,
            MessageReceivedContext context)
        {
            var authorizationBearerToken = context.HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorizationBearerToken))
            {
                // TODO: Skip to next middleware?
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

                    context.Fail(
                        "Authorization header is not in correct format.");
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

                        context.Fail(
                            $"options provided does not include the requested '{accessToken}' user.");
                        return;
                    }
                    else
                    {
                        claims.Add(new Claim("access_token", accessToken));
                    }
                }

                claims.Add(new Claim(ClaimTypes.Name, user.Name));
                claims.AddRange(user.Claims);

                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Constants.StuntmanAuthenticationType));
                context.Success();

                options.AfterBearerValidateIdentity?.Invoke(context);
            }
        }
    }
}

#endif
