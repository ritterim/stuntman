![Stuntman logo](https://raw.githubusercontent.com/ritterim/stuntman/gh-pages/images/stuntman-logo.png)

> "Sometimes, you need a stand-in."

**Stuntman** is a library for impersonating users during development leveraging .NET Claims Identity. Used primarily in web environments like ASP.NET MVC, ASP.NET Web Forms, and OWIN applications that serve HTML. This allows you to test different user scenarios that exist in your application with minimal friction. It also allows you to share those scenarios with other team members via source control.

![working](https://cloud.githubusercontent.com/assets/3382469/7323032/df5e60da-ea79-11e4-9af7-0fc55c64733a.gif)

## Installation

Install the [RimDev.Stuntman](https://www.nuget.org/packages/RimDev.Stuntman/) NuGet package.

```
Install-Package RimDev.Stuntman
```

## Usage

### Startup / Middleware registration

Stuntman uses OWIN and is registered as middleware, and allows for programmatically preset user scenarios, in the form of claims identities. These presets can be utilized by you or other team members working on the same code base.

```csharp
// OWIN Startup class
public class Startup
{
    public void Configuration(IAppBuilder app)
    {
      var options = new StuntmanOptions()
          .AddUser(new StuntmanUser("user-1", "User 1")
              .AddClaim("given_name", "John")
              .AddClaim("family_name", "Doe"));

        // Optionally assign a user an access token.
        options
          .AddUser(new StuntmanUser("user-2", "User 2")
              .SetAccessToken("123")
              .AddClaim("given_name", "Mary")
              .AddClaim("family_name", "Smith"));

        app.UseStuntman(options);
    }
}
```

### View

The below example shows how to use Stuntman in a **Razor** view to get the user picker during development.

```
@{
    var stuntmanOptions = new StuntmanOptions()
        .AddUser(new StuntmanUser("user-1", "User 1"));
}

@Html.Raw(new UserPicker(stuntmanOptions).GetHtml(User, Request.RawUrl));
```

### Bearer-token

Stuntman supports bearer-tokens based on a user's access-token (`StuntmanUser.SetAccessToken`). There is nothing special about the value and no additional encoding/decoding is necessary. Upon successful authentication, the value is added as a claim. Leveraging the previous `Startup` code, you could construct an HTTP-request to utilize User 2's access-token:

```shell
> curl -i -H "Authorization: Bearer 123" http://localhost:54917/secure
HTTP/1.1 200 OK
```

Basic format-checking is done on the value:

```shell
> curl -i -H "Authorization: Bearer not-real" http://localhost:54917/secure
HTTP/1.1 403 options provided does not include the requested 'not-real' user.
```

```shell
> curl -i -H "Authorization: Bearer abc 123" http://localhost:54917/secure
HTTP/1.1 400 Authorization header is not in correct format.
```

## Contributing

### Prerequisites

After cloning down the repository:

```
> npm install -g grunt-cli
> cd the_repository_folder\src\Core
> npm install
```

## License

MIT License
