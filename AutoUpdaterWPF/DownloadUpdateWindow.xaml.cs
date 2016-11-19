using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoUpdaterWPFedition
{
    /// <summary>
    /// Interaction logic for DownloadUpdateWindow.xaml
    /// </summary>
    public partial class DownloadUpdateWindow : Window
    {
        private readonly string _downloadURL;
        private string _tempPath;
        private WebClient _webClient;
        private string _pathToFile;

        public DownloadUpdateWindow(string downloadURL, string pathToFile)
        {
            InitializeComponent();
            _downloadURL = downloadURL;
            _pathToFile = pathToFile;
            DownloadUpdateDialogLoad();
        }

        private void DownloadUpdateDialogLoad()
        {
            _webClient = new WebClient();
            var uri = new Uri(_downloadURL);
            _tempPath = System.IO.Path.Combine(_pathToFile, GetFileName(_downloadURL));
            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;
            _webClient.DownloadFileCompleted += OnDownloadComplete;
            _webClient.DownloadFileAsync(uri, _tempPath);
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;

            progressBarText.Text = ProgressBarText(e.BytesReceived, e.TotalBytesToReceive);

        }

        private string ProgressBarText(long bytesReceivedX, long totalBytesToReceiveX)
        {
            return ReceivedBytesToString(bytesReceivedX) + " / " + TotalBytesToString(totalBytesToReceiveX);
        }

        private static string ReceivedBytesToString(double bytesReceivedX)
        {
            if (bytesReceivedX < 1024)
            {
                return string.Format("{0} Байт", bytesReceivedX.ToString("F0"));
            }
            if (bytesReceivedX < 1048576)
            {
                return string.Format("{0} Kб", (bytesReceivedX / 1024).ToString("F2"));
            }
            if (bytesReceivedX < 1073741824)
            {
                return string.Format("{0} Мб", (bytesReceivedX / 1048576).ToString("F2"));
            }

            return string.Format("{0} Гб", (bytesReceivedX / 1073741824).ToString("F2"));
        }

        private string TotalBytesToString(long totalBytesToReceiveX)
        {
            throw new NotImplementedException();
        }

        private void OnDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                var processStartInfo = new ProcessStartInfo { FileName = _tempPath, UseShellExecute = true };
                Process.Start(processStartInfo);

                var currentProcess = Process.GetCurrentProcess();
                foreach (var process in Process.GetProcessesByName(currentProcess.ProcessName))
                {
                    if (process.Id != currentProcess.Id)
                    {
                        process.Kill();
                    }
                }
                Environment.Exit(0);
            }
        }

        private static string GetFileName(string url, string httpWebRequestMethod = "HEAD")
        {
            try
            {
                var fileName = string.Empty;
                var uri = new Uri(url);
                if (uri.Scheme.Equals(Uri.UriSchemeHttp) || uri.Scheme.Equals(Uri.UriSchemeHttps))
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                    httpWebRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                    httpWebRequest.Method = httpWebRequestMethod;
                    httpWebRequest.AllowAutoRedirect = false;
                    var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    if (httpWebResponse.StatusCode.Equals(HttpStatusCode.Redirect) ||
                        httpWebResponse.StatusCode.Equals(HttpStatusCode.Moved) ||
                        httpWebResponse.StatusCode.Equals(HttpStatusCode.MovedPermanently))
                    {
                        if (httpWebResponse.Headers["Location"] != null)
                        {
                            var location = httpWebResponse.Headers["Location"];
                            fileName = GetFileName(location);
                            return fileName;
                        }
                    }
                    var contentDisposition = httpWebResponse.Headers["content-disposition"];
                    if (!string.IsNullOrEmpty(contentDisposition))
                    {
                        const string lookForFileName = "filename=";
                        const string lookForDelName = "; filename*=";
                        var index = contentDisposition.IndexOf(lookForFileName, StringComparison.CurrentCultureIgnoreCase);
                        if (index >= 0)
                            fileName = contentDisposition.Substring(index + lookForFileName.Length);
                        var index2 = fileName.IndexOf(lookForDelName, StringComparison.CurrentCultureIgnoreCase);
                        if (index2 > 0)
                            fileName = fileName.Substring(0, index2);
                        if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                        {
                            fileName = fileName.Substring(1, fileName.Length - 2);
                        }
                    }
                }
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = System.IO.Path.GetFileName(uri.LocalPath);
                }
                return fileName;
            }
            catch (WebException)
            {
                return GetFileName(url, "GET");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _webClient.CancelAsync();
        }
    }
}
