using System;

namespace Peeky_Blinkers
{
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
