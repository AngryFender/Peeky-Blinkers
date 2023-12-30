using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using System;
using System.ComponentModel;
using ContextMenu = System.Windows.Forms.ContextMenu;
using static Peeky_Blinkers.Overlay;
using System.Threading;
using MessageBox = System.Windows.MessageBox;

namespace Peeky_Blinkers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        private readonly WindowManager _winMan = new WindowManager(new Win());
        private bool _exitRequested = false;
        private bool _initialNotification = true;
        private readonly Mutex _mutex;
        private List<Overlay> _overlaysList = new List<Overlay>();

        public MainWindow()
        {
            bool isNewApp = true;
            _mutex = new Mutex(true, "Peeky Blinkers", out isNewApp);

            if (!isNewApp)
            {
                MessageBox.Show("Peeky Blinkers already running", "Multiple Instances", MessageBoxButton.OK, MessageBoxImage.Error);

                this.CloseApplication();
            }
            else
            {
                InitializeComponent();
                this._notifyIcon = new NotifyIcon
                {
                    BalloonTipText = "Peeky-Blinkers is minimized to tray",
                    BalloonTipTitle = "Peeky-Blinkers",
                    Text = "Peeky-Blinkers",
                    Icon = Properties.Resources.swap,
                    Visible = true
                };
                _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
                this.Closing += MainWindowClosing;
                this.StateChanged += MainWindowStateChanged;

                ContextMenu trayMenu = new ContextMenu();
                trayMenu.MenuItems.Add("Show App", (s, e) => ShowMainWindow());
                trayMenu.MenuItems.Add("Exit", (s, e) => CloseApplication());
                _notifyIcon.ContextMenu = trayMenu;

                _winMan.WindowAddRemoveHandler += WindowAddRemoveHandle;
                _winMan.SwapHandler += WindowSwapHandle;
                _winMan.ShowWindowsOverlay += ShowWindowsOverlayHandle;
                _winMan.HideWindowOverlay += HideWindowsOverlayHandle;
            }
        }

        private void HideWindowsOverlayHandle(object sender, EventArgs e)
        {
            foreach(Overlay overlay in _overlaysList)
            {
                overlay.Hide();
                overlay.OverlayUpdated -= OverlayUpdatedHandler;
            }
            _overlaysList.Clear();
        }

        private void ShowWindowsOverlayHandle(object sender, EventArgs e)
        {
            List<WindowInfo> winList = _winMan.GetCurrentWindowList();
            WinListView.ItemsSource = winList;

            foreach(WindowInfo win in winList)
            {
                float dpiFactor = _winMan.GetDpiFactorForSpecificWindow(win.HWnd);
                var overlay = new Overlay(win)
                {
                    Left = win.Left/dpiFactor,
                    Top = win.Top/dpiFactor,
                    Width = (win.Right - win.Left)/dpiFactor,
                    Height = (win.Bottom - win.Top)/dpiFactor,
                    ResizeMode = ResizeMode.NoResize
                };
                overlay.ShowUpdate();
                overlay.OverlayUpdated += OverlayUpdatedHandler;
                _overlaysList.Add(overlay);
            }
        }

        private void OverlayUpdatedHandler(object sender, OverlayEventArgs e)
        {
            List<WindowInfo> winList = _winMan.GetCurrentWindowList();
            
            foreach(WindowInfo win in winList)
            {
                if(win.HWnd == e.HWnd)
                {
                    win.IsSelected = e.IsSelected;
                }
            }

            WinListView.ItemsSource = winList;
        }

        private void MainWindowStateChanged(object sender, EventArgs e)
        {
            if(WindowState.Minimized == this.WindowState)
            {
                HideWindow();
            }
        }

        private void CloseApplication()
        {
            _exitRequested = true;
            Application.Current.Shutdown();
        }

        private void WindowSwapHandle(object sender, EventArgs e)
        {
            List<WindowInfo> winList = _winMan.GetCurrentWindowList();
            WinListView.ItemsSource = winList;

            if (_winMan.Swap())
            {
                List<WindowInfo> newList = _winMan.GetCurrentWindowList();
                WinListView.ItemsSource = newList;
                _winMan.SetCursor();
            }
       }

        private void WindowAddRemoveHandle(object sender, WindowInfoArgs e)
        {
            WinListView.ItemsSource = e.GetList();
            WinListView.Items.Refresh();
        }

        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            if (!_exitRequested)
            {
                e.Cancel = true;
                HideWindow();
            }
        }

        private void ShowMainWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void HideWindow()
        {
            this.Hide();
            _notifyIcon.Visible = true;
            if (_initialNotification)
            {
                _initialNotification = false;
                _notifyIcon.ShowBalloonTip(1);
            }
        }
    }
}
