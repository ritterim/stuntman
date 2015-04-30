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

            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id must not be empty or whitespace.");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name must not be empty or whitespace.");

            Id = id;
            Name = name;
            Claims = new List<Claim>();
        }

        /// <summary>
        /// Creates a new user with an auto-generated Id.
        /// </summary>
        public StuntmanUser(string name)
            :this(
            id: Guid.NewGuid().ToString("D"),
            name: name)
        {
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public ICollection<Claim> Claims { get; private set; }

        public StuntmanUser AddClaim(string type, string value)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (value == null) throw new ArgumentNullException("value");

            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("type must not be empty or whitespace.");
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("value must not be empty or whitespace.");

            Claims.Add(new Claim(type, value));
            return this;
        }
    }
}
