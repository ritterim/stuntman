
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

Here's how to use Stuntman in a Razor view to show the user picker (assuming the application Startup class has StuntmanOptions that can be used).

```csharp
@* Only show when debug is true in Web.config. *@
@if (System.Web.HttpContext.Current.IsDebuggingEnabled)
{
    @Html.Raw(YourApplicationNamespace.Startup.StuntmanOptions.UserPicker(User));
}
```

### Bearer-token

Stuntman supports bearer-tokens based on a user's access-token (`StuntmanUser.SetAccessToken`). There is nothing special about the value and no additional encoding/decoding is necessary. Upon successful authentication, the value is added as a claim. Leveraging the previous Startup code, you could construct an HTTP-request to utilize User 2's access-token:

```
> curl -i -H "Authorization: Bearer 123" http://localhost:54917/secure
HTTP/1.1 200 OK
```

Basic format-checking is done on the value:

```
> curl -i -H "Authorization: Bearer not-real" http://localhost:54917/secure
 HTTP/1.1 403 options provided does not include the requested 'not-real' user.
```

```
> curl -i -H "Authorization: Bearer abc 123" http://localhost:54917/secure
HTTP/1.1 400 Authorization header is not in correct format.
```

### Remote users

Users can be populated from remote locations using one or more of the following:

From the file system
```csharp
StuntmanOptions.AddUsersFromJson("C:\\path\\to\\users.json");
```

From a web url to a JSON file
```csharp
  StuntmanOptions.AddUsersFromJson("https://example.com/users.json");
```

From a web url to a Stuntman instance with a running server

```csharp
//
// On the server
//
StuntmanOptions.EnableServer();

//
// On the client
//
StuntmanOptions.AddConfigurationFromServer(
    "https://some-stuntman-enabled-app.example.com/");
// or, if you prefer to not throw an exception
// and have the users silently not added
// if the server is unavailable:
StuntmanOptions.TryAddConfigurationFromServer(
    "https://some-stuntman-enabled-app.example.com/");
```

### Example users JSON

Here's an example users JSON that can be consumed by `StuntmanOptions.AddUsersFromJson(string pathOrUrl)`:

```json
[
  { "Id": "user-1", "Name": "User 1" },
  { "Id": "user-2", "Name": "User 2" }
]
```

### Support OSS &mdash; buy an official sticker

You can help this project by buying an official Stuntman sticker at the [StickerMule marketplace](https://www.stickermule.com/marketplace/9330-stuntman){: target="_blank"}, and please send us a picture by tweeting at the contributors or by using the **#stuntman** hashtag.

{: .ui.center.aligned.padded.segment }
![Buy a stuntman sticker](https://www.stickermule.com/marketplace/embed_img/9330)

{: .ui.center.aligned.padded.grid }
[Buy this sticker](https://www.stickermule.com/marketplace/9330-stuntman){: .ui.huge.red.button }
