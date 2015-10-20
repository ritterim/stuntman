using System.IO;
using System.Reflection;

namespace RimDev.Stuntman.Core
{
    public class Resources
    {
        private const string StuntmanEmbeddedAssetsPrefix = "RimDev.Stuntman.Core.assets.";

        public static string GetCss()
        {
            var css = GetStuntmanResource("stuntman.css");

            return css;
        }

        private static string GetStuntmanResource(string resourceName)
        {
            var resource = string.Empty;

            using (var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream(StuntmanEmbeddedAssetsPrefix + resourceName))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    resource = streamReader.ReadToEnd();
                }
            }

            return resource;
        }
    }
}
