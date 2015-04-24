using System;
using System.Linq;
using Xunit;

namespace RimDev.Stuntman.Core.Tests
{
    public class StuntmanUsersTests
    {
        public class Constructor
        {
            [Theory]
            [InlineData(null, "some-name")]
            [InlineData("some-id", null)]
            public void ThrowsForNullArguments(string id, string name)
            {
                Assert.Throws<ArgumentNullException>(
                    () => new StuntmanUser(id, name));
            }

            [Fact]
            public void SetsId()
            {
                var user = new StuntmanUser("user-1", "User 1");

                Assert.Equal("user-1", user.Id);
            }

            [Fact]
            public void SetsName()
            {
                var user = new StuntmanUser("user-1", "User 1");

                Assert.Equal("User 1", user.Name);
            }

            [Fact]
            public void InitializesClaimsCollection()
            {
                var user = new StuntmanUser("user-1", "User 1");

                Assert.NotNull(user.Claims);
            }
        }

        public class AddClaimMethod
        {
            [Theory]
            [InlineData(null, "some-value")]
            [InlineData("some-type", null)]
            public void ThrowsForNullArguments(string type, string value)
            {
                Assert.Throws<ArgumentNullException>(
                    () => new StuntmanUser("user-1", "User 1")
                        .AddClaim(type, value));
            }

            [Fact]
            public void AddsExpectedClaim()
            {
                var user = new StuntmanUser("user-1", "User 1")
                    .AddClaim("some_type", "some_value");

                var claim = user.Claims.Single();
                Assert.Equal("some_type", claim.Type);
                Assert.Equal("some_value", claim.Value);
            }
        }
    }
}
