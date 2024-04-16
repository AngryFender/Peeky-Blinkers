using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using System;
using System.ComponentModel;
using ContextMenu = System.Windows.Forms.ContextMenu;
using static Peeky_Blinkers.Overlay;
using System.Threading;
using System.Collections.ObjectModel;

namespace Peeky_Blinkers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        private readonly WindowManager _winMan = new WindowManager(new Win());
        private readonly ConfigManager _configManager = new ConfigManager(new WindowRegistry());
        private bool _exitRequested = false;
        private bool _initialNotification = true;
        private readonly Mutex _mutex;
        private readonly List<WeakReference<Overlay>> _overlaysList = new List<WeakReference<Overlay>>();

        private ObservableCollection<WindowInfo> _windowInfos = new ObservableCollection<WindowInfo>();

        public MainWindow()
        {
            bool isNewApp = true;
            _mutex = new Mutex(true, "Peeky Blinkers", out isNewApp);

            if (!isNewApp)
            {
                ErrorDialog dialog = new ErrorDialog();
                dialog.ShowDialog();
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
                _winMan.ShowWindowsOverlay += ShowWindowsOverlayHandle;
                _winMan.HideWindowOverlay += HideWindowsOverlayHandle;

                CheckBox_animation.IsChecked = _configManager.GetAnimationState();
                Slider_animation.Value = _configManager.GetAnimationFrameCount();
                CheckAnimationState();
                CheckBox_animation.Checked += CheckBox_animation_handler;
                CheckBox_animation.Unchecked += CheckBox_animation_handler;
                Slider_animation.ValueChanged += Slider_animation_ValueChanged;


                CheckBox_minimized.Checked += CheckBox_minimized_handler;
                CheckBox_minimized.Unchecked += CheckBox_minimized_handler;
            }
        }

        private void CheckBox_minimized_handler(object sender, RoutedEventArgs e)
        {
            _winMan.setMinimizedState(CheckBox_minimized.IsChecked.GetValueOrDefault());
        }

        private void Slider_animation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _configManager.setAnimationFrameCount((int)e.NewValue);
            CheckAnimationState();
        }

        private void CheckBox_animation_handler(object sender, RoutedEventArgs e)
        {
            _configManager.SetAnimationState(CheckBox_animation.IsChecked.GetValueOrDefault());
            CheckAnimationState();
        }

        private void HideWindowsOverlayHandle(object sender, EventArgs e)
        {
            foreach(WeakReference<Overlay> weakRef in _overlaysList)
            {
                List<WeakReference<Overlay>> list = new List<WeakReference<Overlay>>();
                Overlay overlay;
                if (weakRef.TryGetTarget(out overlay))
                {
                    overlay.Hide();
                    overlay.OverlayUpdated -= OverlayUpdatedHandler;
                    overlay.CloseThis();
                }
            }
            _overlaysList.Clear();
            GC.Collect();
        }

        private void ShowWindowsOverlayHandle(object sender, EventArgs e)
        {
            foreach(WeakReference<Overlay> weakRef in _overlaysList)
            {
                List<WeakReference<Overlay>> list = new List<WeakReference<Overlay>>();
                Overlay overlay;
                if (weakRef.TryGetTarget(out overlay))
                {
                    overlay.Hide();
                    overlay.OverlayUpdated -= OverlayUpdatedHandler;
                    overlay.CloseThis();
                }
            }
            _overlaysList.Clear();

            List<WindowInfo> winList = _winMan.GetCurrentWindowList();
            _windowInfos.Clear();
            foreach(WindowInfo win in winList)
            {
                _windowInfos.Add(win);
            }
            WinListView.ItemsSource = _windowInfos;

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

                WeakReference<Overlay> weakRef = new WeakReference<Overlay>(overlay);
                _overlaysList.Add(weakRef);
            }
        }

        private void OverlayUpdatedHandler(object sender, OverlayEventArgs e)
        {
            List<WindowInfo> winList = _winMan.GetCurrentWindowList();
            _windowInfos.Clear();
            foreach(WindowInfo win in winList)
            {
                if(win.HWnd == e.HWnd)
                {
                    win.IsSelected = e.IsSelected;
                }
                _windowInfos.Add(win);
            }


            WinListView.ItemsSource =_windowInfos;
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
            _winMan.Dispose();
            _configManager.Dispose();
            Application.Current.Shutdown();
        }

        private void WindowAddRemoveHandle(object sender, WindowInfoArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var winlist  = e.GetList();
                _windowInfos.Clear();
                foreach(WindowInfo win in winlist)
                {
                    _windowInfos.Add(win);
                }
                WinListView.ItemsSource = _windowInfos;
                WinListView.Items.Refresh();
            });
        }

        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            if (!_exitRequested)
            {
                e.Cancel = true;
                HideWindow();
            }
        }

        private void CheckAnimationState()
        {
            _winMan.setAnimationState(CheckBox_animation.IsChecked.GetValueOrDefault());
            _winMan.setAnimationFrameCount((int)Slider_animation.Value);
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
