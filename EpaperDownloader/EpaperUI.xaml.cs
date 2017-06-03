using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.IO;
using System.ComponentModel;

namespace EpaperDownloader
{
    public partial class EpaperUI : Window
    {
        public WebClient downloader;
        private Epaper epaper;
        // this field holds a copy of DownloadAndFileName List property 
        private List<Dictionary<string, string>> _dlLinkAndFileName;


        public EpaperUI()
        {
            InitializeComponent();

            ePaperDatePicker.SelectedDate = DateTime.Now;

            // select desktop as a default Main directory to download ePapers
            // use System.Environment to get the Desktop for a particular user
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DirectoryInfo dir = new DirectoryInfo(desktopPath);
            downloadDirTextbox.Text = dir.FullName;

            // hide the progress bar and downloadInfoLabel intially
            downloadProgressBar.Visibility = Visibility.Hidden;
            downloadInfoLabel.Visibility = Visibility.Hidden;

            // call EpaperUI_Loaded function everytime the windows Loaded Event is triggered
            this.Loaded += new RoutedEventHandler(EpaperUI_Loaded);
         

        }

        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            // on Browse button Click open file chooser and select a download directory
            // don't do a global using system.windows.Forms because it creates ambigious reference with system.windows
            var folderChooser = new System.Windows.Forms.FolderBrowserDialog();
            var result = folderChooser.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //MessageBox.Show(folderChooser.SelectedPath);
                downloadDirTextbox.Text = folderChooser.SelectedPath;
            }

        }

        // on download button click
        private void ePaperDlButton_Click(object sender, RoutedEventArgs e)
        {
            // on download button click get chosen epaper and the date
            string ePaperName = ePaperChooser.Text;

            // ?? is a Null coalescing operator, if not null use selectedDate if null use currentDate
            DateTime ePaperDate = ePaperDatePicker.SelectedDate ?? DateTime.Now;

            // incase user selects a dir and later deletes the dir, create one for them
            DirectoryInfo dir = new DirectoryInfo(downloadDirTextbox.Text);
            if (!dir.Exists) dir.Create();

            string downloadDir = downloadDirTextbox.Text;

            epaper = new Epaper(ePaperName, ePaperDate, downloadDir);
            // checks for the validity of input date, sets the download Link based on the paper chosen
            epaper.PaperDownloadInfo();

            // clone the ePaper.DownloadLinkAndFileName property List
            // _dlLinkAndFileName will be chnaged but we want to save the original DownloadLinkAndFileName as well
            // hence the copy instead of assignment
            _dlLinkAndFileName = new List<Dictionary<string,string>>(epaper.DownloadLinkAndFileName);

            if (epaper.IsIssueDateValid)
            {
                //System.Threading.Thread thread = new System.Threading.Thread(() =>
                //{
                //    downloader.DownloadFileAsync(new Uri(url), epaper.PathWithFileName);
                //});
                //thread.Start();

                // This application does not allow multi threading, so can't download two papers at the same time
                // download the first link on the List, once this is done if more Links are available it is downloaded
                // in onDownloadComplete Event Handler
                // since webclient doesn't support concurrent I/O operations a loop is not used to download multiple files
                // instead it is downloaded one after another upon download complete
                downloader.DownloadFileAsync(new Uri(_dlLinkAndFileName[0]["dlLink"]), _dlLinkAndFileName[0]["filePathName"]);
                // deactivate the download button to prevent user from attempted download while one download is in progress
                ePaperDlButton.IsEnabled = false;
                
            }
            else
            {
                MessageBox.Show(epaper.InvalidDateMsg);
            }
            
        }

        // called when the UI window loads
        public void EpaperUI_Loaded(object sender, RoutedEventArgs e)
        {
            downloader = new WebClient();
            downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
            downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

        }

        // This function is called when downloadFileCompleted event is triggered by the webclient
        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled) MessageBox.Show("Download Interrupted");
            else if (e.Error == null) // if no error occurs
            {
                // once the first item in the download list is downloaded
                // download the other links in the list
                _dlLinkAndFileName.RemoveAt(0);

                if (_dlLinkAndFileName.Count > 0)
                //{ downloader.DownloadFileAsync(new Uri(epaper.DownloadLink[0]), epaper.PathWithFileName+"-"+epaper.DownloadLink.Count+".jpg"); }
                { downloader.DownloadFileAsync(new Uri(_dlLinkAndFileName[0]["dlLink"]), _dlLinkAndFileName[0]["filePathName"]); }
                else
                {
                    if (epaper.PaperName == "nagarik")
                    {
                        List<string> imagePathAndFileName = new List<string>();
                        //downloadInfoLabel.Content = "Please Wait!! Generating pdf.";
                        foreach (Dictionary<string, string> linkNpath in epaper.DownloadLinkAndFileName)
                        {
                            imagePathAndFileName.Add(linkNpath["filePathName"]);
                        }
                        ImagesToPdf imagesToPdf = new ImagesToPdf(imagePathAndFileName, epaper);
                        imagesToPdf.GeneratePdf();
                    }
                    MessageBox.Show("Download completed. \nCheck " + epaper.DownloadPath + "to view the paper.");
                    // TODO once download is completed, convert images to pdf for those required newspaper
                    // change the label below the progress bar to generating pdf
                    // ReEnable the Download button
                    ePaperDlButton.IsEnabled = true;
                    downloadProgressBar.Value = 0;
                    downloadProgressBar.Visibility = Visibility.Hidden;
                    downloadInfoLabel.Visibility = Visibility.Hidden;
                }
            }
            else // handle errors
            {
                // handle error such as 404 URLs  
                if (e.Error is WebException)
                {
                    WebException we = (WebException)e.Error;
                    HttpWebResponse res = (HttpWebResponse)we.Response;
                    if (res != null && res.StatusCode == HttpStatusCode.NotFound)
                    {
                        // TODO delete the downloaded paper, which has no data
                        MessageBox.Show("404 Error: This issue of " + epaper.PaperName + "is not Available.");
                        ePaperDlButton.IsEnabled = true;
                    }
                }
                else
                {
                    MessageBox.Show(e.Error.ToString()); // Handle any other error such as unspecified file path or file Name and so on.
                    ePaperDlButton.IsEnabled = true;
                }

            }
        }

        // This function is called everytime the downloadProgressChange Event is triggered when downloading
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            downloadProgressBar.Visibility = Visibility.Visible;
            downloadInfoLabel.Visibility = Visibility.Visible;
            downloadProgressBar.Minimum = 0;
            downloadProgressBar.Maximum = 100;
            downloadProgressBar.Value = e.ProgressPercentage;
            downloadInfoLabel.Content ="Downloading: " + e.ProgressPercentage +"%"+"    Files Remaining: " + _dlLinkAndFileName.Count;

            // TODO show download percentage Text Label below the Progress bar..
            // show number of files being downloaded using fileNUm filed, so collecting multiple
            // images user will know. 
            
        }

        private void downloadProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
