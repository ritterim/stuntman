using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace UsageSample.BearerTokenTester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // http://stackoverflow.com/a/17630538
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Request without bearer token set");
            Console.WriteLine("--------------------------------");
            Console.WriteLine();
            await PerformRequest(shouldBeSuccessful: false);

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Request with valid bearer token set");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();
            await PerformRequest(shouldBeSuccessful: true);

            Console.ReadLine();
        }

        private static async Task PerformRequest(bool shouldBeSuccessful)
        {
            var baseUrl = GetAppSetting("UsageSample:BaseUrl");
            var testPath = GetAppSetting("UsageSample:TestPath");
            var testUserToken = GetAppSetting("UsageSample:TestUserToken");

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(baseUrl);

                if (shouldBeSuccessful)
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", testUserToken);
                }

                var response = await httpClient.GetAsync(testPath);

                Console.WriteLine($"Response Status Code: {(int)response.StatusCode}");
                Console.WriteLine($"Response Content-Type: {response.Content.Headers.ContentType}");
                Console.WriteLine();
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
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
