![Stuntman logo](https://raw.githubusercontent.com/ritterim/stuntman/gh-pages/images/stuntman-logo.png)

> "Sometimes you need a Stuntman before you send in real, unsuspecting users!"

![NuGet Version](https://img.shields.io/nuget/v/RimDev.Stuntman.svg)
![NuGet Download Count](https://img.shields.io/nuget/dt/RimDev.Stuntman.svg)

**Stuntman** is a library for impersonating users during development leveraging .NET Claims Identity. Used primarily in web environments like ASP.NET MVC, ASP.NET Web Forms, and OWIN applications that serve HTML. This allows you to test different user scenarios that exist in your application with minimal friction. It also allows you to share those scenarios with other team members via source control.

![Stuntman demo](https://cloud.githubusercontent.com/assets/1012917/10737939/5154bbdc-7beb-11e5-87dd-979c4e8cb3c0.gif)

## Installation

Install the [RimDev.Stuntman](https://www.nuget.org/packages/RimDev.Stuntman/) NuGet package.

```
PM> Install-Package RimDev.Stuntman
```

## Usage

### Startup / Middleware registration

Stuntman uses OWIN and is registered as middleware, and allows for programmatically preset user scenarios, in the form of claims identities. These presets can be utilized by you or other team members working on the same code base.

```csharp
// OWIN Startup class
public class Startup
{
    public readonly StuntmanOptions StuntmanOptions = new StuntmanOptions();

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

## Contributing

Have an idea? Let's talk about it in an issue!

Find a bug? Open an issue or submit a pull request!

## License

MIT License
