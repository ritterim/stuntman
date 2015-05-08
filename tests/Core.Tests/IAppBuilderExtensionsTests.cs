using Microsoft.Owin.Testing;
using Owin;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace RimDev.Stuntman.Core.Tests
{
    public class IAppBuilderExtensionsTests
    {
        public class UseStuntmanExtensionMethod
        {
            [Fact]
            public async Task SignInUri_Returns200Ok()
            {
                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var response = await server.HttpClient.GetAsync(options.SignInUri);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }
            }

            [Fact]
            public async Task SignInUri_IfOverrideNotSpecified_ShowsLoginUI()
            {
                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var response = await server.HttpClient.GetAsync(options.SignInUri);

                    Assert.Contains(
                        "Please select a user to continue authentication.",
                        await response.Content.ReadAsStringAsync());
                }
            }

            [Fact]
            public async Task SignInUri_OverrideSpecified_ThrowsIfNoMatchingUser()
            {
                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var url = string.Format("{0}?{1}={2}",
                        options.SignInUri,
                        StuntmanOptions.OverrideQueryStringKey,
                        "user-1");

                    var response = await server.HttpClient.GetAsync(url);

                    Assert.False(response.IsSuccessStatusCode);
                }
            }

            [Fact]
            public async Task SignInUri_OverrideSpecified_SetsExpectedCookieName()
            {
                var options = new StuntmanOptions()
                    .AddUser(new StuntmanUser("user-1", "User 1"));

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var url = string.Format("{0}?{1}={2}",
                        options.SignInUri,
                        StuntmanOptions.OverrideQueryStringKey,
                        "user-1");

                    var response = await server.HttpClient.GetAsync(url);
                    var setCookie = response.Headers.GetValues("Set-Cookie").SingleOrDefault();

                    Assert.StartsWith(
                        string.Format(".AspNet.{0}=", IAppBuilderExtensions.StuntmanAuthenticationType),
                        setCookie);
                }
            }

            [Theory,
            InlineData("Bearer 123 456"),
            InlineData("Bearer 123 456 789")]
            public async Task AuthorizationBearerToken_400IfNoCorrectFormat(string bearerToken)
            {
                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);

                    app.Map("", root =>
                    {
                        root.Run(context =>
                        {
                            return Task.FromResult(true);
                        });
                    });
                }))
                {
                    var request = server.CreateRequest("");

                    request.AddHeader("Authorization", bearerToken);

                    var response = await request.GetAsync();

                    Assert.Equal(400, (int)response.StatusCode);
                }
            }

            [Fact]
            public async Task AuthorizationBearerToken_ThrowsIfNoMatchingUser()
            {
                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);

                    app.Map("", root =>
                    {
                        root.Run(context =>
                        {
                            return Task.FromResult(true);
                        });
                    });
                }))
                {
                    var request = server.CreateRequest("");

                    request.AddHeader("Authorization", "Bearer 123");

                    var response = await request.GetAsync();

                    Assert.Equal(403, (int)response.StatusCode);
                }
            }

            [Fact]
            public async Task AuthorizationBearerToken_AddsTokenClaim()
            {
                var options = new StuntmanOptions()
                    .AddUser(
                    new StuntmanUser("user-1", "User 1")
                    .SetAccessToken("123"));
                IEnumerable<Claim> claims = null;

                options.AfterBearerValidateIdentity = (context) =>
                {
                    claims = context
                        .OwinContext
                        .Authentication
                        .AuthenticationResponseGrant
                        .Identity
                        .Claims;
                };

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var request = server.CreateRequest("");

                    request.AddHeader("Authorization", "Bearer 123");

                    await request.GetAsync();

                    Assert.NotNull(claims);

                    var accessToken = claims.FirstOrDefault(x => x.Type == "access_token");

                    Assert.NotNull(accessToken);
                    Assert.Equal("123", accessToken.Value);
                }
            }

            [Fact]
            public async Task SignOutUri_Returns302Redirect()
            {
                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var response = await server.HttpClient.GetAsync(options.SignOutUri);

                    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                }
            }

            [Fact]
            public async Task SignOutUri_RemovesExpectedCookieName()
            {
                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var response = await server.HttpClient.GetAsync(options.SignOutUri);
                    var setCookie = response.Headers.GetValues("Set-Cookie").SingleOrDefault();

                    Assert.Equal(
                        string.Format(
                            ".AspNet.{0}=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT",
                            IAppBuilderExtensions.StuntmanAuthenticationType),
                        setCookie);
                }
            }

            [Fact]
            public async Task SignOutUri_ReturnsExpectedLocationHeader()
            {
                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var url = string.Format("{0}?{1}={2}",
                        options.SignOutUri,
                        StuntmanOptions.ReturnUrlQueryStringKey,
                        "https://redirect-uri/");

                    var response = await server.HttpClient.GetAsync(url);

                    Assert.Equal("https://redirect-uri/", response.Headers.Location.AbsoluteUri);
                }
            }
        }
    }
}
