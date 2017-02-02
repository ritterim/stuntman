using Microsoft.Owin.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RimDev.Stuntman.Core.Tests
{
    public partial class StuntmanOptionsTests
    {
        public partial class AddUsersFromJsonMethod
        {
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
        }
    }
}
