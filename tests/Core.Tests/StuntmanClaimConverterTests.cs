using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Claims;
using Xunit;

namespace RimDev.Stuntman.Core.Tests
{
    public class StuntmanClaimConverterTests
    {
        private readonly StuntmanClaimConverter _sut;

        public StuntmanClaimConverterTests()
        {
            _sut = new StuntmanClaimConverter();
        }

        public class CanConvertMethod : StuntmanClaimConverterTests
        {
            [Fact]
            public void CanConvertClaimType()
            {
                Assert.True(_sut.CanConvert(typeof(Claim)));
            }

            [Fact]
            public void CannotConvertNonClaimType()
            {
                Assert.False(_sut.CanConvert(typeof(object)));
            }
        }

        public class ReadJsonMethod : StuntmanClaimConverterTests
        {
            [Fact]
            public void PopulatesValidJsonClaim()
            {
                const string ClaimType = "TestClaimType";
                const string ClaimValue = "TestClaimValue";

                var json = JsonConvert.SerializeObject(new Claim(ClaimType, ClaimValue));

                using (var reader = new JsonTextReader(new StringReader(json)))
                {
                    var result = _sut.ReadJson(reader, null, null, null);

                    var claim = Assert.IsType<Claim>(result);

                    Assert.Equal(ClaimType, claim.Type);
                    Assert.Equal(ClaimValue, claim.Value);
                }
            }

            [Fact]
            public void ThrowsArgumentNullExceptionForNullJson()
            {
                Assert.Throws<ArgumentNullException>(
                    "s",
                    () =>
                    {
                        using (var reader = new JsonTextReader(new StringReader(null)))
                        {
                            _sut.ReadJson(reader, null, null, null);
                        }
                    });
            }

            [Fact]
            public void ThrowsArgumentNullExceptionForEmptyObject()
            {
                Assert.Throws<ArgumentNullException>(
                    "type",
                    () =>
                    {
                        using (var reader = new JsonTextReader(new StringReader("{}")))
                        {
                            _sut.ReadJson(reader, null, null, null);
                        }
                    });
            }

            [Theory,
                InlineData(""),
                InlineData(" "),
            ]
            public void ThrowsForInvalidJson(string json)
            {
                Assert.Throws<JsonReaderException>(() =>
                {
                    using (var reader = new JsonTextReader(new StringReader(json)))
                    {
                        _sut.ReadJson(reader, null, null, null);
                    }
                });
            }
        }

        public class WriteJsonMethod : StuntmanClaimConverterTests
        {
            [Fact]
            public void ThrowsNotImplementedException()
            {
                Assert.Throws<NotImplementedException>(
                    () => _sut.WriteJson(null, null, null));
            }
        }
    }
}
