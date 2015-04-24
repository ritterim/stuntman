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

            if (id == string.Empty) throw new ArgumentException("id must not be empty.");
            if (name == string.Empty) throw new ArgumentException("name must not be empty.");

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

            if (type == string.Empty) throw new ArgumentException("type must not be empty.");
            if (value == string.Empty) throw new ArgumentException("value must not be empty.");

            Claims.Add(new Claim(type, value));
            return this;
        }
    }
}
