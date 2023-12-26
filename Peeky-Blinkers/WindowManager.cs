﻿using Peeky_Blinkers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Peeky_Blinkers
{
    internal class WindowManager: IDisposable
    {
        private readonly IWindowApi _winApi;
        private static WindowManager _windowManager;

        private static List<WindowInfo> _windowList = new List<WindowInfo>();
        private static List<WindowInfo> _rawWindowList = new List<WindowInfo>();
        private static readonly List<string> _banList = new List<string> {"Settings", "Peeky Blinkers", "NVIDIA GeForce Overlay", "Windows Input Experience", "Program Manager", "Peeky Blinkers Overlay"};
        private bool _forwardSequence = true;

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
        private const int SCREEN_SECTIONS = 8;

        private IntPtr _winEventHook;
        private IntPtr _keyboardEventHook;
        private WinEventProc _winEventProc;
        private KeyboardProc _keyboardProc;
        private WinRect _cursorWindow;
        private EnumWindowsProc _enumWinProc;


        public static WindowManager GetInstance(IWindowApi winApi)
        {
            if(null == _windowManager)
            {
                lock (typeof(WindowManager))
                {
                    _windowManager = new WindowManager(winApi);
                }
            }
            return _windowManager;
        }

        private WindowManager(IWindowApi winApi)
        {
            _winApi = winApi;

            _enumWinProc = new EnumWindowsProc(EnumWindowsCallBack);

            _winEventProc = new WinEventProc(WinEventHookHandler);
            _winEventHook = _winApi.SetWinEventHookInvoke(EVENT_SYSTEM_FOREGROUND
                                            , EVENT_SYSTEM_FOREGROUND
                                            , IntPtr.Zero
                                            , _winEventProc
                                            , 0
                                            , 0
                                            , WINEVENT_OUTOFCONTEXT);
             _keyboardProc = new KeyboardProc(KeyboardEventHookHandler);
            IntPtr hInstance = _winApi.LoadLibraryInvoke("User32");
            _keyboardEventHook = _winApi.SetWindowsHookExInvoke(WH_KEYBOARD_LL, _keyboardProc, hInstance,  0);
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
                            _forwardSequence = true;
                        }
                        else if (!isLeftShiftPressedDown && isRightShiftPressedDown)
                        {
                            HideAllWindowOverlay();
                            _forwardSequence = false;
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
            return _winApi.CallNextHookExInvoke(_keyboardEventHook, nCode, wParam, lParam);
        }

        public event EventHandler SwapHandler;

        private void RaiseSwap()
        {
            SwapHandler?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ShowWindowsOverlay;
        
        private void RaiseShowWindowsOverlay()
        {
            ShowWindowsOverlay?.Invoke(this,EventArgs.Empty);
        }

        public event EventHandler HideWindowOverlay;

        private void RaiseHideWindowsOverlay()
        {
            HideWindowOverlay?.Invoke(this,EventArgs.Empty);
        }

        public float GetDpiFactorForSpecificWindow(IntPtr hWnd)
        {
            float dpi = _winApi.GetDpiForWindowInvoke(hWnd);
            float g_DPIScale = dpi / 96.0f;
            return g_DPIScale != 0.0f ?  g_DPIScale: 1.0f;
        }

        public event EventHandler<WindowInfoArgs> WindowAddRemoveHandler;

        private void RaiseWindowInfoChanged(List<WindowInfo> list)
        {
            WindowAddRemoveHandler?.Invoke(this, new WindowInfoArgs(list));
        }

        private void WinEventHookHandler(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            List<WindowInfo> list = GetCurrentWindowList();
            RaiseWindowInfoChanged(list);
        }

        private void HideAllWindowOverlay()
        {
            if (isOverlayShown)
            {
                RaiseHideWindowsOverlay();
                isOverlayShown = false;
            }
        }

        private bool EnumWindowsCallBack(IntPtr hWnd, IntPtr lParam)
        {
            if (_winApi.GetWindowRectInvoke(hWnd, out WinRect rect) && _winApi.IsWindowVisibleInvoke(hWnd))
            {
                WindowInfo info = new WindowInfo(hWnd, rect.left, rect.top, rect.right, rect.bottom, null, false);
                _rawWindowList.Add(info);
            }
            return true;
        }
        
        private void GetEnumWindow()
        {
            _rawWindowList.Clear(); 
            _winApi.EnumDesktopWindowsInvoke(IntPtr.Zero,_enumWinProc, IntPtr.Zero);
        }

        private List<WindowInfo> FilterWindowWithTitles( )
        {
            List<WindowInfo> newList = new List<WindowInfo>();
            var OldWindowDict = _windowList.ToDictionary(win => win.HWnd);

            foreach(WindowInfo window in _rawWindowList)
            {
                StringBuilder buffer = new StringBuilder(256);
                int result = _winApi.GetWindowTextAInvoke(window.HWnd, buffer, buffer.Capacity);

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

        private List<WindowInfo> ScreenOrder(List<WindowInfo> list)
        {
            List<WindowInfo> newList = new List<WindowInfo>();

            var screens = Screen.AllScreens;

            foreach (var screen in screens) {
                int maxWidth = screen.Bounds.Width;
                int maxHeight = screen.Bounds.Height;
                int width = maxHeight / SCREEN_SECTIONS;

                list.Sort((x, y) => x.Left.CompareTo(y.Left));

                for (int w = width; w < maxWidth; w += width)
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
            if (_forwardSequence) 
            {
                newList.Reverse();
            }
            return newList;
        }

        public void Dispose()
        {
            _winApi.UnhookWinEventInvoke(_winEventHook);
            _winApi.UnhookWindowsHookExInvoke(_keyboardEventHook);
        }

        internal void Swap()
        {
            IntPtr cursorHWnd = _winApi.GetForegroundWindowInvoke();
            _winApi.GetWindowRectInvoke(cursorHWnd, out  _cursorWindow);

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
                _winApi.MoveWindowInvoke(movedWindow.HWnd
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
                    _winApi.keybd_eventInvoke(VK_ALT, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

                    _winApi.SetForegroundWindowInvoke(window.HWnd);

                    // Simulate Alt key release
                    _winApi.keybd_eventInvoke(VK_ALT, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
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

}
