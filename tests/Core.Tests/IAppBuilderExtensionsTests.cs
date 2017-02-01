using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                    var response = await server.HttpClient.GetAsync(
                        $"{options.SignInUri}?{Constants.StuntmanOptions.ReturnUrlQueryStringKey}=https://app");

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
                    var response = await server.HttpClient.GetAsync(
                        $"{options.SignInUri}?{Constants.StuntmanOptions.ReturnUrlQueryStringKey}=https://app");

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
                    var response = await server.HttpClient.GetAsync(
                        $"{options.SignInUri}?{Constants.StuntmanOptions.OverrideQueryStringKey}=user-1");

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
                    var response = await server.HttpClient.GetAsync(
                        $"{options.SignInUri}?{Constants.StuntmanOptions.OverrideQueryStringKey}=user-1&{Constants.StuntmanOptions.ReturnUrlQueryStringKey}=https://app");

                    var setCookie = response.Headers.GetValues("Set-Cookie").SingleOrDefault();

                    Assert.StartsWith(
                        $".AspNet.{Constants.StuntmanAuthenticationType}=",
                        setCookie);
                }
            }

            [Fact]
            public async Task SignInUri_ReturnsExpectedLocationHeader_UsingReferer()
            {
                const string RedirectUri = "https://redirect-uri/";

                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var request = new HttpRequestMessage
                    {
                        RequestUri = new Uri(new Uri("https://app"), $"{options.SignInUri}"),
                        Method = HttpMethod.Get
                    };

                    request.Headers.Referrer = new Uri(RedirectUri);

                    var response = await server.HttpClient.SendAsync(request);

                    Assert.Equal(RedirectUri, response.Headers.Location.AbsoluteUri);
                }
            }

            [Fact]
            public async Task SignInUri_ReturnsExpectedLocationHeader_WhenQueryStringUsed()
            {
                const string RedirectUri = "https://redirect-uri/";

                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var response = await server.HttpClient.GetAsync(
                        $"{options.SignInUri}?{Constants.StuntmanOptions.ReturnUrlQueryStringKey}={RedirectUri}");

                    Assert.Equal(RedirectUri, response.Headers.Location.AbsoluteUri);
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
                    var response = await server.HttpClient.GetAsync(
                        $"{options.SignOutUri}?{Constants.StuntmanOptions.ReturnUrlQueryStringKey}=https://app");

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
                    var response = await server.HttpClient.GetAsync(
                        $"{options.SignOutUri}?{Constants.StuntmanOptions.ReturnUrlQueryStringKey}=https://app");

                    var setCookie = response.Headers.GetValues("Set-Cookie").SingleOrDefault();

                    Assert.Equal(
                        $".AspNet.{Constants.StuntmanAuthenticationType}=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT",
                        setCookie);
                }
            }

            [Fact]
            public async Task SignOutUri_ReturnsExpectedLocationHeader_UsingReferer()
            {
                const string RedirectUri = "https://redirect-uri/";

                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var request = new HttpRequestMessage
                    {
                        RequestUri = new Uri(new Uri("https://app"), $"{options.SignOutUri}"),
                        Method = HttpMethod.Get
                    };

                    request.Headers.Referrer = new Uri(RedirectUri);

                    var response = await server.HttpClient.SendAsync(request);

                    Assert.Equal(RedirectUri, response.Headers.Location.AbsoluteUri);
                }
            }

            [Fact]
            public async Task SignOutUri_ReturnsExpectedLocationHeader_WhenQueryStringUsed()
            {
                const string RedirectUri = "https://redirect-uri/";

                var options = new StuntmanOptions();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var response = await server.HttpClient.GetAsync(
                        $"{options.SignOutUri}?{Constants.StuntmanOptions.ReturnUrlQueryStringKey}={RedirectUri}");

                    Assert.Equal(RedirectUri, response.Headers.Location.AbsoluteUri);
                }
            }

            [Fact]
            public async Task ServerEndpoint_ReturnsExpectedResponse_WhenEnabled()
            {
                var options = new StuntmanOptions()
                    .EnableServer();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var response = await server.HttpClient.GetAsync(
                        options.ServerUri);

                    response.EnsureSuccessStatusCode();

                    var stuntmanServerResponse = JsonConvert.DeserializeObject<StuntmanServerResponse>(
                        await response.Content.ReadAsStringAsync());

                    Assert.NotNull(stuntmanServerResponse);
                }
            }

            [Fact]
            public async Task ServerEndpoint_ReturnsExpectedResponse_WhenEnabledWithUsers()
            {
                const string UserId = "user-1";

                var options = new StuntmanOptions()
                    .AddUser(new StuntmanUser(UserId, "User 1"))
                    .EnableServer();

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var response = await server.HttpClient.GetAsync(
                        options.ServerUri);

                    response.EnsureSuccessStatusCode();

                    var stuntmanServerResponse = JsonConvert.DeserializeObject<StuntmanServerResponse>(
                        await response.Content.ReadAsStringAsync());

                    Assert.Equal(UserId, stuntmanServerResponse.Users.Single().Id);
                }
            }

            [Fact]
            public async Task ServerEndpoint_Returns404_WhenNotEnabled()
            {
                var options = new StuntmanOptions();

                Assert.False(options.ServerEnabled);

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var response = await server.HttpClient.GetAsync(
                        options.ServerUri);

                    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                }
            }

            [Fact]
            public async Task DoesNotMapSignInUri_WhenCookieAuthenticationIsDiabled()
            {
                var options = new StuntmanOptions();

                options.AllowCookieAuthentication = false;

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                }))
                {
                    var response = await server.HttpClient.GetAsync(options.SignInUri);

                    Assert.Equal(404, (int)response.StatusCode);
                }
            }

            [Fact]
            public async Task DoesNotRedirectChallenge_WhenCookieAuthenticationIsDiabled()
            {
                var options = new StuntmanOptions();

                options.AllowCookieAuthentication = false;

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);

                    app.Map("/test", root =>
                    {
                        root.Run(context =>
                        {
                            /**
                             * Issuing a challenge for a non-existent authentication type
                             * will result in a 401.
                             *
                             * Normal behavior results in a 302 to redirect to the login UI.
                             */
                            context.Authentication.Challenge(Constants.StuntmanAuthenticationType);

                            return Task.FromResult(true);
                        });
                    });
                }))
                {
                    var response = await server.HttpClient.GetAsync("test");

                    Assert.Equal(401, (int)response.StatusCode);
                }
            }

            [Fact]
            public async Task DoesNotHydrateIdentity_WhenBearerTokenAuthenticationIsDiabled()
            {
                var options = new StuntmanOptions()
                    .AddUser(
                    new StuntmanUser("user-1", "User 1")
                    .SetAccessToken("123"));

                options.AllowBearerTokenAuthentication = false;

                var authenticateResult = new AuthenticateResult(null, new AuthenticationProperties(), new AuthenticationDescription());

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);

                    app.Map("/test", root =>
                    {
                        root.Run(async context =>
                        {
                            /**
                             * This result of `AuthenticateAsync` should be `null` since
                             * nothing is handling the `Authorization` header during the request.
                             */
                            authenticateResult = await context.Authentication.AuthenticateAsync(Constants.StuntmanAuthenticationType);
                        });
                    });
                }))
                {
                    var request = server.CreateRequest("test");

                    request.AddHeader("Authorization", "Bearer 123");

                    var response = await request.GetAsync();

                    Assert.Null(authenticateResult);
                }
            }

            [Fact]
            public async Task DoesNotImmediatelyIssue403_WhenBearerTokenPassthroughIsEnabled()
            {
                var options = new StuntmanOptions()
                    .AddUser(
                    new StuntmanUser("user-1", "User 1")
                    .SetAccessToken("123"));

                options.AllowBearerTokenPassthrough = true;
                options.AllowCookieAuthentication = false;

                using (var server = TestServer.Create(app =>
                {
                    app.UseStuntman(options);
                    app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
                    {
                        Challenge = "Bearer StuntmanTest",
                        AuthenticationMode = AuthenticationMode.Active,
                        AuthenticationType = Constants.StuntmanAuthenticationType
                    });

                    app.Use((context, next) =>
                    {
                        if (context.Authentication.User != null &&
                            context.Authentication.User.Identity != null &&
                            context.Authentication.User.Identity.IsAuthenticated)
                        {
                            return next();
                        }
                        else
                        {
                            context.Authentication.Challenge(Constants.StuntmanAuthenticationType);

                            return Task.FromResult(false);
                        }
                    });

                    app.Map("/test", root =>
                    {
                        root.Run(context =>
                        {
                            return Task.FromResult(true);
                        });
                    });
                }))
                {
                    var request = server.CreateRequest("test");

                    request.AddHeader("Authorization", "Bearer 1234");

                    var response = await request.GetAsync();

                    Assert.Equal(401, (int)response.StatusCode);
                    Assert.Contains("StuntmanTest", response.Headers.WwwAuthenticate.Select(x => x.Parameter));
                }
            }
        }
    }
}
