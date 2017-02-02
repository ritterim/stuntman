using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RimDev.Stuntman.Core.Tests
{
    public partial class StuntmanOptionsTests
    {
        public class SignInUriProperty
        {
            [Fact]
            public void BeginsWithDefaultRootPathWhenNotSpecified()
            {
                var sut = new StuntmanOptions();

                Assert.StartsWith(Constants.StuntmanOptions.DefaultStuntmanRootPath, sut.SignInUri);
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

                Assert.EndsWith(Constants.StuntmanOptions.SignInEndpoint, sut.SignInUri);
            }
        }

        public class SignOutUriProperty
        {
            [Fact]
            public void BeginsWithDefaultRootPathWhenNotSpecified()
            {
                var sut = new StuntmanOptions();

                Assert.StartsWith(Constants.StuntmanOptions.DefaultStuntmanRootPath, sut.SignOutUri);
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

                Assert.EndsWith(Constants.StuntmanOptions.SignOutEndpoint, sut.SignOutUri);
            }
        }

        public class UserPickerAlignmentProperty
        {
            [Fact]
            public void DefaultsToStuntmanAlignmentLeft()
            {
                var sut = new StuntmanOptions();

                Assert.Equal(StuntmanAlignment.Left, sut.UserPickerAlignment);
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

                var exception = Assert.Throws<Exception>(() =>
                    {
                        sut.AddUser(user);
                    });

                Assert.Equal("user must have unique Id.", exception.Message);
            }

            [Fact]
            public void SetsSourceToLocalSource()
            {
                var sut = new StuntmanOptions();

                sut.AddUser(new StuntmanUser("user-1", "User 1"));

                var user = sut.Users.Single();

                Assert.Equal(Constants.StuntmanOptions.LocalSource, user.Source);
            }
        }

        public partial class AddUsersFromJsonMethod
        {
            [Fact]
            public void ThrowsForNullPathOrUrl()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new StuntmanOptions().AddUsersFromJson(null));
            }

            [Theory,
                InlineData(""),
                InlineData(" "),
                InlineData("../file.json"),
                InlineData("..\\file.json"),
            ]
            public void ThrowsForInvalidUri(string pathOrUrl)
            {
                Assert.Throws<UriFormatException>(
                    () => new StuntmanOptions().AddUsersFromJson(pathOrUrl));
            }

            [Fact]
            public void ThrowsForInvalidJson()
            {
                var json = @"{{""}}{{";

                Assert.Throws<JsonReaderException>(
                    () => new StuntmanOptions(stuntmanOptionsRetriever: new TestStuntmanOptionsRetriever(json))
                        .AddUsersFromJson("C:\\test.json"));
            }
        }

        public class AddConfigurationFromServer
        {
            [Fact]
            public void ThrowsForNullServerBaseUrl()
            {
                Assert.Throws<ArgumentNullException>(
                    "serverBaseUrl",
                    () => new StuntmanOptions().AddConfigurationFromServer(null));
            }

            [Theory,
                InlineData(""),
                InlineData(" "),
                InlineData("file.json")
            ]
            public void ThrowsForInvalidUri(string serverBaseUrl)
            {
                Assert.Throws<UriFormatException>(
                    () => new StuntmanOptions().AddConfigurationFromServer(serverBaseUrl));
            }

            [Fact]
            public void ThrowsForUnexpectedResponse()
            {
                var options = new StuntmanOptions(stuntmanOptionsRetriever: new TestStuntmanOptionsRetriever(
                    webClientStringToReturn: "some_error"));

                Assert.Throws<JsonReaderException>(
                    () => options.AddConfigurationFromServer("https://example.com"));
            }

            [Fact]
            public void AddsStuntmanServerResponseUsers()
            {
                const string Id1 = "user-1";

                var stuntmanServerResponse = new StuntmanServerResponse
                {
                    Users = new[]
                    {
                        new StuntmanUser(Id1, "User 1")
                    }
                };

                var json = JsonConvert.SerializeObject(stuntmanServerResponse);

                var options = new StuntmanOptions(stuntmanOptionsRetriever: new TestStuntmanOptionsRetriever(
                    webClientStringToReturn: json));

                options.AddConfigurationFromServer("https://example.com");

                Assert.Equal(Id1, options.Users.Single().Id);
            }

            [Fact]
            public void SetsSourceToserverBaseUrl()
            {
                const string ServerBaseUrl = "https://example.com";

                var stuntmanServerResponse = new StuntmanServerResponse
                {
                    Users = new[]
                    {
                        new StuntmanUser("user-1", "User 1")
                    }
                };

                var json = JsonConvert.SerializeObject(stuntmanServerResponse);

                var options = new StuntmanOptions(stuntmanOptionsRetriever: new TestStuntmanOptionsRetriever(
                    webClientStringToReturn: json));

                options.AddConfigurationFromServer(ServerBaseUrl);

                Assert.Equal(ServerBaseUrl, options.Users.Single().Source);
            }
        }

        public class TryAddConfigurationFromServer
        {
            [Fact]
            public void ThrowsForNullServerBaseUrl()
            {
                Assert.Throws<ArgumentNullException>(
                    "serverBaseUrl",
                    () => new StuntmanOptions().TryAddConfigurationFromServer(null));
            }

            [Fact]
            public void DoesNotThrowForUnexpectedResponse()
            {
                var options = new StuntmanOptions(stuntmanOptionsRetriever: new TestStuntmanOptionsRetriever(
                    webClientStringToReturn: "some_error"));

                options.TryAddConfigurationFromServer("https://example.com");
            }

            [Fact]
            public void ProcessesStuntmanServerResponse()
            {
                const string Id1 = "user-1";

                var stuntmanServerResponse = new StuntmanServerResponse
                {
                    Users = new[]
                    {
                        new StuntmanUser(Id1, "User 1")
                    }
                };

                var json = JsonConvert.SerializeObject(stuntmanServerResponse);

                var options = new StuntmanOptions(stuntmanOptionsRetriever: new TestStuntmanOptionsRetriever(
                    webClientStringToReturn: json));

                options.TryAddConfigurationFromServer("https://example.com");

                Assert.Equal(Id1, options.Users.Single().Id);
            }

            [Fact]
            public void SetsSourceToserverBaseUrl()
            {
                const string ServerBaseUrl = "https://example.com";

                var stuntmanServerResponse = new StuntmanServerResponse
                {
                    Users = new[]
                    {
                        new StuntmanUser("user-1", "User 1")
                    }
                };

                var json = JsonConvert.SerializeObject(stuntmanServerResponse);

                var options = new StuntmanOptions(stuntmanOptionsRetriever: new TestStuntmanOptionsRetriever(
                    webClientStringToReturn: json));

                options.TryAddConfigurationFromServer(ServerBaseUrl);

                Assert.Equal(ServerBaseUrl, options.Users.Single().Source);
            }

            [Fact]
            public void InvokesonExceptionWhenExceptionThrown()
            {
                var options = new StuntmanOptions(stuntmanOptionsRetriever: new TestStuntmanOptionsRetriever(
                    webClientStringToReturn: "error"));

                Exception actualException = null;

                options.TryAddConfigurationFromServer(
                    "https://example.com", (ex) => { actualException = ex; });

                Assert.NotNull(actualException);
                Assert.Equal(
                    "Unexpected character encountered while parsing value: "
                        + "e. Path '', line 0, position 0.",
                    actualException.Message);
            }
        }

        public class SetUserPickerAlignmentMethod
        {
            [Theory,
                InlineData(StuntmanAlignment.Left),
                InlineData(StuntmanAlignment.Center),
                InlineData(StuntmanAlignment.Right)]
            public void SetsUserPickerAlignment(StuntmanAlignment alignment)
            {
                var sut = new StuntmanOptions();

                sut.SetUserPickerAlignment(alignment);

                Assert.Equal(alignment, sut.UserPickerAlignment);
            }
        }

        public class UserPickerMethod_IPrincipal
        {
            // Rely on tests in UserPickerTests
        }

        public class UserPickerMethod_IPrincipal_String
        {
            // Rely on tests in UserPickerTests
        }

        private class TestStuntmanOptionsRetriever : StuntmanOptionsRetriever
        {
            private readonly string _localFileStringToReturn;
            private readonly string _webClientStringToReturn;

            public TestStuntmanOptionsRetriever(
                string localFileStringToReturn = null,
                string webClientStringToReturn = null)
            {
                _localFileStringToReturn = localFileStringToReturn;
                _webClientStringToReturn = webClientStringToReturn;
            }

            public override string GetStringFromLocalFile(Uri uri)
            {
                if (_localFileStringToReturn == null)
                {
                    return base.GetStringFromLocalFile(uri);
                }

                return _localFileStringToReturn;
            }

            public override string GetStringUsingWebClient(Uri uri)
            {
                if (_webClientStringToReturn == null)
                {
                    return base.GetStringUsingWebClient(uri);
                }

                return _webClientStringToReturn;
            }
        }
    }
}
