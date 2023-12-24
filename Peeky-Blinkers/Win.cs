﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace Peeky_Blinkers
{
    internal class Win : IDisposable
    {
        private static Win _singleWin;
        private static List<WindowInfo> _windowList = new List<WindowInfo>();
        private static List<WindowInfo> _rawWindowList = new List<WindowInfo>();
        private static readonly List<string> _banList = new List<string> {"Settings", "Peeky Blinkers", "NVIDIA GeForce Overlay", "Windows Input Experience", "Program Manager", "Peeky Blinkers Overlay"};

        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private bool isLeftShiftPressedDown = false;
        private bool isRightShiftPressedDown = false;
        private bool isLAltPressedDown = false;
        private bool isOverlayShown = false;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYUP = 0x0105;
        private const int VK_LSHIFT = 0xA0;
        private const int VK_RSHIFT = 0xA1;
        private const int VK_ALT = 0x12;
        private const int VK_LALT = 164;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const int SCREEN_SCAN_SECTIONS = 8;

        private IntPtr _winEventHook;
        private IntPtr _keyboardEventHook;
        private WinEventProc _winEventProc;
        private KeyboardProc _keyboardProc;
        private WinRect _cursorWindow;
        private bool forwardSequence = true;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        EnumWindowsProc _enumWinProc;

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

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static extern uint GetDpiForWindow(IntPtr hWnd);

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
            _enumWinProc = new EnumWindowsProc(EnumWindowsCallBack);

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
            if (_windowList.Count() > 0 && nCode >= 0)
            {
                bool isKeyDown = wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN;
                bool isKeyUp = wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP;

                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == VK_LSHIFT
                 || vkCode == VK_RSHIFT
                 || vkCode == VK_LALT)
                {
                    if (isKeyDown)
                    {
                        switch (vkCode)
                        {
                            case VK_LSHIFT: isLeftShiftPressedDown = true; break;
                            case VK_RSHIFT: isRightShiftPressedDown = true; break;
                            case VK_LALT: isLAltPressedDown = true; break;
                        }

                        if (isLeftShiftPressedDown && !isRightShiftPressedDown)
                        {
                            HideAllWindowOverlay();
                            forwardSequence = true;
                        }
                        else if (!isLeftShiftPressedDown && isRightShiftPressedDown)
                        {
                            HideAllWindowOverlay();
                            forwardSequence = false;
                        }

                        if (isLeftShiftPressedDown && isRightShiftPressedDown && !isLAltPressedDown)
                        {
                            HideAllWindowOverlay();
                            RaiseSwap();
                        }
                        else if (isLeftShiftPressedDown && isRightShiftPressedDown && isLAltPressedDown)
                        {
                            if (!isOverlayShown)
                            {
                                RaiseShowWindowsOverlay();
                                isOverlayShown = true;
                            }
                        }
                    }

                    if (isKeyUp)
                    {
                        switch (vkCode)
                        {
                            case VK_LSHIFT: isLeftShiftPressedDown = false; break;
                            case VK_RSHIFT: isRightShiftPressedDown = false; break;
                            case VK_LALT: isLAltPressedDown = false; break;
                        }
                    }
                }
                else
                {
                    if (isKeyDown && !isLAltPressedDown && !isLAltPressedDown)
                    {
                        HideAllWindowOverlay();
                    }
                }
            }
            return CallNextHookEx(_keyboardEventHook, nCode, wParam, lParam);
        }

        private void HideAllWindowOverlay()
        {
            if (isOverlayShown)
            {
                RaiseHideWindowsOverlay();
                isOverlayShown = false;
            }
        }

        public event EventHandler SwapHandler;

        public void RaiseSwap()
        {
            SwapHandler?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ShowWindowsOverlay;
        
        public void RaiseShowWindowsOverlay()
        {
            ShowWindowsOverlay?.Invoke(this,EventArgs.Empty);
        }

        public event EventHandler HideWindowOverlay;

        public void RaiseHideWindowsOverlay()
        {
            HideWindowOverlay?.Invoke(this,EventArgs.Empty);
        }

        public float GetDpiFactorForSpecificWindow(IntPtr hWnd)
        {
            float dpi = GetDpiForWindow(hWnd);
            float g_DPIScale = dpi / 96.0f;
            return g_DPIScale != 0.0f ?  g_DPIScale: 1.0f;
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
            EnumDesktopWindows(IntPtr.Zero,_enumWinProc, IntPtr.Zero);
        }

        public List<WindowInfo> FilterWindowWithTitles( )
        {
            List<WindowInfo> newList = new List<WindowInfo>();
            var OldWindowDict = _windowList.ToDictionary(win => win.HWnd);

            foreach(WindowInfo window in _rawWindowList)
            {
                StringBuilder buffer = new StringBuilder(256);
                int result = GetWindowTextA(window.HWnd, buffer, buffer.Capacity);

                if (result > 0)
                {
                    window.Title = buffer.ToString();
                    bool isTitleBanned = _banList. Contains(window.Title);

                    if (isTitleBanned)
                    {
                        continue;
                    }

                    newList.Add(window);

                    if(OldWindowDict.TryGetValue(window.HWnd, out WindowInfo oldWin)){
                        if(window.HWnd == oldWin.HWnd)
                        {
                            window.IsSelected = oldWin.IsSelected;
                        }
                    }
                }
            }

            _windowList = ScreenOrder(newList);
            return _windowList;
        }

        public List<WindowInfo> ScreenOrder(List<WindowInfo> list)
        {
            List<WindowInfo> newList = new List<WindowInfo>();

            var Screens = Screen.AllScreens;
            foreach (var screen in Screens)
            {
                int maxWidth = screen.Bounds.Width + screen.Bounds.X;
                int width = screen.Bounds.Width / SCREEN_SCAN_SECTIONS;

                list.Sort((x, y) => x.Left.CompareTo(y.Left));

                for (int w = screen.Bounds.X; w < maxWidth; w += width)
                {
                    List<WindowInfo> column = new List<WindowInfo>();

                    foreach (WindowInfo window in list)
                    {
                        if (w > window.Left && w < window.Right)
                        {
                            column.Add(window);
                        }
                    }

                    column.Sort((x, y) => x.Top.CompareTo(y.Top));

                    foreach (WindowInfo window in column)
                    {
                        newList.Add(window);
                        list.Remove(window);
                    }
                }
            }
            if (forwardSequence) 
            {
                newList.Reverse();
            }

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
                    // Simulate Alt key press to bypass restrictions on SetForegroundWindow
                    keybd_event(VK_ALT, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

                    SetForegroundWindow(window.HWnd);

                    // Simulate Alt key release
                    keybd_event(VK_ALT, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
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
