using System.Collections.Generic;
using System.Security.Claims;

namespace RimDev.Stuntman.Core
{
    public class StuntmanUser
    {
        public StuntmanUser()
        {
            Claims = new List<Claim>();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Claim> Claims { get; set; }
    }
}
