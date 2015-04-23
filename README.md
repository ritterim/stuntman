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
        var options = new StuntmanOptions
        {
            Users = new[]
            {
                new StuntmanUser
                {
                    Id = "test-user-1",
                    Name = "Test User 1",
                    Claims = new[]
                    {
                        new Claim("given_name", "John"),
                        new Claim("family_name", "Doe")
                    }
                },
                // Add more users here,
                // if you'd like.
            }
        };

        // Wire up Stuntman authentication.
        app.UseStuntman(options);
    }
}
```

### View

The below example shows how to use Stuntman in a **Razor** view to get the user picker during development.

```
@Html.Raw(Stuntman.Picker.GetHtml(Request.RawUrl));
```

## License

MIT License
