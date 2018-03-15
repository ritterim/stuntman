using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

#if NET461

using Microsoft.Owin.Testing;

#endif

namespace RimDev.Stuntman.Core.Tests
{
    public class StuntmanOptionsTests
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

        public class AddUsersMethod
        {
            [Fact]
            public void ThrowsForNullUsers()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new StuntmanOptions().AddUsers(null));
            }

            [Fact]
            public void AddsUsers()
            {
                var sut = new StuntmanOptions();

                var users = Enumerable.Range(1, 10)
                    .Select(i => new StuntmanUser(i.ToString(), $"user-{i}"))
                    .ToList();

                var options = sut.AddUsers(users);

                Assert.Equal(10, options.Users.Count);
            }
        }

        public class AddUsersFromJsonMethod
        {

#if NET461

            [Fact]
            public async Task AddsUsersFromFileSystem()
            {
                const string Id1 = "user-1";
                const string Name1 = "Test Name 1";

                const string Id2 = "user-2";
                const string Name2 = "Test Name 2";

                var json = string.Empty;

                var stuntmanOptions = new StuntmanOptions()
                    .AddUser(new StuntmanUser(Id1, Name1))
                    .AddUser(new StuntmanUser(Id2, Name2));

                using (var server = TestServer.Create(app =>
                {
                    stuntmanOptions.EnableServer();

                    app.UseStuntman(stuntmanOptions);
                }))
                {
                    var response = await server.HttpClient.GetAsync(stuntmanOptions.ServerUri);

                    json = await response.Content.ReadAsStringAsync();
                }

                var options = new StuntmanOptions(
                    stuntmanOptionsRetriever:
                        new TestStuntmanOptionsRetriever(localFileStringToReturn: json))
                    .AddUsersFromJson("C:\\test.json");

                Assert.Equal(2, options.Users.Count);

                Assert.NotNull(options.Users.SingleOrDefault(x => x.Id == Id1));
                Assert.NotNull(options.Users.SingleOrDefault(x => x.Name == Name1));

                Assert.NotNull(options.Users.SingleOrDefault(x => x.Id == Id2));
                Assert.NotNull(options.Users.SingleOrDefault(x => x.Name == Name2));
            }

            [Fact]
            public async Task AddsUsersFromWebClientRequest()
            {
                const string Id1 = "user-1";
                const string Name1 = "Test Name 1";

                const string Id2 = "user-2";
                const string Name2 = "Test Name 2";

                var json = string.Empty;

                var stuntmanOptions = new StuntmanOptions()
                    .AddUser(new StuntmanUser(Id1, Name1))
                    .AddUser(new StuntmanUser(Id2, Name2));

                using (var server = TestServer.Create(app =>
                {
                    stuntmanOptions.EnableServer();

                    app.UseStuntman(stuntmanOptions);
                }))
                {
                    var response = await server.HttpClient.GetAsync(stuntmanOptions.ServerUri);

                    json = await response.Content.ReadAsStringAsync();
                }

                var options = new StuntmanOptions(
                    stuntmanOptionsRetriever:
                        new TestStuntmanOptionsRetriever(webClientStringToReturn: json))
                    .AddUsersFromJson("https://example.com");

                Assert.Equal(2, options.Users.Count);

                Assert.NotNull(options.Users.SingleOrDefault(x => x.Id == Id1));
                Assert.NotNull(options.Users.SingleOrDefault(x => x.Name == Name1));

                Assert.NotNull(options.Users.SingleOrDefault(x => x.Id == Id2));
                Assert.NotNull(options.Users.SingleOrDefault(x => x.Name == Name2));
            }

            [Fact]
            public async Task AddsUserClaims()
            {
                const string ClaimType = "TestClaim";
                const string ClaimValue = "TestClaimValue";

                var json = string.Empty;

                var stuntmanOptions = new StuntmanOptions()
                    .AddUser(new StuntmanUser("user-1", "Test Name 1")
                        .AddClaim(ClaimType, ClaimValue))
                    .AddUser(new StuntmanUser("user-2", "Test Name 2"));

                using (var server = TestServer.Create(app =>
                {
                    stuntmanOptions.EnableServer();

                    app.UseStuntman(stuntmanOptions);
                }))
                {
                    var response = await server.HttpClient.GetAsync(stuntmanOptions.ServerUri);

                    json = await response.Content.ReadAsStringAsync();
                }

                var options = new StuntmanOptions(
                    stuntmanOptionsRetriever:
                        new TestStuntmanOptionsRetriever(localFileStringToReturn: json))
                    .AddUsersFromJson("C:\\test.json");

                var user1 = options.Users.SingleOrDefault(x => x.Claims.Any());

                Assert.NotNull(user1);

                var testClaim = user1.Claims.Single();

                Assert.Equal(ClaimType, testClaim.Type);
                Assert.Equal(ClaimValue, testClaim.Value);
            }

            [Fact]
            public async Task AllowsOptionalConfigurationOfUsers()
            {
                const string TestClaim = "test-claim";
                const string TestClaimValue = "test-claim-value";

                var json = string.Empty;

                var stuntmanOptions = new StuntmanOptions()
                    .AddUser(new StuntmanUser("user-1", "Test Name 1"))
                    .AddUser(new StuntmanUser("user-2", "Test Name 2"));

                using (var server = TestServer.Create(app =>
                {
                    stuntmanOptions.EnableServer();

                    app.UseStuntman(stuntmanOptions);
                }))
                {
                    var response = await server.HttpClient.GetAsync(stuntmanOptions.ServerUri);

                    json = await response.Content.ReadAsStringAsync();
                }

                var options = new StuntmanOptions(
                    stuntmanOptionsRetriever:
                        new TestStuntmanOptionsRetriever(localFileStringToReturn: json))
                    .AddUsersFromJson(
                        "C:\\test.json",
                        user => user.Claims.Add(new Claim(TestClaim, TestClaimValue)));

                Assert.True(options.Users.Any());
                Assert.True(options.Users.All(
                    x => x.Claims.Count(y => y.Type == TestClaim && y.Value == TestClaimValue) == 1));
            }

#endif

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
