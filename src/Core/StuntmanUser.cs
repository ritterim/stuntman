using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace RimDev.Stuntman.Core
{
    public class StuntmanUser
    {
        public StuntmanUser(string id, string name)
        {
            if (id == null) throw new ArgumentNullException("id");
            if (name == null) throw new ArgumentNullException("name");

            Id = id;
            Name = name;
            Claims = new List<Claim>();
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public ICollection<Claim> Claims { get; private set; }

        public StuntmanUser AddClaim(string type, string value)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (value == null) throw new ArgumentNullException("value");

            Claims.Add(new Claim(type, value));
            return this;
        }
    }
}
