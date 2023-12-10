using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Drawing;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System;
using System.ComponentModel;

namespace Peeky_Blinkers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        private readonly Win _win = Win.GetInstance();
        
        public MainWindow()
        {
            InitializeComponent();

            this.notifyIcon = new NotifyIcon
            {
                BalloonTipText = "Peeky-Blinkers is minimized to tray",
                BalloonTipTitle = "Peeky-Blinkers",
                Text = "Peeky-Blinkers",
                Icon = Properties.Resources.swap,
                Visible = true
            };
            notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            this.Closing += MainWindowClosing;

            List<WindowInfo> winList = _win.GetCurrentWindowList();
            WinListView.ItemsSource = winList;
            WinListView.SelectionChanged += WinItemSelectionChangedHandler;
            BtnRefresh.Click += BtnRefresh_Click;
            BtnSwap.Click += BtnSwap_Click;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            List<WindowInfo> winList = _win.GetCurrentWindowList();
            WinListView.ItemsSource = winList;
        }

        private void BtnSwap_Click(object sender, RoutedEventArgs e)
        {
            _win.Swap();
             List<WindowInfo> newList = _win.GetCurrentWindowList();
            WinListView.ItemsSource = newList;
       }

        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            HideWindow();
        }

        private void ShowMainWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void HideWindow()
        {
            this.Hide();
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(1);
        }

        private void WinItemSelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            if (!(WinListView.SelectedValue is WindowInfo info))
            {
                return;
            }
            _win.SelectWindow(info.HWnd);
        }
    }
}
