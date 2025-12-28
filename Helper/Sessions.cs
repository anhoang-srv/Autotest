using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using SamsungCloudTest.Configuration;
using System;

namespace SamsungCloudTest.Helper 
{
    public class Sessions
    {

        public WindowsDriver driver;
        private AppiumSettings _config;

        public void InitializeSession()
        {
            _config = ConfigReader.GetAppiumSettings();
            if (driver == null)
            {

                var options = new AppiumOptions();
                options.AddAdditionalAppiumOption("app", _config.AppId);
                options.AddAdditionalAppiumOption("deviceName", _config.DeviceName);

                driver = new WindowsDriver(new Uri(_config.DriverUrl), options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
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