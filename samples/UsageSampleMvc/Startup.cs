using Owin;
using RimDev.Stuntman.Core;
using System.Security.Claims;

namespace RimDev.Stuntman.UsageSampleMvc
{
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
                        Id = "user-1",
                        Name = "User 1",
                        Claims = new[]
                        {
                            new Claim("given_name", "John"),
                            new Claim("family_name", "Doe")
                        }
                    },
                }
            };

            app.UseStuntman(options);
        }
    }
}
