using Microsoft.Owin.Hosting;
using System;

namespace UsageSample
{
    public class Program
    {
        private static void Main()
        {
            var baseAddress = "https://localhost:44399/";

            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Console.WriteLine($"Running at {baseAddress}");
                Console.ReadLine();
            }
        }
    }
}
