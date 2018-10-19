using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

#if NET461

using Microsoft.Owin;
using Owin;

#endif

#if NETCOREAPP2_1

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

#endif

namespace RimDev.Stuntman.Core
{
    internal class IAppBuilderShared
    {
        internal static string GetUsersLoginUI(
#if NET461
            IOwinContext context,
#endif
#if NETCOREAPP2_1
            HttpContext context,
#endif
            StuntmanOptions options)
        {
            var usersHtml = new StringBuilder();

            foreach (var user in options.Users)
            {
                var returnUrl = context.Request.Query[Constants.StuntmanOptions.ReturnUrlQueryStringKey]
#if NETCOREAPP2_1
                    .ToString()
#endif
                ;

                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    returnUrl = context.Request.Headers["Referer"].ToString();
                }

                var href = $"{options.SignInUri}?OverrideUserId={user.Id}"
                    + $"&{Constants.StuntmanOptions.ReturnUrlQueryStringKey}={WebUtility.UrlEncode(returnUrl)}";

                usersHtml.Append($@"<li><a href=""{href}"" title=""{(string.IsNullOrEmpty(user.Description) ? null : WebUtility.HtmlEncode(user.Description) + " ")}Source: {user.Source}"">{user.Name}</a></li>");
            }

            return usersHtml.ToString();
        }

        internal static void RedirectToReturnUrl(
#if NET461
            IAppBuilder app
#endif
#if NETCOREAPP2_1
            IApplicationBuilder app
#endif
            )
        {
            app.Run(context =>
            {
                var returnUrl = context.Request.Query[Constants.StuntmanOptions.ReturnUrlQueryStringKey]
#if NETCOREAPP2_1
                    .ToString()
#endif
                    ;

                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    returnUrl = context.Request.Headers["Referer"].ToString();
                }

                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    throw new InvalidOperationException(
                        "ReturnUrl was not specified via query string or Referer header.");
                }

                if (context.Response.Headers.ContainsKey("Location"))
                {
                    context.Response.Headers.Remove("Location");
                }

                context.Response.Headers.Add(
                    "Location",
#if NET461
                    new[] { returnUrl }
#endif
#if NETCOREAPP2_1
                    returnUrl
#endif
                );

                context.Response.StatusCode = 302;

                return Task.FromResult(true);
            });
        }

        internal static void ShowLoginUI(
#if NET461
            IOwinContext context,
#endif
#if NETCOREAPP2_1
            HttpContext context,
#endif
            StuntmanOptions options)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;

            var css = Resources.GetCss();
            var logoForInlining = Resources.GetLogoForInlining();
            var usersHtml = GetUsersLoginUI(context, options);

            var html = $@"
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
</html>";

#if NET461
            context.Response.Write(html);
#endif
#if NETCOREAPP2_1
            context.Response.WriteAsync(html).ConfigureAwait(false).GetAwaiter().GetResult();
#endif
        }
    }
}
