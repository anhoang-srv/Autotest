using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using SamsungCloudTest.Configuration;
using System;

namespace SamsungCloudTest.Helper 
{
    public class Sessions
    {

        public WindowsDriver<WindowsElement>? driver;
        private AppiumSettings? _config;

        public void InitializeSession()
        {
            _config = ConfigReader.GetAppiumSettings();
            if (driver == null)
            {
            
                var options = new AppiumOptions();
                options.AddAdditionalCapability("app", _config.AppId);
                options.AddAdditionalCapability("deviceName", _config.DeviceName);
                options.AddAdditionalCapability("platformName", "Windows");

                driver = new WindowsDriver<WindowsElement>(new Uri(_config.DriverUrl), options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(_config.ImplicitWaitSeconds);
            }
        }

        public void StopSession()
        {
            if (driver != null)
            {
                driver.Quit();
                driver = null;
            }
        }
    }
}