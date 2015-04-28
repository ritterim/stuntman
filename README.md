# Stuntman

> "Sometimes, you need a stand-in."

**Stuntman** is a library for impersonating users during development leveraging .NET Claims Identity. Used primarily in web environments like ASP.NET MVC, ASP.NET Web Forms, and OWIN applications that serve HTML. This allows you to test different user scenarios that exist in your application with minimal friction. It also allows you to share those scenarios with other team members via source control.

## Installation

Install the [*NOT_YET_AVAILABLE RimDev.Stuntman*](https://www.nuget.org/packages/RimDev.Stuntman/) NuGet package.

```
NOT_YET_AVAILABLE Install-Package RimDev.Stuntman
```

## Usage

### Startup / Middleware registration

Stuntman uses OWIN and is registered as middleware, and allows for programmatictally preset user scenarios, in the form of claims identities. These presets can be utilized by you or other team members working on the same code base.

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
