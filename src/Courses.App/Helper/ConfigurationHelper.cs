using System;
using Microsoft.Extensions.Configuration;

namespace Courses.App.Helper
{
    public class ConfigurationHelper
    {
        public static IConfiguration Build()
        {
            string? env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}