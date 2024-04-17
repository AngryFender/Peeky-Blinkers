using System;
using System.Text;
using System.Runtime.InteropServices;
using Peeky_Blinkers.Interface;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace Peeky_Blinkers
{
    internal class Win : IWindowApi
    {
        [DllImport("user32.dll")]
        static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumWindowsProc eumWinProc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out WinRect lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

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

        [DllImport("user32.dll")]
        static extern bool IsIconic(IntPtr hWnd);

        private readonly List<Rectangle> _rectangles = new List<Rectangle>();

        public bool EnumDesktopWindowsInvoke(IntPtr hDesktop, Interface.EnumWindowsProc eumWinProc, IntPtr lParam)
        {
            return EnumDesktopWindows(hDesktop, eumWinProc, lParam);
        }

        public IntPtr SetActiveWindowInvoke(IntPtr hWnd)
        {
            return SetActiveWindow(hWnd);
        }

        public bool GetWindowRectInvoke(IntPtr hWnd, out Interface.WinRect lpRect)
        {
            return GetWindowRect(hWnd, out lpRect);
        }

        public bool MoveWindowInvoke(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint)
        {
            return MoveWindow(hWnd, X, Y, nWidth, nHeight, bRepaint);
        }

        public int GetWindowTextAInvoke(IntPtr hWnd, StringBuilder lpString, int nMaxCount)
        {
            return GetWindowTextA(hWnd, lpString, nMaxCount);
        }

        public bool IsWindowVisibleInvoke(IntPtr hWnd)
        {
            return IsWindowVisible(hWnd);
        }

        public IntPtr SetWinEventHookInvoke(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, Interface.WinEventProc fnWinEventProc, uint idProcess, uint idThread, uint dwFlag)
        {
            return SetWinEventHook(eventMin, eventMax, hmodWinEventProc, fnWinEventProc, idProcess, idThread, dwFlag);
        }

        public IntPtr UnhookWinEventInvoke(IntPtr hWinEventHook)
        {
            return UnhookWinEvent(hWinEventHook);
        }

        public IntPtr SetWindowsHookExInvoke(int idHook, Interface.KeyboardProc lpfn, IntPtr hmod, uint dwThreadId)
        {
            return SetWindowsHookEx(idHook, lpfn, hmod, dwThreadId);
        }

        public bool UnhookWindowsHookExInvoke(IntPtr hhk)
        {
            return UnhookWindowsHookEx(hhk);
        }

        public IntPtr CallNextHookExInvoke(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam)
        {
            return CallNextHookEx(hhk, nCode, wParam, lParam);
        }

        public IntPtr GetModuleHandleAInvoke(string lpModuleName)
        {
            return GetModuleHandleA(lpModuleName);
        }

        public IntPtr LoadLibraryInvoke(string lpFileName)
        {
            return LoadLibrary(lpFileName);
        }

        public IntPtr GetForegroundWindowInvoke()
        {
            return GetForegroundWindow();
        }

        public bool SetForegroundWindowInvoke(IntPtr hWnd)
        {
            return SetForegroundWindow(hWnd);
        }

        public void keybd_eventInvoke(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo)
        {
            keybd_event(bVk, bScan, dwFlags, dwExtraInfo);
        }

        public uint GetDpiForWindowInvoke(IntPtr hWnd)
        {
            return GetDpiForWindow(hWnd);
        }

        public List<Rectangle> GetAllScreensRectangles()
        {
            _rectangles.Clear();
            var screens = Screen.AllScreens;
            foreach(var screen in screens)
            {
                _rectangles.Add(screen.Bounds);
            }
            return _rectangles;
        }

        public bool IsIconicInvoke(IntPtr hWnd)
        {
            return IsIconic(hWnd);
        }

        public Win()
        {

        }
    }
}