using SamsungCloudTest.Configuration;
using System;
using DotNetEnv; 

namespace SamsungCloudTest.Configuration
{
    public static class ConfigReader
    {
        public static AppiumSettings GetAppiumSettings()
        {
            
            Env.Load();

            return new AppiumSettings
            {
                DriverUrl = Environment.GetEnvironmentVariable("DRIVER_URL") ?? "http://127.0.0.1:4723",
                AppId = Environment.GetEnvironmentVariable("APP_ID"),
                DeviceName = Environment.GetEnvironmentVariable("DEVICE_NAME") ?? "WindowsPC",

                // Parse chuỗi sang int
                ImplicitWaitSeconds = int.Parse(Environment.GetEnvironmentVariable("IMPLICIT_WAIT_SECONDS") ?? "2")
            };
        }
    }
}