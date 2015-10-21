﻿using System;
using System.Linq;
using Xunit;

namespace RimDev.Stuntman.Core.Tests
{
    public class StuntmanUsersTests
    {
        public class IdAndNameConstructor
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
            public void ThrowsForEmptyIdArgument()
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => new StuntmanUser(string.Empty, "some-name"));

                Assert.Equal("id must not be empty or whitespace.", exception.Message);
            }

            [Fact]
            public void ThrowsForEmptyNameArgument()
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => new StuntmanUser("some-id", string.Empty));

                Assert.Equal("name must not be empty or whitespace.", exception.Message);
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

        public class NameConstructor
        {
            [Fact]
            public void ShouldGenerateId()
            {
                var user = new StuntmanUser("user-1");

                Assert.NotNull(user.Id);
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
            public void ThrowsForEmptyType()
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => new StuntmanUser("user-1", "User 1")
                        .AddClaim(string.Empty, "some-value"));

                Assert.Equal("type must not be empty or whitespace.", exception.Message);
            }

            [Fact]
            public void ThrowsForEmptyValue()
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => new StuntmanUser("user-1", "User 1")
                        .AddClaim("some-type", string.Empty));

                Assert.Equal("value must not be empty or whitespace.", exception.Message);
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

        public class SetAccessToken
        {
            [Fact]
            public void ThrowsForNullArguments()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new StuntmanUser("user-1", "User 1")
                        .SetAccessToken(null));
            }

            [Theory,
            InlineData(""),
            InlineData(" ")]
            public void ThrowsForEmptyValue(string accessToken)
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => new StuntmanUser("user-1", "User 1")
                        .SetAccessToken(accessToken));

                Assert.Equal("accessToken must not be empty or whitespace.", exception.Message);
            }
        }

        public class SetIconColorMethod_StringOverload
        {
            [Fact]
            public void ThrowsForNullArgument()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new StuntmanUser("user-1", "User 1")
                        .SetIconColor(null));
            }

            [Fact]
            public void ThrowsWhenMissingNumberSign()
            {
                const string hexColor = "ccc";

                var exception = Assert.Throws<ArgumentException>(
                    () => new StuntmanUser("user-1", "User 1")
                        .SetIconColor(hexColor));

                Assert.Equal($"{hexColor} must start with #.", exception.Message);
            }

            [Fact]
            public void SetsExpectedColor()
            {
                const string hexColor = "#ccc";

                var user = new StuntmanUser("user-1", "User 1")
                    .SetIconColor(hexColor);

                Assert.Equal(hexColor, user.IconColor);
            }
        }

        public class SetIconColorMethod_StuntmanColorOverload
        {
            [Fact]
            public void SetsExpectedColor()
            {
                var user = new StuntmanUser("user-1", "User 1")
                    .SetIconColor(StuntmanColor.DarkBlue);

                Assert.Equal("#002a4a", user.IconColor);
            }
        }
    }
}
