using Seek.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace Seek
{
    /// <summary>
    /// Interaction logic for SeriesDownload.xaml
    /// </summary>
    public partial class SeriesDownload : Window
    {
        List<string> SearchList = new List<string>();
        private bool startImmediately;
        private MainWindow mainWindow;
        private List<string> downloadUrls = new List<string>();
        string searchLink = "";
        bool fetchDone = false;
        Thread t;

        #region Constructor

        public SeriesDownload(MainWindow mainWin)
        {
            InitializeComponent();
            mainWindow = mainWin;
            tbDownloadFolder.Text = Settings.Default.DownloadLocation;
            startImmediately = true;
        }

        #endregion

        #region Methods

        private int countChecked()
        {
            int count = 0;
            foreach (CheckBox item in chkList.Items)
            {
                if (item.IsChecked == true)
                {
                    count++;
                }
            }
            return count;
        }

        private List<string> getSelectedUrls()
        {
            List<string> urls = new List<string>();

            foreach (CheckBox item in chkList.Items)
            {
                if (item.IsChecked == true)
                {
                    urls.Add(item.Content.ToString());
                }
            }
            return urls;
        }

        private void fetchFiles(int index)
        {
            Cursor = System.Windows.Input.Cursors.Wait;
            try
            {
                var listfiles = MyScrapper.Webscraper(SearchList[index]);

                if (listfiles == null)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show($"Did not get any file for the selected link.");
                    Cursor = System.Windows.Input.Cursors.Arrow;
                    return;
                }
                else
                {
                    chkList.Items.Clear();

                    foreach (var item in listfiles)
                    {
                        var chk = new CheckBox()
                        {
                            Content = item,
                            IsChecked = true,
                        };

                        chk.Unchecked += Chk_Unchecked;
                        chk.Checked += Chk_Checked;

                        chkList.Items.Add(chk);
                    }
                }

                lblFilesToDownload.Content = ($"{listfiles.Count} files to download.");

            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("An Error Occured, check your internet connection. \n " + ex.Message);
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        #endregion

        #region Event Handlers

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var img = new Image()
                {
                    Height = btnSearch.Height,
                    Width = btnSearch.Height,
                };

                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("loading.gif", UriKind.Relative);
                image.EndInit();
                ImageBehavior.SetAnimatedSource(img, image);

                btnSearch.Content = img;
                tbDownloadFolder.Text = System.IO.Path.Combine(Settings.Default.DownloadLocation + tbName.Text.Replace(" ", ""));

                listSearch.Items.Clear();
                chkList.Items.Clear();

                searchLink = $"https://www.google.com/search?q=index+of+" + tbName.Text.Trim().Replace(" ", "+");
                fetchDone = false;

                t = new Thread(MyThreadStartMethod);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();

                var tim = new System.Windows.Forms.Timer();
                tim.Interval = 1000;
                tim.Tick += Tim_Tick;

                tim.Enabled = true;

            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("An error occured. \n Please check your internet connection and try again.\n" + ex.Message);
                btnSearch.Content = "Search";
            }
        }

        private void Tim_Tick(object sender, EventArgs e)
        {
            if (fetchDone)
            {
                try
                {
                    var tim = (System.Windows.Forms.Timer)sender;
                    tim.Stop();

                    t.Abort();

                    if (SearchList.Count < 1)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show($"Did not Find valid index site for this movie.\n");
                        btnSearch.Content = "Search";
                    }
                    else
                    {
                        foreach (var item in SearchList)
                        {
                            listSearch.Items.Add(item);
                        }

                        listSearch.SelectedIndex = 0;

                        fetchFiles(0);

                        Xceed.Wpf.Toolkit.MessageBox.Show("DONE! \n\n Click 'Next' or 'Previous' to view files on other servers. Note that some servers are much slower than others.\n\n" +
                            "You can use the 'UNCHECK' button to quickly unselect items that contains a particular text.");
                        btnSearch.Content = "Search";
                    }
                }
                catch (Exception ex)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("An error occured. \n Please check your internet connection and try again.\n" + ex.Message);
                    btnSearch.Content = "Search";
                }
            }
        }

        private void MyThreadStartMethod()
        {
            SearchList = MyScrapper.GetGoogleLinks(searchLink, out fetchDone);
        }

        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            lblFilesToDownload.Content = ($"{countChecked()} files to download.");
        }

        private void Chk_Unchecked(object sender, RoutedEventArgs e)
        {
            lblFilesToDownload.Content = ($"{countChecked()} files to download.");
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbDialog = new System.Windows.Forms.FolderBrowserDialog();
            fbDialog.Description = "Choose Batch Download Folder";
            fbDialog.ShowNewFolderButton = true;
            System.Windows.Forms.DialogResult result = fbDialog.ShowDialog();

            if (result.ToString().Equals("OK"))
            {
                string path = fbDialog.SelectedPath;
                if (path.EndsWith("\\") == false)
                    path += "\\";
                tbDownloadFolder.Text = path;
            }
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            downloadUrls = getSelectedUrls();

            if (downloadUrls.Count > 0)
            {
                try
                {
                    foreach (string url in downloadUrls)
                    {
                        WebDownloadClient download = new WebDownloadClient(url);
                        download.IsBatch = true;
                        download.BatchUrlChecked = false;

                        download.FileName = url.Substring(url.LastIndexOf('/') + 1);

                        // Register WebDownloadClient events
                        download.DownloadProgressChanged += download.DownloadProgressChangedHandler;
                        download.DownloadCompleted += download.DownloadCompletedHandler;
                        download.PropertyChanged += this.mainWindow.PropertyChangedHandler;
                        download.StatusChanged += this.mainWindow.StatusChangedHandler;
                        download.DownloadCompleted += this.mainWindow.DownloadCompletedHandler;

                        // Create path to temporary file
                        if (!Directory.Exists(tbDownloadFolder.Text))
                        {
                            Directory.CreateDirectory(tbDownloadFolder.Text);
                        }
                        string filePath = System.IO.Path.Combine(tbDownloadFolder.Text, download.FileName);
                        string tempPath = filePath + ".tmp";

                        // Check if there is already an ongoing download on that path
                        if (File.Exists(tempPath))
                        {
                            // If there is, skip adding this download
                            continue;
                        }

                        // Check if the file already exists
                        if (File.Exists(filePath))
                        {
                            string message = "There is already a file with the same name, do you want to overwrite it?";
                            MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(message, "File Name Conflict: " + filePath, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                            if (result == MessageBoxResult.Yes)
                            {
                                File.Delete(filePath);
                            }
                            else
                            {
                                // Skip adding this download
                                continue;
                            }
                        }

                        // Set username and password if HTTP authentication is required
                        if (cbLoginToServer.IsChecked.Value && (tbUsername.Text.Trim().Length > 0) && (tbPassword.Password.Trim().Length > 0))
                        {
                            download.ServerLogin = new NetworkCredential(tbUsername.Text.Trim(), tbPassword.Password.Trim());
                        }

                        download.TempDownloadPath = tempPath;

                        download.AddedOn = DateTime.UtcNow;
                        download.CompletedOn = DateTime.MinValue;

                        // Add the download to the downloads list
                        DownloadManager.Instance.DownloadsList.Add(download);

                        // Start downloading the file
                        if (startImmediately)
                            download.Start();
                        else
                            download.Status = DownloadStatus.Paused;
                    }

                    // Close the Create Batch Download window
                    this.Close();
                }
                catch (Exception ex)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                Xceed.Wpf.Toolkit.MessageBox.Show("There are no files to download!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbStartImmediately_Click(object sender, RoutedEventArgs e)
        {
            startImmediately = this.cbStartImmediately.IsChecked.Value;
        }

        private void cbLoginToServer_Click(object sender, RoutedEventArgs e)
        {
            tbUsername.IsEnabled = cbLoginToServer.IsChecked.Value;
            tbPassword.IsEnabled = cbLoginToServer.IsChecked.Value;
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (listSearch.SelectedIndex < SearchList.Count - 1)
            {
                listSearch.SelectedIndex++;
                fetchFiles(listSearch.SelectedIndex);
            }
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (listSearch.SelectedIndex > 0)
            {
                listSearch.SelectedIndex--;
                fetchFiles(listSearch.SelectedIndex);
            }
        }

        private void ListSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listSearch.SelectedIndex > 0 && listSearch.SelectedIndex < SearchList.Count - 1)
            {
                fetchFiles(listSearch.SelectedIndex);
            }
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            if (!string.IsNullOrWhiteSpace(tbFilter.Text))
            {
                foreach (CheckBox item in chkList.Items)
                {
                    var text = item.Content.ToString().ToLower();
                    if (text.Contains(tbFilter.Text.Trim().ToLower()))
                    {
                        item.IsChecked = false;
                    }
                }

                tbFilter.Text = "";
            }

            Cursor = Cursors.Arrow;
        }

        private void BtnToggleCheck_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            if (btnToggleCheck.Content.ToString().ToLower() == "uncheck all")
            {
                foreach (CheckBox item in chkList.Items)
                {
                    item.IsChecked = false;
                }
                btnToggleCheck.Content = "Check All";
            }
            else
            {
                foreach (CheckBox item in chkList.Items)
                {
                    item.IsChecked = true;
                }
                btnToggleCheck.Content = "Uncheck All";
            }

            Cursor = Cursors.Arrow;
        }
        #endregion

    }
}
