using System.IO;
using System.Reflection;

namespace RimDev.Stuntman.Core
{
    public class Resources
    {
        private const string StuntmanResourcesPrefix = "RimDev.Stuntman.Core.resources.";

        public static string GetCss()
        {
            var css = GetStuntmanResource("stuntman.pure.css") +
                GetStuntmanResource("stuntman.css");

            return css;
        }

        private static string GetStuntmanResource(string resourceName)
        {
            var resource = string.Empty;

            using (var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream(StuntmanResourcesPrefix + resourceName))
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
