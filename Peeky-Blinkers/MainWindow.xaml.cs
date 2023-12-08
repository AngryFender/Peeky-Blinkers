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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Peeky_Blinkers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Win _win = Win.GetInstance();
        private List<WindowInfo> _winList = new List<WindowInfo>();
        
        public MainWindow()
        {
            InitializeComponent();
            _winList = _win.GetEnumWindow();
            _win.FilterWindowTitles();
            WinListView.ItemsSource = _winList;
            WinListView.SelectionChanged += WinItemSelectionChangedHandler;
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
