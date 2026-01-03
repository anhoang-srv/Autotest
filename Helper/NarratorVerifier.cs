using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.EventHandlers;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GalaxyCloud.Helpers
{
    /// <summary>
    /// Class lưu trữ thông tin thô của một sự kiện Narrator để không phải xử lý chuỗi phức tạp.
    /// </summary>
    public class NarratorEvent
    {
        /// <summary>
        /// Gets or sets tên (Name) của element được đọc.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets loại điều khiển (ControlType) của element (ví dụ: Button, Text).
        /// </summary>
        public string ControlType { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets trạng thái (State) của element (ví dụ: Checked, Unchecked, Collapsed, Expanded).
        /// </summary>
        public string State { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets thời gian xảy ra sự kiện.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Chuyển đổi thông tin sự kiện sang chuỗi để debug.
        /// </summary>
        /// <returns>Chuỗi định dạng log.</returns>
        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] Name: {Name} | ControlType: {ControlType} | State: {State}";
        }
    }

    /// <summary>
    /// Lớp hỗ trợ kiểm tra (Verify) hoạt động của Narrator bằng cách lắng nghe sự kiện Focus từ UI Automation.
    /// Hỗ trợ điều khiển bàn phím (Tab, Shift+Tab) và xử lý đa luồng an toàn.
    /// </summary>
    public class NarratorVerifier : IDisposable
    {
        /// <summary>
        /// Đối tượng Automation chuẩn UIA3 để tương tác với hệ thống.
        /// </summary>
        private readonly UIA3Automation _automation;

        /// <summary>
        /// Biến lưu trữ trình xử lý sự kiện Focus, dùng để hủy đăng ký khi không dùng nữa.
        /// </summary>
        private FocusChangedEventHandlerBase? _focusHandler;

        /// <summary>
        /// Đối tượng dùng để khóa luồng (Thread-safety) khi truy cập danh sách Events.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Gets danh sách lưu trữ các sự kiện Focus đã bắt được.
        /// </summary>
        public List<NarratorEvent> Events { get; private set; }

        /// <summary>
        /// Khởi tạo một instance mới của lớp <see cref="NarratorVerifier"/>.
        /// </summary>
        public NarratorVerifier()
        {
            _automation = new UIA3Automation();
            Events = new List<NarratorEvent>();
        }

        #region Input Methods (Điều khiển)

        /// <summary>
        /// Gửi phím TAB vào hệ thống để chuyển focus sang element tiếp theo.
        /// </summary>
        public void SendTabKey()
        {
            Keyboard.Press(VirtualKeyShort.TAB);
        }

        /// <summary>
        /// Gửi tổ hợp phím SHIFT + TAB (Đi lùi).
        /// Cần thiết để reset focus ra khỏi element hiện tại rồi quay lại.
        /// </summary>
        public void SendShiftTabKey()
        {
            // Giữ Shift -> Nhấn Tab -> Nhả Shift
            Keyboard.Pressing(VirtualKeyShort.SHIFT);
            Keyboard.Press(VirtualKeyShort.TAB);
            Keyboard.Release(VirtualKeyShort.SHIFT);
        }

        /// <summary>
        /// Thực hiện: Shift+Tab (lùi) -> Start Listening -> Tab (tiến) -> Wait -> Stop.
        /// </summary>
        /// <param name="waitTimeInMs">Thời gian chờ Narrator đọc (mặc định 2000ms).</param>
        public void RefocusAndListen(int waitTimeInMs = 2000)
        {
            // Bước 1: Lùi ra element trước đó (để mất focus khỏi nút hiện tại)
            SendShiftTabKey();
            Thread.Sleep(500); // Chờ UI ổn định

            // Bước 2: Bắt đầu nghe -> Tiến lại -> Chờ -> Dừng
            StartListening();
            SendTabKey();
            Thread.Sleep(waitTimeInMs);
            StopListening();
        }

        /// <summary>
        /// Quy trình chuẩn: Bắt đầu nghe -> Nhấn Tab -> Chờ sự kiện -> Dừng nghe.
        /// </summary>
        /// <param name="waitTimeInMs">Thời gian chờ Narrator đọc (mặc định 2000ms).</param>
        public void TabAndListen(int waitTimeInMs = 2000)
        {
            StartListening();
            SendTabKey();
            Thread.Sleep(waitTimeInMs);
            StopListening();
        }

        #endregion

        #region Core Logic

        /// <summary>
        /// Bắt đầu lắng nghe sự kiện FocusChanged trên toàn bộ hệ thống.
        /// Xóa danh sách log cũ trước khi bắt đầu.
        /// </summary>
        public void StartListening()
        {
            lock (_lockObject)
            {
                Events.Clear();
            }

            // Hủy đăng ký cũ nếu có để tránh duplicate handler
            if (_focusHandler != null)
            {
                StopListening();
            }

            _focusHandler = _automation.RegisterFocusChangedEvent(OnFocusChanged);
        }

        /// <summary>
        /// Hàm callback được gọi tự động khi tiêu điểm (focus) của UI thay đổi.
        /// </summary>
        /// <param name="element">Thành phần UI nhận được focus.</param>
        private void OnFocusChanged(AutomationElement element)
        {
            try
            {
                if (element != null)
                {
                    // Lấy dữ liệu ngay lập tức (Snapshot) để tránh lỗi truy cập cross-thread sau này
                    string state = string.Empty;
                    try
                    {
                        // Cố gắng lấy state từ patterns
                        var togglePattern = element.Patterns.Toggle.PatternOrDefault;
                        var expandCollapsePattern = element.Patterns.ExpandCollapse.PatternOrDefault;
                        
                        if (togglePattern != null)
                        {
                            state = togglePattern.ToggleState.ToString();
                        }
                        else if (expandCollapsePattern != null)
                        {
                            state = expandCollapsePattern.ExpandCollapseState.ToString();
                        }
                    }
                    catch
                    {
                        // Nếu không lấy được state thì để trống
                    }

                    var newEvent = new NarratorEvent
                    {
                        Name = element.Name ?? string.Empty,
                        ControlType = element.ControlType.ToString(),
                        State = state,
                        Timestamp = DateTime.Now
                    };

                    lock (_lockObject)
                    {
                        Events.Add(newEvent);
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi truy cập element đã bị hủy
            }
        }

        /// <summary>
        /// Dừng lắng nghe sự kiện FocusChanged và hủy đăng ký handler.
        /// </summary>
        public void StopListening()
        {
            if (_focusHandler != null)
            {
                try
                {
                    _automation.UnregisterFocusChangedEvent(_focusHandler);
                }
                catch
                {
                    // Bỏ qua lỗi nếu automation đã bị dispose
                }
                finally
                {
                    _focusHandler = null;
                }
            }
        }

        #endregion

        #region Getters (Truy xuất dữ liệu)

        /// <summary>
        /// Lấy thuộc tính Name (tên) của sự kiện Focus cuối cùng.
        /// </summary>
        /// <returns>Tên element hoặc chuỗi rỗng nếu không có dữ liệu.</returns>
        public string GetLastName()
        {
            lock (_lockObject)
            {
                var lastEvent = Events.LastOrDefault();
                return lastEvent != null ? lastEvent.Name : string.Empty;
            }
        }

        /// <summary>
        /// Lấy thuộc tính ControlType (loại điều khiển) của sự kiện Focus cuối cùng.
        /// </summary>
        /// <returns>Loại control hoặc chuỗi rỗng nếu không có dữ liệu.</returns>
        public string GetLastControlType()
        {
            lock (_lockObject)
            {
                var lastEvent = Events.LastOrDefault();
                return lastEvent != null ? lastEvent.ControlType : string.Empty;
            }
        }

        /// <summary>
        /// Lấy thuộc tính State (trạng thái) của sự kiện Focus cuối cùng.
        /// </summary>
        /// <returns>Trạng thái element hoặc chuỗi rỗng nếu không có dữ liệu.</returns>
        public string GetLastState()
        {
            lock (_lockObject)
            {
                var lastEvent = Events.LastOrDefault();
                return lastEvent != null ? lastEvent.State : string.Empty;
            }
        }

        /// <summary>
        /// In toàn bộ log các sự kiện đã bắt được ra Console để phục vụ Debug.
        /// </summary>
        public void PrintLogs()
        {
            lock (_lockObject)
            {
                foreach (var evt in Events)
                {
                    Console.WriteLine(evt.ToString());
                }
            }
        }

        /// <summary>
        /// Xóa tất cả event logs đã capture để reset trạng thái.
        /// </summary>
        public void ClearEvents()
        {
            lock (_lockObject)
            {
                Events.Clear();
            }
        }

        #endregion

        /// <summary>
        /// Giải phóng tài nguyên Automation và hủy đăng ký sự kiện.
        /// </summary>
        public void Dispose()
        {
            StopListening();
            _automation?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}