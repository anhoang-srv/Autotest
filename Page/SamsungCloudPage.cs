using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using SamsungCloudTest.Helper;
using System;
using OpenQA.Selenium.Interactions;

namespace SamsungCloudTest.Page
{
    /// <summary>
    /// Page Object cho màn hình Samsung Cloud
    /// </summary>
    public class SamsungCloudPage : General
    {
        // --- Element Locators ---
        private const string CancelButtonAccessibilityId = "OneUIPrimaryActionButton";
        private const string InstallButtonAccessibilityId = "OneUISecondaryActionButton";

        // Constructor nhận driver từ Sessions
        public SamsungCloudPage(WindowsDriver<WindowsElement> driver) : base(driver)
        {
        }

        #region Element Accessors

        /// <summary>
        /// Lấy Cancel button element
        /// </summary>
        /// 
        private By CancelButton => By.Name("Cancel");

        /// <summary>
        /// Lấy Install button element
        /// </summary>
        private By InstallButton => By.Name("Install");

        #endregion

        #region Actions

        /// <summary>
        /// Focus vào Cancel button bằng cách tab đến nó
        /// </summary>
        public void FocusCancelButton()
        {
            try
            {
                // Gửi phím Tab để navigate đến Cancel button
                // Giả sử Cancel là button đầu tiên trong tab order
                var actions = new Actions(_driver);
                actions.SendKeys(Keys.Tab).Perform();
                
                // Đợi Narrator đọc xong element trước khi tiếp tục
                System.Threading.Thread.Sleep(2500); // 2.5 giây để Narrator đọc hết
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể focus vào Cancel button. Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Focus vào Install button
        /// </summary>
        public void FocusInstallButton()
        {
            try
            {
                // Gửi phím Tab để navigate đến Install button
                // Nếu Install nằm sau Cancel thì cần Tab thêm 1 lần nữa
                var actions = new Actions(_driver);
                actions.SendKeys(Keys.Tab).Perform();
                
                // Đợi Narrator đọc xong element trước khi tiếp tục
                System.Threading.Thread.Sleep(2500); // 2.5 giây để Narrator đọc hết
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể focus vào Install button. Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra app đã được launch chưa
        /// </summary>
        public bool IsSamsungCloudAppLaunched()
        {
            try
            {
                // Kiểm tra title window hoặc một element đặc trưng
                return IsAppLaunched() && _driver.Title.Contains("Samsung Cloud");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Phóng to cửa sổ app để tập trung focus
        /// </summary>
        public void MaximizeWindow()
        {
            try
            {
                _driver.Manage().Window.Maximize();
                System.Threading.Thread.Sleep(500); // Đợi animation phóng to xong
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể phóng to cửa sổ: {ex.Message}");
            }
        }

        #endregion
    }
}
