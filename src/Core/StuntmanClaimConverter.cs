using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Claims;

namespace RimDev.Stuntman.Core
{
    public class StuntmanClaimConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Claim);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            var emptyClaim = new Claim("", "");

            var claimType = (string)jObject[nameof(emptyClaim.Type)];
            var claimValue = (string)jObject[nameof(emptyClaim.Value)];

            return new Claim(claimType, claimValue);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
