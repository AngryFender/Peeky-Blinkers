using System;
using System.Collections.Generic;

namespace Peeky_Blinkers
{
    public class WindowInfo
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

        public static bool operator ==(WindowInfo a, WindowInfo b)
        {
            if(a.HWnd != b.HWnd){ return false; }
            if(a.Left != b.Left) {  return false; }
            if(a.Top != b.Top) { return false; }
            if(a.Right != b.Right) { return false; }
            if(a.Bottom != b.Bottom) { return false; }
            if (a.Title != b.Title) {  return false; }
            if(a.IsSelected != b.IsSelected) { return false; }

            return true;
        }

        public static bool operator !=(WindowInfo a, WindowInfo b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is WindowInfo info &&
                   EqualityComparer<IntPtr>.Default.Equals(HWnd, info.HWnd) &&
                   Left == info.Left &&
                   Top == info.Top &&
                   Right == info.Right &&
                   Bottom == info.Bottom &&
                   Title == info.Title &&
                   IsSelected == info.IsSelected;
        }

        public override int GetHashCode()
        {
            int hashCode = 221842514;
            hashCode = hashCode * -1521134295 + HWnd.GetHashCode();
            hashCode = hashCode * -1521134295 + Left.GetHashCode();
            hashCode = hashCode * -1521134295 + Top.GetHashCode();
            hashCode = hashCode * -1521134295 + Right.GetHashCode();
            hashCode = hashCode * -1521134295 + Bottom.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + IsSelected.GetHashCode();
            return hashCode;
        }
    }
}
