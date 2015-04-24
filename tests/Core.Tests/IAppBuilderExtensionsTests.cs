using Microsoft.Owin.Testing;
using System.Linq;
using System.Net;
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
