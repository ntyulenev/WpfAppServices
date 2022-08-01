using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace WpfAppServices {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();

            App.Current.DispatcherUnhandledException += (s, e) =>
            {
                Trace.TraceError($"UNHANDLED EXCEPTION: [{e.Exception.GetType()}]: {e.Exception.Message}\r\n{e.Exception.StackTrace}");
                MessageBox.Show("Application has been crashed. Something gone unexpectedly bad.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //throw new Exception("Now it should crash!", e.Exception);
            };
        }

        private void ErrorCloseBtn_Click(object sender, RoutedEventArgs e) {
            ExecutionErrorGrid.Visibility = Visibility.Collapsed;
        }
    }
}
