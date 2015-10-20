using System;
using System.Net;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Xunit;

namespace RimDev.Stuntman.Core.Tests
{
    public class UserPickerTests
    {
        public class GetHtmlMethod
        {
            [Fact]
            public void ThrowsForNullCurrentPrincipal()
            {
                var options = StuntmanOptionsFactory.GetDefaultTestOptions();

                Assert.Throws<ArgumentNullException>(
                    () => new UserPicker(options)
                        .GetHtml(null, "https://return-uri/"));
            }

            [Fact]
            public void ThrowsForNullReturnUrl()
            {
                var options = StuntmanOptionsFactory.GetDefaultTestOptions();

                Assert.Throws<ArgumentNullException>(
                    () => new UserPicker(options)
                        .GetHtml(new TestPrincipal(), null));
            }

            [Fact]
            public void ReturnsExpectedItemFormat()
            {
                var options = StuntmanOptionsFactory.GetDefaultTestOptions()
                    .AddUser(new StuntmanUser("user-1", "User 1"));

                var returnUrl = "https://return-url";

                var html = new UserPicker(options).GetHtml(new TestPrincipal(), returnUrl);

                Assert.Contains(
                    $"/stuntman/{StuntmanOptions.SignInEndpoint}?{StuntmanOptions.OverrideQueryStringKey}=user-1&{StuntmanOptions.ReturnUrlQueryStringKey}={WebUtility.UrlEncode(returnUrl)}",
                    html);
            }

            [Fact]
            public void ReturnsUsers()
            {
                var options = StuntmanOptionsFactory.GetDefaultTestOptions()
                    .AddUser(new StuntmanUser("user-1", "User 1"))
                    .AddUser(new StuntmanUser("user-2", "User 2"));

                var html = new UserPicker(options).GetHtml(new TestPrincipal(), "https://return-url");

                Assert.Contains("user-1", html);
                Assert.Contains("User 1", html);

                Assert.Contains("user-2", html);
                Assert.Contains("User 2", html);
            }

            [Fact]
            public void ReturnsLogout()
            {
                var options = StuntmanOptionsFactory.GetDefaultTestOptions();

                var html = new UserPicker(options).GetHtml(new TestPrincipal(), "https://return-url");

                Assert.Contains("Logout", html);
            }

            [Fact]
            public void ReturnsExpectedNumberOfTotalItems()
            {
                var options = StuntmanOptionsFactory.GetDefaultTestOptions()
                    .AddUser(new StuntmanUser("user-1", "User 1"));

                var html = new UserPicker(options).GetHtml(new TestPrincipal(), "https://return-url");

                Assert.Equal(2, Regex.Matches(html, "<li.*>", RegexOptions.Multiline).Count);
            }
        }

        private class TestPrincipal : IPrincipal
        {
            public IIdentity Identity
            {
                get { return new TestIdentity(); }
            }

            public bool IsInRole(string role)
            {
                throw new NotImplementedException();
            }
        }

        private class TestIdentity : IIdentity
        {
            public string AuthenticationType
            {
                get { return "TestIdentity"; }
            }

            public bool IsAuthenticated
            {
                get { return true; }
            }

            public string Name
            {
                get { return "TestIdentity User"; }
            }
        }
    }
}
