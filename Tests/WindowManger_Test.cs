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
        public void Test_GetVisibleWindows()
        {
            var mock = new Mock<IWindowApi>();
            WindowManager winMan = new WindowManager(mock.Object);

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

            mock.Setup(x => x.IsWindowVisibleInvoke(It.IsAny<IntPtr>())).Returns((IntPtr hwnd) =>
            {
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

        [Fact]
        public void Test_GetTitledWindows()
        {
            var mock = new Mock<IWindowApi>();
            WindowManager winMan = new WindowManager(mock.Object);

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
                if (hWnd == (IntPtr)3)
                {
                    lpString.Clear();
                    lpString.Append("Test-Title");
                    return 10;
                }
                else
                {
                    return 0;
                }
            });

            mock.Setup(x => x.GetAllScreensRectangles()).Returns(rectangles);

            mock.Setup(x => x.GetWindowRectInvoke(It.IsAny<IntPtr>(), out mockRect)).Returns(true);

            mock.Setup(x => x.IsWindowVisibleInvoke(It.IsAny<IntPtr>())).Returns(true);

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

        [Fact]
        public void Test_GetCorrectWindowsOrder()
        {
            var mock = new Mock<IWindowApi>();
            WindowManager winMan = new WindowManager(mock.Object);

            var expectedList = new List<WindowInfo>();
            expectedList.Add(new WindowInfo((IntPtr)1, 0, 500, 400, 400, "Third", false));
            expectedList.Add(new WindowInfo((IntPtr)2, 0, 100, 400, 400, "Second", false));
            expectedList.Add(new WindowInfo((IntPtr)3, 0, 0, 400, 400, "First", false));

            List<IntPtr> mockHwdList = new List<IntPtr>();
            mockHwdList.Add((IntPtr)1);
            mockHwdList.Add((IntPtr)2);
            mockHwdList.Add((IntPtr)3);

            List<Rectangle> rectangles = new List<Rectangle>();
            rectangles.Add(new Rectangle(0, 0, 1920, 1080));

            WinRect mockRect1 = new WinRect()
            {
                left = 0,
                top = 500,
                right = 400,
                bottom = 400
            };

            WinRect mockRect2 = new WinRect()
            {
                left = 0,
                top = 100,
                right = 400,
                bottom = 400
            };

            WinRect mockRect3 = new WinRect()
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

                switch ((int)hWnd)
                {
                    case 3: lpString.Append("First"); return 5;
                    case 2: lpString.Append("Second"); return 6;
                    case 1: lpString.Append("Third"); return 5;
                    default: return 0;
                }
            });

            mock.Setup(x => x.GetAllScreensRectangles()).Returns(rectangles);

            mock.Setup(x => x.GetWindowRectInvoke(It.IsAny<IntPtr>(), out It.Ref<WinRect>.IsAny))
                .Callback(new CallBack_GetWindowRectInvoke((IntPtr hWnd, ref WinRect lpRect) =>
                {
                    switch ((int)hWnd)
                    {
                        case 1:
                            lpRect = mockRect1;
                            break;
                        case 2:
                            lpRect = mockRect2;
                            break;
                        case 3:
                            lpRect = mockRect3;
                            break;
                        default:
                            lpRect = new WinRect();
                            break;
                    }
                })).Returns(true);

            mock.Setup(x => x.IsWindowVisibleInvoke(It.IsAny<IntPtr>())).Returns(true);

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

            Assert.True(CompareWindowInfoLists(result, expectedList));
        }

        delegate void CallBack_GetWindowRectInvoke(IntPtr hWnd, ref WinRect lpRect);

        private bool CompareWindowInfoLists(List<WindowInfo> a, List<WindowInfo> b)
        {
            if(a.Count != b.Count)
            {
                return false;
            }

            int index = 0;
            while (index < a.Count)
            {
                if(a.ElementAt(index) != b.ElementAt(index))
                {
                    return false;
                }
                index++;
            }

            return true;
        }
    }
}
