using System.Collections.Generic;

namespace RimDev.Stuntman.Core
{
    public class StuntmanServerResponse
    {
        public IEnumerable<StuntmanUser> Users { get; set; } = new List<StuntmanUser>();
    }
}
