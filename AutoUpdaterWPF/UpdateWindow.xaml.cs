using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (AutoUpdater.InstalledVersion == null || AutoUpdater.CurrentVersion == null)
            {
                MessageBox.Show("Сервер основлений недоступен","Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
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
    }
}
