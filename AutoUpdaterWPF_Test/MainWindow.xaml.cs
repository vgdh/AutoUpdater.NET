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
using System.Windows.Threading;
using AutoUpdaterWPFedition;


namespace AutoUpdaterWPF_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AutoUpdater.Initialize("https://dl.dropboxusercontent.com/u/23825089/Update.xml", 30, UpdateType.CheckOnly);
            AutoUpdater.UpdateCheckEvent += AutoUpdaterOnUpdateAvailableNotify;
        }

        private void AutoUpdaterOnUpdateAvailableNotify(UpdateInfoEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (args.IsUpdateAvailable)
                {
                    Button1.Background = Brushes.Green;
                }
                else
                {
                    Button1.Background = (new Button()).Background;
                }
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //AutoUpdater.CheckUpdate(UpdateType.UpdateWindow);
            //AutoUpdater.DownloadUpdate();

            AutoUpdater.ShowUpdateWindow();

        }

    }
}
