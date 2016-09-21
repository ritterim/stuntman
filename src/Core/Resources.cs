using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace RimDev.Stuntman.Core
{
    public class Resources
    {
        private const string StuntmanEmbeddedAssetsPrefix = "Core.assets.";

        public static string GetCss()
        {
            var css = GetStuntmanResource("stuntman.css");

            return css;
        }

        public static string GetLogoForInlining()
        {
            var logoBytes = GetStuntmanResourceBytes("stuntman-logo.png");

            return "data:image/png;base64," + Convert.ToBase64String(logoBytes);
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

        public static byte[] GetStuntmanResourceBytes(string resourceName)
        {
            using (var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream(StuntmanEmbeddedAssetsPrefix + resourceName))
            {
                // http://stackoverflow.com/a/7073124 via http://stackoverflow.com/questions/1080442/how-to-convert-an-stream-into-a-byte-in-c
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
