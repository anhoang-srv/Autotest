using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SamsungCloudTest.Configuration
{
    public static class ConfigReader
    {
        public static AppiumSettings GetAppiumSettings()
        {
            var builder = new ConfigurationBuilder()
    
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var settings = new AppiumSettings();
            configuration.GetSection("AppiumSettings").Bind(settings);

            return settings;
        }
    }
}