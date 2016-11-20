using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace AutoUpdaterWPFedition
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        internal class DayReminder
        {
            public int NumberOfDays { get; set; }
            public string Name { get; set; }
        }

        public UpdateWindow()
        {
            InitializeComponent();

            //TODO сделать чтобы при отсутствии интернета все не падало
            if (AutoUpdater.InstalledVersion == null || AutoUpdater.CurrentVersion == null)
            {
                MessageBox.Show("Сервер основлений недоступен");
                return;
            }

            VersionInstalled.Text = AutoUpdater.InstalledVersion.ToString();
            VersionAvailable.Text = AutoUpdater.CurrentVersion.ToString();


            string strContent = " ";
            try
            {
                var webRequest = WebRequest.Create(AutoUpdater.ChangeLogURL);
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
            List<DayReminder> days = new List<DayReminder>()
            {
                new DayReminder() {NumberOfDays=1,Name="Завтра" },
                new DayReminder() {NumberOfDays=2,Name="Послезавтра" },
                new DayReminder() {NumberOfDays=7,Name="На следующей неделе" },
                new DayReminder() {NumberOfDays=30,Name="В следующем месяце" },
            };
            RemindLaterDays.ItemsSource = days;
            RemindLaterDays.DisplayMemberPath = "Name";
            RemindLaterDays.SelectedIndex = 0;

            if (AutoUpdater.CurrentVersion <= AutoUpdater.InstalledVersion)
            {
                UpdateButton.IsEnabled = false;
                SkipVersionButton.IsEnabled = false;
                ReminderLaterButton.IsEnabled = false;
                RemindLaterDays.IsEnabled = false;
                HeaderNewVersion.Text = "Обновлений нет";
            }
        }




        private void Skip_Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.SkipVersion = AutoUpdater.CurrentVersion;
            this.Close();
        }

        private void Later_Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.RemindLater = DateTime.Now + TimeSpan.FromDays((RemindLaterDays.SelectedItem as DayReminder).NumberOfDays);
            this.Close();
        }

        private void DownloadUpdate_Button_Click(object sender, RoutedEventArgs e)
        {
            AutoUpdater.DownloadUpdate();

            //Для запуска стороннего процесса
            //var processStartInfo = new ProcessStartInfo(AutoUpdater.DownloadURL);
            //Process.Start(processStartInfo);
        }
    }
}
