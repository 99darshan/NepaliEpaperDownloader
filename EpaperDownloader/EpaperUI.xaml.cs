using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.IO;
 

namespace EpaperDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EpaperUI : Window
    {
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


        }
    }
}
