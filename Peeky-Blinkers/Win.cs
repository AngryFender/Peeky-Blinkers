﻿using System;
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

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumWindowsProc eumWinProc, IntPtr lParam);

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

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool MoveWindow(IntPtr hWnd,int X, int Y, int nWidth, int nHeight, bool bRepaint);

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
        
        public List<WindowInfo> GetCurrentWindowList()
        {
            GetEnumWindow();
            return FilterWindowWithTitles();
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
            throw new NotImplementedException();
        }

        internal void Swap()
        {
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
            int nextIndex = 0;

            for(int index = 0; index < count; ++index)
            {
                nextIndex = index + 1;
                if(nextIndex < safe)
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
                if(!MoveWindow(movedWindow.HWnd
                    , movedWindow.Left
                    , movedWindow.Top
                    , movedWindow.Right - movedWindow.Left
                    , movedWindow.Bottom - movedWindow.Top
                    , true
                    ))
                {
                    MessageBox.Show(movedWindow.Title);
                }
            }
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
