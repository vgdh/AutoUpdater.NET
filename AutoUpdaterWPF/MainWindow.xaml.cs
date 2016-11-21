using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace AutoUpdaterWPFedition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BrowserWindow : Window
    {
        private string _messageURL;
        public BrowserWindow(string messageURL)
        {
            InitializeComponent();
            _messageURL = messageURL;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.Message = _messageURL;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string strContent = " ";
            try
            {
                var webRequest = WebRequest.Create(AutoUpdater.MessageURL);
                var response = webRequest.GetResponse();
                var content = response.GetResponseStream();
                var reader = new StreamReader(content);
                strContent = reader.ReadToEnd();
            }
            catch (Exception)
            {
                // ignored
            }
            WebBrowser.NavigateToString(strContent);
        }
    }
}
