using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Peeky_Blinkers
{
    /// <summary>
    /// Interaction logic for Overlay.xaml
    /// </summary>
    public partial class Overlay : Window
    {
        private const string _unselectedColour = "#18FF0000";
        private const string _selectedColour = "#1800FF00";
        private readonly SolidColorBrush _selectedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_selectedColour));
        private readonly SolidColorBrush _unselectedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_unselectedColour));
        private WindowInfo _winInfo;
        internal Overlay(WindowInfo info)
        {
            InitializeComponent();
            _winInfo = info;
            BtnSelect.Click += BtnSelectClicked;
        }

        public event EventHandler<OverlayEventArgs> OverlayUpdated;

        public void CloseThis()
        {
            BtnSelect.Click -= BtnSelectClicked;
            _winInfo = null;
            this.Close();
        }

        public void RaisedOverlayUpdated(OverlayEventArgs args)
        {
            OverlayUpdated?.Invoke(this, args);
        }

        internal void ShowUpdate()
        {
            ChangeBtnColour();
            this.Show();
        }

        private void BtnSelectClicked(object sender, RoutedEventArgs e)
        {
            _winInfo.IsSelected = !_winInfo.IsSelected;
            ChangeBtnColour();
            RaisedOverlayUpdated(new OverlayEventArgs(_winInfo.HWnd, _winInfo.IsSelected));
        }

        private void ChangeBtnColour()
        {            
            if (_winInfo.IsSelected)
            {
                BtnSelect.Background = _selectedBrush;
            }
            else
            {
                BtnSelect.Background = _unselectedBrush;
            }
        }

        public class OverlayEventArgs : EventArgs
        {
            public IntPtr HWnd { get; }
            public bool IsSelected { get; }
            public OverlayEventArgs( IntPtr hWnd, bool isSelected)
            {
                HWnd = hWnd;
                IsSelected = isSelected;
            }
        }
    }
}
