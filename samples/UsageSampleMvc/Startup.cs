using Owin;
using RimDev.Stuntman.Core;

namespace RimDev.Stuntman.UsageSampleMvc
{
    public class Startup
    {
        public static readonly StuntmanOptions StuntmanOptions = new StuntmanOptions();

        public void Configuration(IAppBuilder app)
        {
            StuntmanOptions
                .AddUser(new StuntmanUser("user-1", "User 1")
                    .AddClaim("given_name", "John")
                    .AddClaim("family_name", "Doe"));

            if (System.Web.HttpContext.Current.IsDebuggingEnabled)
            {
                app.UseStuntman(StuntmanOptions);
            }
        }
    }
}
