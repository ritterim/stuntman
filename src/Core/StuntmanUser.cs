using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;

namespace RimDev.Stuntman.Core
{
    public class StuntmanUser
    {
        public const string DefaultNameClaimType = "name";
        public const string DefaultRoleClaimType = "role";

        [JsonConstructor]
        public StuntmanUser(
            string id,
            string name,
            string nameClaimType,
            string roleClaimType)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (nameClaimType == null) throw new ArgumentNullException(nameof(nameClaimType));
            if (roleClaimType == null) throw new ArgumentNullException(nameof(roleClaimType));

            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException($"{nameof(id)} must not be empty or whitespace.");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException($"{nameof(name)} must not be empty or whitespace.");
            if (string.IsNullOrWhiteSpace(nameClaimType)) throw new ArgumentException($"{nameof(nameClaimType)} must not be empty or whitespace.");
            if (string.IsNullOrWhiteSpace(roleClaimType)) throw new ArgumentException($"{nameof(roleClaimType)} must not be empty or whitespace.");

            Id = id;
            Name = name;
            NameClaimType = nameClaimType;
            RoleClaimType = roleClaimType;
        }

        public StuntmanUser(string id, string name)
            : this(
                id: id,
                name: name,
                nameClaimType: DefaultNameClaimType,
                roleClaimType: DefaultRoleClaimType)
        { }

        /// <summary>
        /// Creates a new user with an auto-generated Id.
        /// </summary>
        public StuntmanUser(string name)
            : this(
                id: Guid.NewGuid().ToString("D"),
                name: name,
                nameClaimType: DefaultNameClaimType,
                roleClaimType: DefaultRoleClaimType)
        { }

        public string AccessToken { get; private set; }

        public string Id { get; private set; }

        public string Name { get; private set; }

        [DefaultValue(DefaultNameClaimType)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string NameClaimType { get; private set; }

        public ICollection<Claim> Claims { get; private set; } = new List<Claim>();

        public string Description { get; private set; }

        [DefaultValue(DefaultRoleClaimType)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string RoleClaimType { get; private set; }

        public string Source { get; private set; }

        public StuntmanUser AddClaim(string type, string value)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("type must not be empty or whitespace.");
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("value must not be empty or whitespace.");

            Claims.Add(new Claim(type, value));
            return this;
        }

        public StuntmanUser AddName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException($"{nameof(name)} must not be empty or whitespace.");

            AddClaim(NameClaimType, name);

            return this;
        }

        public StuntmanUser AddRole(string role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentException($"{nameof(role)} must not be empty or whitespace.");

            AddClaim(RoleClaimType, role);

            return this;
        }

        public StuntmanUser SetAccessToken(string accessToken)
        {
            if (accessToken == null) throw new ArgumentNullException(nameof(accessToken));

            if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentException("accessToken must not be empty or whitespace.");

            AccessToken = accessToken;

            return this;
        }

        public StuntmanUser SetDescription(string description)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));

            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("description must not be empty or whitespace.");

            Description = description;

            return this;
        }

        public StuntmanUser SetSource(string source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("source must not be empty or whitespace.");

            Source = source;

            return this;
        }
    }
}
