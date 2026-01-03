using NUnit.Framework;
using SamsungCloudTest.Helper;
using SamsungCloudTest.Page;
using TechTalk.SpecFlow;
using System;

namespace SamsungCloudTest.Steps
{
    [Binding]
    public class NarratorSteps
    {
        #region Fields & Constructor

        private readonly Sessions _sessions;
        private SamsungCloudPage? _samsungCloudPage;

        // SpecFlow DI tự động inject Sessions
        public NarratorSteps(Sessions sessions)
        {
            _sessions = sessions;
        }

        #endregion

        #region Given Steps

        [Given(@"""(.*)"" app is launched")]
        public void GivenAppIsLaunched(string appName)
        {
            try
            {
                // Khởi tạo Page Object với driver từ Sessions
                _samsungCloudPage = new SamsungCloudPage(_sessions.driver);

                // Verify app đã được launch
                bool isLaunched = _samsungCloudPage.IsSamsungCloudAppLaunched();
                Assert.That(isLaunched, Is.True, $"App '{appName}' chưa được launch thành công.");
                Console.WriteLine($"App '{appName}' đã được launch thành công.");

                // Bước 1: Phóng to cửa sổ app
                _samsungCloudPage.MaximizeWindow();
                Console.WriteLine("Đã phóng to cửa sổ app.");

                // Bước 2: Bật Narrator nếu chưa chạy
                if (!AxeHelper.IsNarratorRunning())
                {
                    StartNarrator();
                    System.Threading.Thread.Sleep(3000); // Đợi Narrator khởi động hoàn tất
                    Console.WriteLine("Đã bật Narrator.");
                }
                else
                {
                    Console.WriteLine("Narrator đã chạy sẵn.");
                }

                // Bước 3: Reset Narrator Event Logs để tránh bắt nhầm
                // (sẽ thực hiện trong Then step khi khởi tạo verifier)
            }
            catch (Exception ex)
            {
                Assert.Fail($"Lỗi khi launch app '{appName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Bật Narrator bằng tổ hợp phím Ctrl+Win+Enter
        /// </summary>
        private void StartNarrator()
        {
            try
            {
                // Gửi phím Escape để thoát các dialog nếu có
                System.Windows.Forms.SendKeys.SendWait("{ESC}"); // Thoát các dialog nếu có
                System.Threading.Thread.Sleep(200);
                
                // Sử dụng Process để start Narrator an toàn hơn
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "narrator.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể bật Narrator tự động: {ex.Message}. Vui lòng bật thủ công bằng Win+Ctrl+Enter.");
            }
        }

        #endregion

        #region When Steps

        [When(@"the ""(.*)"" button is focused")]
        public void WhenTheButtonIsFocused(string buttonName)
        {
            try
            {
                if (_samsungCloudPage == null)
                {
                    Assert.Fail("Samsung Cloud Page chưa được khởi tạo. Đảm bảo step 'Given' đã chạy trước.");
                    return;
                }

                if (buttonName.Equals("Cancel", StringComparison.OrdinalIgnoreCase))
                {
                    _samsungCloudPage.FocusCancelButton();
                    Console.WriteLine($"Đã focus vào button '{buttonName}'.");
                }
                else if (buttonName.Equals("Install", StringComparison.OrdinalIgnoreCase))
                {
                    _samsungCloudPage.FocusInstallButton();
                    Console.WriteLine($"Đã focus vào button '{buttonName}'.");
                }
                else
                {
                    Assert.Fail($"Button '{buttonName}' không được hỗ trợ trong test case này.");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Lỗi khi focus vào button '{buttonName}': {ex.Message}");
            }
        }

        #endregion

        #region Then Steps

        [Then(@"the Narrator must be reading ""(.*)""")]
        public void ThenTheNarratorMustBeReading(string expectedNarratorText)
        {
            try
            {
                // Sử dụng NarratorAccessibilityVerifier để kiểm tra
                using (var verifier = new NarratorAccessibilityVerifier())
                {                    // QUAN TRỌNG: Clear event logs trước để tránh bắt nhầm events cũ
                    System.Threading.Thread.Sleep(500); // Đợi các event cũ settle
                    verifier.ClearEvents(); // Clear events đã capture khi khởi tạo
                    Console.WriteLine("Đã reset Narrator event logs.");
                    // Verify Narrator đọc đúng text và không có lỗi accessibility
                    // Sử dụng Tab để focus và verify
                    var (success, message) = verifier.VerifyNarratorWithAccessibility(
                        expectedNarratorText, 
                        waitTimeMs: 2000
                    );

                    // In log để debug nếu cần
                    if (!success)
                    {
                        Console.WriteLine("=== Narrator Event Logs ===");
                        verifier.PrintEventLogs();
                        System.Threading.Thread.Sleep(1000); // Đợi log in xong
                    }

                    // Assert kết quả
                    Assert.That(success, Is.True, message);
                    Console.WriteLine($"Narrator đọc đúng: '{expectedNarratorText}'");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Lỗi khi verify Narrator đọc '{expectedNarratorText}': {ex.Message}");
            }
        }

        #endregion
    }
}
