using Moq;
using Xunit;
using Peeky_Blinkers;
using Peeky_Blinkers.Interface;
using System.Text;
using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace Tests
{
    public class WindowManager_Tests
    {
        [Fact]
        public void GetVisibleWindows()
        {
            var mock = new Mock<IWindowApi>();
            WindowManager winMan = WindowManager.GetInstance(mock.Object);

            WindowInfo info = new WindowInfo((IntPtr)3, 0, 0, 400, 400, "Test-Title", false);
            var expectedList = new List<WindowInfo>();
            expectedList.Add(info);

            List<IntPtr> mockHwdList = new List<IntPtr>();
            mockHwdList.Add((IntPtr)1);
            mockHwdList.Add((IntPtr)2);
            mockHwdList.Add((IntPtr)3);

            List<Rectangle> rectangles = new List<Rectangle>();
            rectangles.Add(new Rectangle(0, 0, 1920, 1080));

            WinRect mockRect = new WinRect()
            {
                left = 0,
                top = 0,
                right = 400,
                bottom = 400
            };

            StringBuilder sb = new StringBuilder(256);
            mock.Setup(x => x.GetWindowTextAInvoke(It.IsAny<IntPtr>(), It.IsAny<StringBuilder>(), It.IsAny<int>())).Returns((IntPtr hWnd, StringBuilder lpString, int nMaxCount) =>
            {
                lpString.Clear();
                lpString.Append("Test-Title");
                return 10;
            });

            mock.Setup(x => x.GetAllScreensRectangles()).Returns(rectangles);

            mock.Setup(x => x.GetWindowRectInvoke(It.IsAny<IntPtr>(), out mockRect)).Returns(true);

            mock.Setup(x => x.IsWindowVisibleInvoke(It.IsAny<IntPtr>())).Returns((IntPtr hwnd) => {
                if (hwnd == (IntPtr)3)
                {
                    return true;
                }
                return false;
            });

            mock.Setup(x => x.EnumDesktopWindowsInvoke(It.IsAny<IntPtr>()
                , It.IsAny<EnumWindowsProc>()
                , It.IsAny<IntPtr>()))
                .Returns((IntPtr hDesktop, EnumWindowsProc enumWinProc, IntPtr lParam) =>
                {
                    foreach (var hwnd in mockHwdList)
                    {
                        enumWinProc(hwnd, IntPtr.Zero);
                    }
                    return true;
                });

            List<WindowInfo> result = winMan.GetCurrentWindowList();

            Assert.True(result.Count() == expectedList.Count());

            var firstResult = result.First();
            var firstExpected = expectedList.First();

            Assert.True(firstResult == firstExpected);
        }
    }
}
