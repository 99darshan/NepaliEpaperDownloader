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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EpaperUI : Window
    {
        public WebClient downloader;

        public EpaperUI()
        {
            InitializeComponent();

            ePaperDatePicker.SelectedDate = DateTime.Now;

            // select desktop as a default Main directory to download ePapers
            // use System.Environment to get the Desktop for a particular user
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DirectoryInfo dir = new DirectoryInfo(desktopPath);
            downloadDirTextbox.Text = dir.FullName;

            // hide the progress bar intially
            downloadProgressBar.Visibility = Visibility.Hidden;

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

            Epaper epaper = new Epaper(ePaperName, ePaperDate, downloadDir);
            // checks for the validity of input date, sets the download Link based on the paper chosen
            epaper.PaperDownloadInfo();

            if (epaper.IsIssueDateValid)
            {
                foreach (string url in epaper.DownloadLink)
                {
                    //System.Threading.Thread thread = new System.Threading.Thread(() =>
                    //{
                    //    downloader.DownloadFileAsync(new Uri(url), epaper.PathWithFileName);
                    //});
                    //thread.Start();

                    // This application does not allow multi threading, so can't download two papers at the same time

                    // TODO check for invalid links 
                    downloader.DownloadFileAsync(new Uri(url), epaper.PathWithFileName);
                    // deactivate the download button to prevent user from attempted download while one download is in progress
                    ePaperDlButton.IsEnabled = false;
                    

                }
            }
            else
            {
                MessageBox.Show(epaper.InvalidDateMsg);
            }
            
        }


        public void EpaperUI_Loaded(object sender, RoutedEventArgs e)
        {
            downloader = new WebClient();
            downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
            downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled) MessageBox.Show("Download Interrupted");
            else
            {
                MessageBox.Show("Download complete");
                // TODO once download is completed, convert images to pdf for those required newspaper
                // change the label below the progress bar to generating pdf
                // ReEnable the Download button
                ePaperDlButton.IsEnabled = true;
                downloadProgressBar.Value = 0;
                downloadProgressBar.Visibility = Visibility.Hidden;
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            downloadProgressBar.Visibility = Visibility.Visible;
            downloadProgressBar.Minimum = 0;
            downloadProgressBar.Maximum = 100;
            downloadProgressBar.Value = e.ProgressPercentage;

            // TODO show download percentage Text Label below the Progress bar..
            // show number of files being downloaded using fileNUm filed, so collecting multiple
            // images user will know. 
            
        }

    }
}
