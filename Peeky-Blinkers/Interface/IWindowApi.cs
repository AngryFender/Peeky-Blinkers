using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Peeky_Blinkers.Interface
{
    internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam); 
    internal delegate void WinEventProc(IntPtr hWinEventHook
                                            , uint eventType
                                            , IntPtr hwnd
                                            , int idObject
                                            , int idChild
                                            , uint dwEventThread
                                            , uint dwmsEventTime);
    internal delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    struct WinRect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    internal interface IWindowApi
    {
        bool EnumDesktopWindowsInvoke(IntPtr hDesktop, EnumWindowsProc eumWinProc, IntPtr lParam);
        IntPtr SetActiveWindowInvoke(IntPtr hWnd);
        bool GetWindowRectInvoke(IntPtr hWnd,out WinRect lpRect);
        bool MoveWindowInvoke(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        int GetWindowTextAInvoke(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        bool IsWindowVisibleInvoke(IntPtr hWnd);
        IntPtr SetWinEventHookInvoke(uint eventMin
                                     , uint eventMax
                                     , IntPtr hmodWinEventProc
                                     , WinEventProc fnWinEventProc
                                     , uint idProcess
                                     , uint idThread
                                     , uint dwFlag);
        IntPtr UnhookWinEventInvoke(IntPtr hWinEventHook);
        IntPtr SetWindowsHookExInvoke(int idHook
                                     , KeyboardProc lpfn
                                     , IntPtr hmod
                                     , uint dwThreadId);
        bool UnhookWindowsHookExInvoke(IntPtr hhk);
        IntPtr CallNextHookExInvoke(IntPtr hhk
                                    , int nCode
                                    , IntPtr wParam
                                    , IntPtr lParam);
        IntPtr GetModuleHandleAInvoke(string lpModuleName);
        IntPtr LoadLibraryInvoke(string lpFileName);
        IntPtr GetForegroundWindowInvoke();
        bool SetForegroundWindowInvoke(IntPtr hWnd);
        void keybd_eventInvoke(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        uint GetDpiForWindowInvoke(IntPtr hWnd);
    }
}
