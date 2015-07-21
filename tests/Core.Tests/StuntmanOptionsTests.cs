using System;
using System.Linq;
using Xunit;

namespace RimDev.Stuntman.Core.Tests
{
    public class StuntmanOptionsTests
    {
        public class NonDebugUsageAllowedProperty
        {
            [Fact]
            public void FalseByDefault()
            {
                var sut = new StuntmanOptions();

                Assert.False(sut.NonDebugUsageAllowed);
            }

            [Fact]
            public void TrueWhenAllowNonDebugUsageIsInvoked()
            {
                var sut = new StuntmanOptions()
                    .AllowNonDebugUsage();

                Assert.True(sut.NonDebugUsageAllowed);
            }
        }

        public class SignInUriProperty
        {
            [Fact]
            public void BeginsWithDefaultRootPathWhenNotSpecified()
            {
                var sut = new StuntmanOptions();

                Assert.StartsWith(StuntmanOptions.DefaultStuntmanRootPath, sut.SignInUri);
            }

            [Fact]
            public void BeginsWithSpecifiedRootPath()
            {
                var sut = new StuntmanOptions("custom/root/path");

                Assert.StartsWith("custom/root/path", sut.SignInUri);
            }

            [Fact]
            public void AddsTrailingSlashToRootPath()
            {
                var sut = new StuntmanOptions("custom/root/path");

                Assert.StartsWith("custom/root/path/", sut.SignInUri);
            }

            [Fact]
            public void EndsWithCorrectSuffix()
            {
                var sut = new StuntmanOptions();

                Assert.EndsWith(StuntmanOptions.SignInEndpoint, sut.SignInUri);
            }
        }

        public class SignOutUriProperty
        {
            [Fact]
            public void BeginsWithDefaultRootPathWhenNotSpecified()
            {
                var sut = new StuntmanOptions();

                Assert.StartsWith(StuntmanOptions.DefaultStuntmanRootPath, sut.SignOutUri);
            }

            [Fact]
            public void BeginsWithSpecifiedRootPath()
            {
                var sut = new StuntmanOptions("custom/root/path");

                Assert.StartsWith("custom/root/path", sut.SignOutUri);
            }

            [Fact]
            public void AddsTrailingSlashToRootPath()
            {
                var sut = new StuntmanOptions("custom/root/path");

                Assert.StartsWith("custom/root/path/", sut.SignOutUri);
            }

            [Fact]
            public void EndsWithCorrectSuffix()
            {
                var sut = new StuntmanOptions();

                Assert.EndsWith(StuntmanOptions.SignOutEndpoint, sut.SignOutUri);
            }
        }

        public class AddUserMethod
        {
            [Fact]
            public void ThrowsForNullUser()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new StuntmanOptions().AddUser(null));
            }

            [Fact]
            public void AddsUser()
            {
                var sut = new StuntmanOptions();

                sut.AddUser(new StuntmanUser("user-1", "User 1"));

                var user = sut.Users.Single();

                Assert.Equal("user-1", user.Id);
                Assert.Equal("User 1", user.Name);
            }

            [Fact]
            public void ThrowsForDuplicateId()
            {
                var sut = new StuntmanOptions();
                var user = new StuntmanUser("user-1", "User 1");

                sut.AddUser(user);

                var exception = Assert.Throws<ApplicationException>(() =>
                    {
                        sut.AddUser(user);
                    });

                Assert.Equal("user must have unique Id.", exception.Message);
            }
        }

        public class AllowNonDebugUsageMethod
        {
            [Fact]
            public void UpdatesNonDebugUsageAllowedProperty()
            {
                var sut = new StuntmanOptions();

                sut.AllowNonDebugUsage();

                Assert.True(sut.NonDebugUsageAllowed);
            }
        }

        public class VerifyUsageIsPermittedMethod
        {
            [Fact]
            public void ThrowsWhenDebugIsFalse()
            {
                var sut = new StuntmanOptions(isDebug: () => false);

                Assert.Throws<InvalidOperationException>(
                    () => sut.VerifyUsageIsPermitted());
            }

            [Fact]
            public void DoesNotThrowWhenDebugIsFalseAndAllowNonDebugUsageIsInvoked()
            {
                var sut = new StuntmanOptions(isDebug: () => false)
                    .AllowNonDebugUsage();

                sut.VerifyUsageIsPermitted();
            }

            [Fact]
            public void DoesNotThrowWhenAndDebugIsTrue()
            {
                var sut = new StuntmanOptions(isDebug: () => true);

                sut.VerifyUsageIsPermitted();
            }

            [Fact]
            public void DoesNotThrowWhenDebugIsTrueAndAllowNonDebugUsageIsInvoked()
            {
                var sut = new StuntmanOptions(isDebug: () => true)
                    .AllowNonDebugUsage();

                sut.VerifyUsageIsPermitted();
            }
        }
    }
}
