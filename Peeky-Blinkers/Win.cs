using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Policy;

namespace Peeky_Blinkers
{
    internal class Win : IDisposable
    {
        private static Win _singleWin;
        private static readonly List<WindowInfo> _windowList = new List<WindowInfo>();

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc eumWinProc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern int GetWindowTextA(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);


        public static Win GetInstance()
        {
            if (null == _singleWin)
            {
                _singleWin = new Win();
            }
            return _singleWin;
        }

        public void SelectWindow(IntPtr hWnd)
        {
            if (_windowList.Any(window => window.HWnd == hWnd))
            {
                ShowWindow(hWnd,9);
            }
        }

        private bool EnumWindowsCallBack(IntPtr hWnd, IntPtr lParam)
        {
            WinRect rect;
            if (GetWindowRect(hWnd, out rect))
            {
                WindowInfo info = new WindowInfo(hWnd, rect.left, rect.top, rect.right, rect.bottom, null);
                _windowList.Add(info);
            }
            return true;
        }
        
        public List<WindowInfo> GetEnumWindow()
        {
            _windowList.Clear(); 
            EnumWindowsProc enumWinProc = new EnumWindowsProc(EnumWindowsCallBack);
            EnumWindows(enumWinProc, IntPtr.Zero);
            return _windowList;
        }

        public void FilterWindowVisible()
        {

            List<WindowInfo> removeList = new List<WindowInfo>();
            foreach(var window in _windowList)
            {
                if(!IsWindowVisible(window.HWnd))
                {
                    removeList.Add(window);
                }
            }

            foreach(var reject in removeList)
            {
                _windowList.Remove(reject);
            }
        }

        public void FilterWindowTitles()
        {

            List<WindowInfo> removeList = new List<WindowInfo>();
            foreach(var window in _windowList)
            {
                StringBuilder buffer = new StringBuilder(256);
                int result = GetWindowTextA(window.HWnd, buffer, buffer.Capacity);

                if (result > 0)
                {
                    window.Title = buffer.ToString();
                }
                else
                {
                    removeList.Add(window);
                }
            }

            foreach(var reject in removeList)
            {
                _windowList.Remove(reject);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }

    internal class WindowInfo
    {
        public WindowInfo(IntPtr hWnd, int left, int top, int right, int bottom, string title)
        {
            HWnd = hWnd;
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
            Title = title;
        }

        public IntPtr HWnd { get; }
        public int Left { get; }
        public int Top { get; }
        public int Right { get; }
        public int Bottom { get; }
        public string Title { get; set; }
    }
}
