# Stuntman

> "Sometimes, you need to be a stand-in."

A library for impersonating users during development leveraging ASP.NET Identity.

## Installation

Install the [*NOT_YET_AVAILABLE RimDev.Stuntman*](https://www.nuget.org/packages/RimDev.Stuntman/) NuGet package.

```
NOT_YET_AVAILABLE Install-Package RimDev.Stuntman
```

## Usage

Stuntman uses OWIN. If you're using ASP.NET MVC, you can add OWIN and find a way to render the HTML as necessary for the user picker.

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

        // The return uri to redirect to after a Stuntman login.
        // You probably want this to be the current page.
        var loginReturnUri = "https://return-uri";

        // Inject this HTML into your page
        // to have a method to pick users.
        var html = new UserPicker(options).GetHtml(loginReturnUri);
    }
}
```

## License

MIT License
