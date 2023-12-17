using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Drawing;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System;
using System.ComponentModel;
using ContextMenu = System.Windows.Forms.ContextMenu;
using System.Windows.Input;

namespace Peeky_Blinkers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        private readonly Win _win = Win.GetInstance();
        private bool _exitRequested = false;
        private bool _initialNotification = true;
        private List<Overlay> _overlaysList = new List<Overlay>();
        
        public MainWindow()
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

            _win.WindowAddRemoveHandler += WindowAddRemoveHandle;
            _win.SwapHandler += WindowSwapHandle;
            _win.ShowWindowsOverlay += ShowWindowsOverlayHandle;
            _win.HideWindowOverlay += HideWindowsOverlayHandle;
        }

        private void HideWindowsOverlayHandle(object sender, EventArgs e)
        {
            foreach(Overlay overlay in _overlaysList)
            {
                overlay.Hide();
            }
            _overlaysList.Clear();
        }

        private void ShowWindowsOverlayHandle(object sender, EventArgs e)
        {
            List<WindowInfo> winList = _win.GetCurrentWindowList();
            WinListView.ItemsSource = winList;

            foreach(WindowInfo win in winList)
            {
                float dpiFactor = _win.GetDpiFactorForSpecificWindow(win.HWnd);
                var overlay = new Overlay()
                {
                    Left = win.Left/dpiFactor,
                    Top = win.Top/dpiFactor,
                    Width = (win.Right - win.Left)/dpiFactor,
                    Height = (win.Bottom - win.Top)/dpiFactor,
                    ResizeMode = ResizeMode.NoResize
                };
                overlay.Show();
                _overlaysList.Add(overlay);
            }
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
            List<WindowInfo> winList = _win.GetCurrentWindowList();
            WinListView.ItemsSource = winList;

            _win.Swap();
             List<WindowInfo> newList = _win.GetCurrentWindowList();
            WinListView.ItemsSource = newList;
            _win.SetCursor();
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
