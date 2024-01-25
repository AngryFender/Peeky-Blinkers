using System;
using System.Collections.Generic;

namespace Peeky_Blinkers
{
    public class WindowInfo
    {
        public WindowInfo(WindowInfo old) { 
            HWnd = old.HWnd;
            Left = old.Left;
            Top = old.Top;
            Right = old.Right;
            Bottom = old.Bottom;
            Title = old.Title;
            IsSelected = old.IsSelected;
        }

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
        public int Left { get; set; } 
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
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

        public WindowInfo Copy(WindowInfo original)
        {
            WindowInfo copy = new WindowInfo(original.HWnd,
                                            original.Left,
                                            original.Top, 
                                            original.Right, 
                                            original.Bottom, 
                                            original.Title, 
                                            original.IsSelected);
            return copy;
        }

        public void MoveStep(WindowInfo original, int factor)
        {
            Left = Left + (original.Left - Left) / factor;
            Right = Right + (original.Right - Right) / factor;
            Top = Top + (original.Top - Top) / factor;
            Bottom = Bottom + (original.Bottom - Bottom) / factor;
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
