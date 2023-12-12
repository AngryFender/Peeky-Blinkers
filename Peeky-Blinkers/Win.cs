using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;

namespace Peeky_Blinkers
{
    internal class Win : IDisposable
    {
        private static Win _singleWin;
        private static List<WindowInfo> _windowList = new List<WindowInfo>();
        private static List<WindowInfo> _rawWindowList = new List<WindowInfo>();

        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static bool isLeftShiftPressedDown = false;
        private static bool isRightShiftPressedDown = false;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYUP = 0x0105;
        private const int VK_LSHIFT = 0xA0;
        private const int VK_RSHIFT = 0xA1;

        private IntPtr _winEventHook;
        private IntPtr _keyboardEventHook;
        private WinEventProc _winEventProc;
        private KeyboardProc _keyboardProc;
        private WinRect _cursorWindow;
 
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        private delegate void WinEventProc(IntPtr hWinEventHook
                                            , uint eventType
                                            , IntPtr hwnd
                                            , int idObject
                                            , int idChild
                                            , uint dwEventThread
                                            , uint dwmsEventTime);
        private delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumWindowsProc eumWinProc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct WinRect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd,out WinRect lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool MoveWindow(IntPtr hWnd,int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern int GetWindowTextA(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin
                                            , uint eventMax
                                            , IntPtr hmodWinEventProc
                                            , WinEventProc fnWinEventProc
                                            , uint idProcess
                                            , uint idThread
                                            , uint dwFlag);

        [DllImport("user32.dll")]
        static extern IntPtr UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook
            , KeyboardProc lpfn
            , IntPtr hmod
            , uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hhk
            , int nCode
            , IntPtr wParam
            , IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandleA(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public static Win GetInstance()
        {
            if (null == _singleWin)
            {
                _singleWin = new Win();
            }
            return _singleWin;
        }

        private Win()
        {
            _winEventProc = new WinEventProc(WinEventHookHandler);
            _winEventHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND
                                            , EVENT_SYSTEM_FOREGROUND
                                            , IntPtr.Zero
                                            , _winEventProc
                                            , 0
                                            , 0
                                            , WINEVENT_OUTOFCONTEXT);
             _keyboardProc = new KeyboardProc(KeyboardEventHookHandler);
            IntPtr hInstance = LoadLibrary("User32");
            _keyboardEventHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, hInstance,  0);
        }

        public List<WindowInfo> GetCurrentWindowList()
        {
            GetEnumWindow();
            return FilterWindowWithTitles();
        }

        private IntPtr KeyboardEventHookHandler(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (_windowList.Count() >0 && nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool isKeyDown = wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN;
                bool isKeyUp = wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP;

                if (isKeyDown)
                {
                    if (vkCode == VK_LSHIFT)
                    {
                        isLeftShiftPressedDown = true;
                    }
                    else if (vkCode == VK_RSHIFT)
                    {
                        isRightShiftPressedDown = true;
                    }

                    if (isLeftShiftPressedDown && isRightShiftPressedDown)
                    {
                        RaiseSwap();
                    }
                }

                if (isKeyUp)
                { 
                    if (vkCode == VK_LSHIFT)
                    {
                        isLeftShiftPressedDown = false;
                    }
                    else if (vkCode == VK_RSHIFT)
                    {
                        isRightShiftPressedDown = false;
                    }
                }

            }
            return CallNextHookEx(_keyboardEventHook, nCode, wParam, lParam);
        }

        public event EventHandler SwapHandler;

        public void RaiseSwap()
        {
            SwapHandler?.Invoke(this, EventArgs.Empty);
        }

        private void WinEventHookHandler(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            List<WindowInfo> list = GetCurrentWindowList();
            RaiseWindowInfoChanged(list);
        }

        public event EventHandler<WindowInfoArgs> WindowAddRemoveHandler;

        public void RaiseWindowInfoChanged(List<WindowInfo> list)
        {
            WindowAddRemoveHandler?.Invoke(this, new WindowInfoArgs(list));
        }

        private bool EnumWindowsCallBack(IntPtr hWnd, IntPtr lParam)
        {
            if (GetWindowRect(hWnd, out WinRect rect) && IsWindowVisible(hWnd))
            {
                WindowInfo info = new WindowInfo(hWnd, rect.left, rect.top, rect.right, rect.bottom, null, false);
                _rawWindowList.Add(info);
            }
            return true;
        }
        
        public void GetEnumWindow()
        {
            _rawWindowList.Clear(); 
            EnumWindowsProc enumWinProc = new EnumWindowsProc(EnumWindowsCallBack);
            EnumDesktopWindows(IntPtr.Zero,enumWinProc, IntPtr.Zero);
        }

        public List<WindowInfo> FilterWindowWithTitles( )
        {
            List<WindowInfo> newList = new List<WindowInfo>();
            foreach(WindowInfo window in _rawWindowList)
            {
                StringBuilder buffer = new StringBuilder(256);
                int result = GetWindowTextA(window.HWnd, buffer, buffer.Capacity);

                if (result > 0)
                {
                    window.Title = buffer.ToString();
                    newList.Add(window);
                }
            }

            var OldWindowDict = _windowList.ToDictionary(win => win.HWnd);
            foreach(WindowInfo newWin in newList)
            {
                if(OldWindowDict.TryGetValue(newWin.HWnd, out WindowInfo oldWin)){
                    if(newWin.HWnd == oldWin.HWnd)
                    {
                        newWin.IsSelected = oldWin.IsSelected;
                    }
                }
            }
            _windowList = newList;
            return newList;
        }

        public void Dispose()
        {
            UnhookWinEvent(_winEventHook);
            UnhookWindowsHookEx(_keyboardEventHook);
        }

        internal void Swap()
        {
            IntPtr cursorHWnd = GetForegroundWindow();
            GetWindowRect(cursorHWnd, out  _cursorWindow);

            List<WindowInfo> selectedWindowList = new List<WindowInfo> ();
            List<IntPtr> hwndList = new List<IntPtr>();
            foreach(var window in _windowList)
            {
                if (window.IsSelected)
                {
                    hwndList.Add(window.HWnd);
                    selectedWindowList.Add(window);
                }
            }

            int safe = hwndList.Count();
            int count = selectedWindowList.Count();
            for (int index = 0; index < count; ++index)
            {
                int nextIndex = index + 1;
                if (nextIndex < safe)
                {
                    selectedWindowList[index].HWnd = hwndList[nextIndex];
                }
                else
                {
                    selectedWindowList[index].HWnd = hwndList[0] ;
                }
            }

            foreach(var movedWindow in selectedWindowList)
            {
                MoveWindow(movedWindow.HWnd
                    , movedWindow.Left
                    , movedWindow.Top
                    , movedWindow.Right - movedWindow.Left
                    , movedWindow.Bottom - movedWindow.Top
                    , true );
            }
        }

        internal void SetCursor()
        {
            foreach(var window in _windowList)
            {
                if(_cursorWindow.left == window.Left &&
                   _cursorWindow.top == window.Top &&
                   _cursorWindow.right == window.Right &&
                   _cursorWindow.bottom == window.Bottom)
                {
                    SetForegroundWindow(window.HWnd);
                }
            }
        }
    }

    internal class WindowInfoArgs : EventArgs
    {
        private readonly List<WindowInfo> windowList;

        public List<WindowInfo> GetList()
        {
            return windowList;
        }

        public WindowInfoArgs(List<WindowInfo> list)
        {
            this.windowList = list;
        }
    }

    internal class WindowInfo
    {
        public WindowInfo(IntPtr hWnd, int left, int top, int right, int bottom, string title, bool isSelected)
        {
            HWnd = hWnd;
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
            Title = title;
            IsSelected = isSelected;
        }

        public IntPtr HWnd { get; set; }
        public int Left { get; } 
        public int Top { get; }
        public int Right { get; }
        public int Bottom { get; }
        public string Title { get; set; }
        public bool IsSelected { get; set; }
    }
}
