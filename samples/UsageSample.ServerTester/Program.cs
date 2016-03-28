using RimDev.Stuntman.Core;
using System;
using System.Configuration;
using System.Linq;

namespace UsageSample.ServerTester
{
    internal class Program
    {
        private static StuntmanOptions StuntmanOptions = new StuntmanOptions();

        private static void Main(string[] args)
        {
            var baseUrl = GetAppSetting("UsageSample:BaseUrl");

            StuntmanOptions
                .AddConfigurationFromServer(baseUrl);

            Console.WriteLine("Users:");
            Console.WriteLine(string.Join(
                Environment.NewLine,
                StuntmanOptions.Users.Select(x => $"- {x.Id}: {x.Name}")));

            Console.ReadLine();
        }

        private static string GetAppSetting(string key)
        {
            var appSetting = ConfigurationManager.AppSettings[key];
            if (appSetting == null)
                throw new ApplicationException($"appSetting {key} must be set.");

            return appSetting;
        }
    }
}
