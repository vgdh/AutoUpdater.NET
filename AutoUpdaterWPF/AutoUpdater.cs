using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using System.Xml;

namespace AutoUpdaterWPFedition
{

    public enum UpdateType
    {
        CheckOnly,
        UpdateWindow
    }
    public static class AutoUpdater
    {
        internal static string ChangeLogURL;
        internal static string DownloadURL;
        internal static string MessageURL;
        internal static Version CurrentVersion;
        internal static Version InstalledVersion;
        /// <summary>
        /// Адрес местоположения файла с информацией об обновлении
        /// Пример: https://dl.dropboxusercontent.com/u/649259497/Update.xml
        /// </summary>
        internal static string XmlUpdateFileURL;
        internal static Timer Timer;

        public delegate void UpdateCheckEventHandler(UpdateInfoEventArgs args);
        /// <summary>
        /// Вызывается при каждой проверке обновления, в аргументах содержит всю дополнительную информацию по обновлению. 
        /// </summary>
        public static event UpdateCheckEventHandler UpdateCheckEvent;

        public delegate void UpdateActionOverrideEventHandler(UpdateInfoEventArgs args);
        /// <summary>
        /// При использоывании заменяет внутренние механизмы поведения при обнаружении обновления на пользовательские.
        /// </summary>
        public static event UpdateActionOverrideEventHandler UpdateActionOverrideEvent;

        /// <summary>
        /// Инициализация атоапдейтера
        /// </summary>
        /// <param name="xmlUpdateFileUrlX">Ссылка на XML документ</param>
        /// <param name="timerLoopTimeX">Время проверки обновления в секундах (0 - если не нужно проверять по таймеру)</param>
        /// <param name="updateTypeX">Тип автоматической проверки обновлений</param>
        public static void Initialize(string xmlUpdateFileUrlX, int timerLoopTimeX = 0, UpdateType updateTypeX = UpdateType.UpdateWindow)
        {
            XmlUpdateFileURL = xmlUpdateFileUrlX;
            if (timerLoopTimeX != 0)
            {
                TimerCallback timeCB = new TimerCallback(delegate { CheckUpdate(updateTypeX); });
                Timer = new Timer(timeCB, null, 0, timerLoopTimeX * 1000);
            }

        }

        /// <summary>
        /// Проверка обновления
        /// </summary>
        public static void CheckUpdate(UpdateType updateTypeX = UpdateType.UpdateWindow)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorkerDoWork;
            backgroundWorker.RunWorkerAsync(updateTypeX);
        }

        /// <summary>
        /// Реализует проверку обновления
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            UpdateType updateType = (UpdateType)e.Argument;
            Assembly mainAssembly = Assembly.GetEntryAssembly();
            InstalledVersion = mainAssembly.GetName().Version;

            WebRequest webRequest = WebRequest.Create(XmlUpdateFileURL);
            webRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            WebResponse webResponse;
            try
            {
                webResponse = webRequest.GetResponse();
            }
            catch (Exception)
            {
                UpdateActionOverrideEvent?.Invoke(null);
                return;
            }

            Stream stream = webResponse.GetResponseStream();
            XmlDocument receivedXmlDocument = new XmlDocument();

            if (stream != null)
            {
                receivedXmlDocument.Load(stream);
            }
            else
            {
                UpdateActionOverrideEvent?.Invoke(null);
                return;
            }

            XmlNodeList xmlNodeList = receivedXmlDocument.SelectNodes("item");

            if (xmlNodeList != null)
            {
                XmlNode xmlNodeVersion = xmlNodeList[0].SelectSingleNode("version");
                if (xmlNodeVersion != null) CurrentVersion = new Version(xmlNodeVersion.InnerText);

                XmlNode xmlNodeChangeLog = xmlNodeList[0].SelectSingleNode("changelog");
                ChangeLogURL = GetURL(webResponse.ResponseUri, xmlNodeChangeLog);

                XmlNode xmlNodeMessage = xmlNodeList[0].SelectSingleNode("message");
                MessageURL = GetURL(webResponse.ResponseUri, xmlNodeMessage);


                XmlNode xmlNodeUrl = xmlNodeList[0].SelectSingleNode("URLx86");
                DownloadURL = GetURL(webResponse.ResponseUri, xmlNodeUrl);

                if (IntPtr.Size.Equals(8))
                {
                    XmlNode appCastUrl64 = xmlNodeList[0].SelectSingleNode("URLx64");
                    var downloadURL64 = GetURL(webResponse.ResponseUri, appCastUrl64);
                    if (!string.IsNullOrEmpty(downloadURL64))
                    {
                        DownloadURL = downloadURL64;
                    }
                }
            }

            var args = new UpdateInfoEventArgs
            {
                DownloadURL = DownloadURL,
                ChangelogURL = ChangeLogURL,
                CurrentVersion = CurrentVersion,
                InstalledVersion = InstalledVersion,
                IsUpdateAvailable = false,
            };

            if (CurrentVersion > InstalledVersion)
            {
                args.IsUpdateAvailable = true;

                // Показывает прошло ли время с момента нажатия кнопки "Напомнить позже"
                if (DateTime.Compare(DateTime.Now, Settings.RemindLater) > 0 & CurrentVersion.ToString(3) != Settings.SkipVersion.ToString(3))
                {
                    if (UpdateActionOverrideEvent == null & updateType == UpdateType.UpdateWindow)
                    {
                        var thread = new Thread(ShowUpdateWindow);
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                    }
                    UpdateActionOverrideEvent?.Invoke(args);
                }
            }
            UpdateCheckEvent?.Invoke(args);

            if (Settings.Message != MessageURL)
            {
                var thread = new Thread(ShowMessageWindow);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        private static void ShowMessageWindow()
        {
            BrowserWindow browserWindow = new BrowserWindow(MessageURL);
            browserWindow.ShowDialog();
        }

        private static string GetURL(Uri baseUri, XmlNode xmlNode)
        {
            var tempURL = xmlNode != null ? xmlNode.InnerText : "";
            if (!string.IsNullOrEmpty(tempURL) && Uri.IsWellFormedUriString(tempURL, UriKind.Relative))
            {
                Uri uri = new Uri(baseUri, tempURL);

                if (uri.IsAbsoluteUri)
                {
                    tempURL = uri.AbsoluteUri;
                }
            }
            return tempURL;
        }

        /// <summary>
        /// Запуск окна с развернутой информацией об обновлении (перед запуском автоматически проверит обновление)
        /// </summary>
        public static void ShowUpdateWindow()
        {
            if (XmlUpdateFileURL == null) return; //чтобы не проверять по пустому адресу
            BackgroundWorkerDoWork(null, new DoWorkEventArgs(UpdateType.CheckOnly));

            UpdateWindow updateWindow = new UpdateWindow();
            updateWindow.ShowDialog();

        }

        /// <summary>
        /// Скачивание и запуск обновления
        /// </summary>
        public static void DownloadUpdate()
        {
            if (XmlUpdateFileURL == null) return; //чтобы не проверять по пустому адресу
            BackgroundWorkerDoWork(null, new DoWorkEventArgs(UpdateType.CheckOnly));
            if (CurrentVersion > InstalledVersion)
            {
                DownloadUpdateWindow downloadUpdateWindow = new DownloadUpdateWindow(DownloadURL, Directory.GetCurrentDirectory());
                downloadUpdateWindow.ShowDialog();
            }
        }
    }



    /// <summary>
    ///     Object of this class gives you all the details about the update useful in handling the update logic yourself.
    /// </summary>
    public class UpdateInfoEventArgs : EventArgs
    {
        /// <summary>
        ///     If new update is available then returns true otherwise false.
        /// </summary>
        public bool IsUpdateAvailable { get; set; }

        /// <summary>
        ///     Download URL of the update file.
        /// </summary>
        public string DownloadURL { get; set; }

        /// <summary>
        ///     URL of the webpage specifying changes in the new update.
        /// </summary>
        public string ChangelogURL { get; set; }

        /// <summary>
        ///     Returns newest version of the application available to download.
        /// </summary>
        public Version CurrentVersion { get; set; }

        /// <summary>
        ///     Returns version of the application currently installed on the user's PC.
        /// </summary>
        public Version InstalledVersion { get; set; }
    }
}