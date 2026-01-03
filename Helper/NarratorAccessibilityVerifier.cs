using GalaxyCloud.Helpers;
using System;
using System.Collections.Generic;

namespace SamsungCloudTest.Helper
{
    /// <summary>
    /// Tích hợp NarratorVerifier và AxeHelper để kiểm tra Narrator đọc đúng và bảo đảm accessibility.
    /// </summary>
    public class NarratorAccessibilityVerifier : IDisposable
    {
        private readonly NarratorVerifier _narratorVerifier;
        private readonly AxeHelper _axeHelper;
        private bool _disposed;

        public NarratorAccessibilityVerifier()
        {
            _narratorVerifier = new NarratorVerifier();
            _axeHelper = new AxeHelper();
        }

        /// <summary>
        /// Kiểm tra Narrator đọc đúng nội dung và không có lỗi accessibility (sử dụng Tab).
        /// <param name="expectedText">Text mong đợi Narrator đọc.</param>
        /// <param name="waitTimeMs">Thời gian chờ Narrator đọc (ms). Mặc định 2000ms.</param>
        /// </summary>
        public (bool Success, string Message) VerifyNarratorWithAccessibility(string expectedText, int waitTimeMs = 2000)
        {
            if (string.IsNullOrWhiteSpace(expectedText))
            {
                return (false, "expectedText không được rỗng");
            }

            if (waitTimeMs <= 0)
            {
                return (false, "waitTimeMs phải lớn hơn 0");
            }

            if (!AxeHelper.IsNarratorRunning())
            {
                return (false, "Narrator không chạy. Vui lòng bật Narrator (Win + Ctrl + Enter).");
            }

            try
            {
                _narratorVerifier.TabAndListen(waitTimeMs);

                string actualText = _narratorVerifier.GetLastName();

                if (string.IsNullOrEmpty(actualText))
                {
                    return (false, "Narrator không đọc được nội dung (text rỗng).");
                }

                if (!string.Equals(actualText, expectedText, StringComparison.Ordinal))
                {
                    return (false, $"Narrator đọc '{actualText}' thay vì '{expectedText}'.");
                }

                if (!_axeHelper.IsAccessibilityCompliant())
                {
                    int errorCount = _axeHelper.GetTotalErrorCount();
                    string details = _axeHelper.GetErrorDetails();
                    return (false, $"Có {errorCount} lỗi accessibility. Chi tiết:\n{details}");
                }

                return (true, $"Narrator đọc đúng '{expectedText}' và không có lỗi accessibility.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra Narrator với quy trình Refocus (Shift+Tab rồi Tab).
        /// </summary>
        public (bool Success, string Message) VerifyNarratorWithRefocus(string expectedText, int waitTimeMs = 2000)
        {
            if (string.IsNullOrWhiteSpace(expectedText))
            {
                return (false, "expectedText không được rỗng");
            }

            if (waitTimeMs <= 0)
            {
                return (false, "waitTimeMs phải lớn hơn 0");
            }

            if (!AxeHelper.IsNarratorRunning())
            {
                return (false, "Narrator không chạy. Vui lòng bật Narrator (Win + Ctrl + Enter).");
            }

            try
            {
                _narratorVerifier.RefocusAndListen(waitTimeMs);

                string actualText = _narratorVerifier.GetLastName();

                if (string.IsNullOrEmpty(actualText))
                {
                    return (false, "Narrator không đọc được nội dung sau Refocus.");
                }

                if (!string.Equals(actualText, expectedText, StringComparison.Ordinal))
                {
                    return (false, $"Narrator đọc '{actualText}' thay vì '{expectedText}'.");
                }

                if (!_axeHelper.IsAccessibilityCompliant())
                {
                    int errorCount = _axeHelper.GetTotalErrorCount();
                    string details = _axeHelper.GetErrorDetails();
                    return (false, $"Có {errorCount} lỗi accessibility. Chi tiết:\n{details}");
                }

                return (true, $"Narrator đọc đúng '{expectedText}' sau Refocus và không có lỗi accessibility.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }   

        /// <summary>
        /// Kiểm tra chi tiết: Name + ControlType + Accessibility.
        /// </summary>
        public (bool Success, string Message) VerifyDetailedNarrator(string expectedName, string expectedControlType, string expectedState, int waitTimeMs = 2000)
        {
            if (string.IsNullOrWhiteSpace(expectedName))
            {
                return (false, "expectedName không được rỗng");
            }

            if (string.IsNullOrWhiteSpace(expectedControlType))
            {
                return (false, "expectedControlType không được rỗng");
            }

            if (string.IsNullOrWhiteSpace(expectedState))
            {
                return (false, "expectedState không được rỗng");
            }

            if (waitTimeMs <= 0)
            {
                return (false, "waitTimeMs phải lớn hơn 0");
            }

            if (!AxeHelper.IsNarratorRunning())
            {
                return (false, "Narrator không chạy. Vui lòng bật Narrator.");
            }

            try
            {
                _narratorVerifier.TabAndListen(waitTimeMs);

                string actualName = _narratorVerifier.GetLastName();
                string actualType = _narratorVerifier.GetLastControlType();
                string actualState = _narratorVerifier.GetLastState();

                var errors = new List<string>();

                if (string.IsNullOrEmpty(actualName))
                {
                    errors.Add("Narrator không đọc Name.");
                }
                else if (!string.Equals(actualName, expectedName, StringComparison.Ordinal))
                {
                    errors.Add($"Name khác nhau: '{actualName}' != '{expectedName}'.");
                }

                if (string.IsNullOrEmpty(actualType))
                {
                    errors.Add("Narrator không đọc ControlType.");
                }
                else if (!string.Equals(actualType, expectedControlType, StringComparison.Ordinal))
                {
                    errors.Add($"ControlType khác nhau: '{actualType}' != '{expectedControlType}'.");
                }

                if (string.IsNullOrEmpty(actualState))
                {
                    errors.Add("Narrator không đọc State.");
                }
                else if (!string.Equals(actualState, expectedState, StringComparison.Ordinal))
                {
                    errors.Add($"State khác nhau: '{actualState}' != '{expectedState}'.");
                }

                if (errors.Count > 0)
                {
                    return (false, string.Join("\n", errors));
                }

                if (!_axeHelper.IsAccessibilityCompliant())
                {
                    int errorCount = _axeHelper.GetTotalErrorCount();
                    string details = _axeHelper.GetErrorDetails();
                    return (false, $"Có {errorCount} lỗi accessibility. Chi tiết:\n{details}");
                }

                return (true, $"Name, ControlType và State khớp, không có lỗi accessibility.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// In log sự kiện đã bắt được (phục vụ debug).
        /// </summary>
        public void PrintEventLogs()
        {
            try
            {
                _narratorVerifier.PrintLogs();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi in log: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách sự kiện đã capture để tùy chỉnh kiểm tra.
        /// </summary>
        public List<NarratorEvent> GetCapturedEvents()
        {
            try
            {
                return _narratorVerifier.Events ?? new List<NarratorEvent>();
            }
            catch (Exception)
            {
                return new List<NarratorEvent>();
            }
        }
        /// <summary>
        /// Xóa tất cả event logs đã capture để reset trạng thái.
        /// </summary>
        public void ClearEvents()
        {
            try
            {
                _narratorVerifier?.ClearEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi clear events: {ex.Message}");
            }
        }
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                _narratorVerifier?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi dispose: {ex.Message}");
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}