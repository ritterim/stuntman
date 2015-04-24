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
            public void ReturnsExpectedItemFormat()
            {
                var options = new StuntmanOptions
                {
                    Users = new[]
                    {
                        new StuntmanUser
                        {
                            Id = "user-1",
                            Name = "User 1",
                        }
                    }
                };

                var returnUrl = "https://return-url";

                var html = new UserPicker(options).GetHtml(new TestPrincipal(), returnUrl);

                Assert.Contains(
                    string.Format(
                        "/stuntman/{0}?{1}=user-1&{2}={3}",
                        StuntmanOptions.SignInEndpoint,
                        StuntmanOptions.OverrideQueryStringKey,
                        StuntmanOptions.ReturnUrlQueryStringKey,
                        WebUtility.UrlEncode(returnUrl)),
                    html);
            }

            [Fact]
            public void ReturnsUsers()
            {
                var options = new StuntmanOptions
                {
                    Users = new[]
                    {
                        new StuntmanUser
                        {
                            Id = "user-1",
                            Name = "User 1",
                        },
                        new StuntmanUser
                        {
                            Id = "user-2",
                            Name = "User 2",
                        }
                    }
                };

                var html = new UserPicker(options).GetHtml(new TestPrincipal(), "https://return-url");

                Assert.Contains("user-1", html);
                Assert.Contains("User 1", html);

                Assert.Contains("user-2", html);
                Assert.Contains("User 2", html);
            }

            [Fact]
            public void ReturnsLogout()
            {
                var options = new StuntmanOptions();

                var html = new UserPicker(options).GetHtml(new TestPrincipal(), "https://return-url");

                Assert.Contains("Logout", html);
            }

            [Fact]
            public void ReturnsExpectedNumberOfTotalItems()
            {
                var options = new StuntmanOptions
                {
                    Users = new[]
                    {
                        new StuntmanUser
                        {
                            Id = "user-1",
                            Name = "User 1",
                        }
                    }
                };

                var html = new UserPicker(options).GetHtml(new TestPrincipal(), "https://return-url");

                Assert.Equal(2, Regex.Matches(html, "<li>", RegexOptions.Multiline).Count);
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
