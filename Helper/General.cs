using Axe.Windows.Automation;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Support.UI;
using System;

namespace SamsungCloudTest.Helper 
{
    public class General
    {
        protected WindowsDriver<WindowsElement> _driver;
        protected WebDriverWait _wait;

        // Constructor
        public General(WindowsDriver<WindowsElement> driver)
        {

            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        // --- Các phương thức bổ trợ (Helper Methods) ---

        public void ClickElement(By locator)
        {
            try
            {
                var element = _wait.Until(d => d.FindElement(locator));
                element.Click();
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể click vào element: {locator}. Lỗi: {ex.Message}");
            }
        }

        public void SetFocus(By locator)
        {
            var element = _wait.Until(d => d.FindElement(locator));
            element.SendKeys(Keys.Tab);
        }

        public string GetFocusedElementAccessibilityName()
        {
            try
            {
                var focusedElement = _driver.SwitchTo().ActiveElement();
                return focusedElement.GetAttribute("Name");
            }
            catch
            {
                return string.Empty;
            }
        }

        public bool IsAppLaunched()
        {
            return _driver != null && _driver.SessionId != null;
        }

    }
}