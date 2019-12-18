using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace PostgresExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
                .Build()
                .Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args)
                .UseDefaultServiceProvider(configure => configure.ValidateScopes = false)
                .UseStartup<Startup>();

            return builder;
        }
    }
}