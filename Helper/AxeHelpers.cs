using Axe.Windows.Automation;
using Axe.Windows.Automation.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SamsungCloudTest.Helper
{
    public class AxeHelper
    {
        // Cấu hình Scanner của Axe
        private readonly IScanner _scanner;

        public AxeHelper()
        {
            // Config Axe để quét Process hiện tại
            // Lưu ý: Cần đảm bảo process ID đúng, hoặc đơn giản là quét Window đang active, đó là process của test runner (NUnit), không phải app đang test (Samsung Cloud)
            var config = Config.Builder.ForProcessId(Process.GetCurrentProcess().Id).Build();
            _scanner = ScannerFactory.CreateScanner(config);
        }

        // Phương thức quét và trả về kết quả
        public ScanOutput ScanData()
        {
            try
            {
                return _scanner.Scan(null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi quét Accessibility: {ex.Message}", ex);
            }
        }

        // Phương thức kiểm tra nhanh: Trả về true nếu không có lỗi
        public bool IsAccessibilityCompliant()
        {
            try
            {
                var results = ScanData();

                // Kiểm tra null
                if (results == null || results.WindowScanOutputs == null)
                {
                    return false;
                }

                // Kiểm tra tất cả các window scan outputs
                // Trả về true nếu KHÔNG có lỗi nào trong tất cả các windows
                return results.WindowScanOutputs.All(window =>
                    window != null &&
                    window.Errors != null &&
                    !window.Errors.Any());
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Phương thức lấy tổng số lỗi từ tất cả các windows
        public int GetTotalErrorCount()
        {
            try
            {
                var results = ScanData();

                if (results == null || results.WindowScanOutputs == null)
                {
                    return 0;
                }

                return results.WindowScanOutputs
                    .Where(w => w != null)
                    .Sum(window => window.ErrorCount);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // Lấy chi tiết lỗi để log
        public string GetErrorDetails()
        {
            try
            {
                var results = ScanData();

                if (results == null || results.WindowScanOutputs == null)
                {
                    return "Không thể lấy kết quả scan.";
                }

                var errorMessages = new StringBuilder();
                int totalErrors = 0;

                foreach (var window in results.WindowScanOutputs.Where(w => w != null))
                {
                    if (window.Errors == null || !window.Errors.Any())
                        continue;

                    foreach (var error in window.Errors)
                    {
                        totalErrors++;

                        string ruleName = error.Rule?.Description ?? "Unknown Rule";
                        string elementName = TryGetProperty(error.Element?.Properties, "Name", "AutomationId", "AutomationID") ?? "N/A";
                        string controlType = TryGetProperty(error.Element?.Properties, "ControlType", "LocalizedControlType") ?? "Unknown";

                        errorMessages.AppendLine($"[{totalErrors}] Rule: {ruleName}");
                        errorMessages.AppendLine($"    Element: {elementName} ({controlType})");
                    }
                }

                return totalErrors > 0
                    ? $"Tìm thấy {totalErrors} lỗi Accessibility:\n{errorMessages}"
                    : "Không có lỗi Accessibility.";
            }
            catch (Exception ex)
            {
                return $"Lỗi khi lấy chi tiết: {ex.Message}";
            }
        }

        // Kiểm tra Narrator có đang chạy không
        public static bool IsNarratorRunning()
        {
            try
            {
                return Process.GetProcessesByName("Narrator").Any();
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Kiểm tra element có đủ thuộc tính để Narrator đọc không
        public bool CanNarratorReadElement(string automationId)
        {
            try
            {
                var results = ScanData();

                if (results == null || results.WindowScanOutputs == null)
                {
                    return false;
                }

                var allElements = results.WindowScanOutputs
                    .Where(w => w != null && w.Errors != null)
                    .SelectMany(w => w.Errors)
                    .Where(e => e?.Element != null)
                    .Select(e => e.Element);

                var targetElement = allElements.FirstOrDefault(e =>
                {
                    var automationIdValue = TryGetProperty(e.Properties, "AutomationId", "AutomationID");
                    return string.Equals(automationIdValue, automationId, StringComparison.OrdinalIgnoreCase);
                });

                if (targetElement == null)
                {
                    return false;
                }

                var name = TryGetProperty(targetElement.Properties, "Name");
                var helpText = TryGetProperty(targetElement.Properties, "HelpText");

                bool hasName = !string.IsNullOrWhiteSpace(name);
                bool hasHelpText = !string.IsNullOrWhiteSpace(helpText);

                return hasName || hasHelpText;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string? TryGetProperty(IReadOnlyDictionary<string, string>? properties, params string[] keys)
        {
            if (properties == null || keys == null)
            {
                return null;
            }

            foreach (var key in keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (properties.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }
    }
}