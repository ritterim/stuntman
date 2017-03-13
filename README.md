![Stuntman logo](https://raw.githubusercontent.com/ritterim/stuntman/gh-pages/images/stuntman-logo.png)

> "Sometimes you need a Stuntman before you send in real, unsuspecting users!"

| Package                    | Version |
| -------------------------- | ------- |
| RimDev.Stuntman            | ![RimDev.Stuntman NuGet Version](https://img.shields.io/nuget/v/RimDev.Stuntman.svg) |
| RimDev.Stuntman.AspNetCore | ![RimDev.Stuntman.AspNetCore NuGet Version](https://img.shields.io/nuget/v/RimDev.Stuntman.AspNetCore.svg) |

**Stuntman** is a library for impersonating users during development leveraging .NET Claims Identity. Used primarily in web environments like ASP.NET MVC, ASP.NET Web Forms, and OWIN applications that serve HTML. This allows you to test different user scenarios that exist in your application with minimal friction. It also allows you to share those scenarios with other team members via source control.

![Stuntman demo](https://cloud.githubusercontent.com/assets/1012917/10737939/5154bbdc-7beb-11e5-87dd-979c4e8cb3c0.gif)

## Installation

Install either the [RimDev.Stuntman](https://www.nuget.org/packages/RimDev.Stuntman/) or [RimDev.Stuntman.AspNetCore](https://www.nuget.org/packages/RimDev.Stuntman.AspNetCore/) NuGet package, depending on the environment.

```
PM> Install-Package RimDev.Stuntman

or

PM> Install-Package RimDev.Stuntman.AspNetCore
```

## Usage

### Startup / Middleware registration

Stuntman uses OWIN and is registered as middleware, and allows for programmatically preset user scenarios, in the form of claims identities. These presets can be utilized by you or other team members working on the same code base.

```csharp
// OWIN Startup class
public class Startup
{
    public static readonly StuntmanOptions StuntmanOptions = new StuntmanOptions();

    public void Configuration(IAppBuilder app)
    {
        StuntmanOptions
            .AddUser(new StuntmanUser("user-1", "User 1")
                .AddClaim("given_name", "John")
                .AddClaim("family_name", "Doe"));

        // Optionally assign a user an access token.
        StuntmanOptions
            .AddUser(new StuntmanUser("user-2", "User 2")
                .SetAccessToken("123")
                .AddClaim("given_name", "Mary")
                .AddClaim("family_name", "Smith"));

        // You can also add users using HTTP/HTTPS or the file system!
        StuntmanOptions
            .AddUsersFromJson("https://example.com/web-test-users.json")
            .AddUsersFromJson(@"C:\local-test-users.json");

        // Optional alignment of user picker
        // Supported options are:
        // - StuntmanAlignment.Left (default)
        // - StuntmanAlignment.Center
        // - StuntmanAlignment.Right
        StuntmanOptions.SetUserPickerAlignment(StuntmanAlignment.Right);

        // Only show when debug is true in Web.config.
        if (System.Web.HttpContext.Current.IsDebuggingEnabled)
        {
            app.UseStuntman(StuntmanOptions);
        }
    }
}
```

### View

Here's how to use Stuntman in a **Razor** view to show the user picker *(assuming the application `Startup` class has `StuntmanOptions` that can be used)*.

```
@* Only show when debug is true in Web.config. *@
@if (System.Web.HttpContext.Current.IsDebuggingEnabled)
{
    @Html.Raw(YourApplicationNamespace.Startup.StuntmanOptions.UserPicker(User));
}
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

### Remote users

Users can be populated from remote locations using one or more of the following:

- From the file system

```csharp
StuntmanOptions.AddUsersFromJson("C:\\path\\to\\users.json");
```
- From a web url to a JSON file

```csharp
StuntmanOptions.AddUsersFromJson("https://example.com/users.json");
```
- From a web url to a Stuntman instance with a running server

```csharp
//
// On the server
//
StuntmanOptions.EnableServer();

//
// On the client
//
StuntmanOptions.AddConfigurationFromServer("https://some-stuntman-enabled-app.example.com/");
// or, if you prefer to not throw an exception
// and have the users silently not added
// if the server is unavailable:
StuntmanOptions.TryAddConfigurationFromServer("https://some-stuntman-enabled-app.example.com/");
```

### Example users JSON

Here's an example users JSON that can be consumed by `StuntmanOptions.AddUsersFromJson(string pathOrUrl)`:

```json
[
  { "Id": "user-1", "Name": "User 1" },
  { "Id": "user-2", "Name": "User 2" }
]
```

## Contributing

Have an idea? Let's talk about it in an issue!

Find a bug? Open an issue or submit a pull request!

## License

MIT License

## Stickers

You can buy an official sticker at the [StickerMule Marketplace](https://www.stickermule.com/marketplace/9330-stuntman).
