using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace VideoMerge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.FileInfos = new ObservableCollection<FileInfo>();
            this.FileInfosView = CollectionViewSource.GetDefaultView(FileInfos);
            FileInfosView.SortDescriptions.Add(new SortDescription("CreationTime", ListSortDirection.Ascending));
            this.DataContext = this.FileInfosView;
        }


        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;

            if(e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                foreach(string filename in filenames)
                {
                    if(System.IO.Path.GetExtension(filename).ToLowerInvariant() != ".mp4")
                    {
                        dropEnabled = false;
                        break;
                    }
                }
            }
            else
            {
                dropEnabled = false;
            }

            if(!dropEnabled)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        public ObservableCollection<FileInfo> FileInfos
        {
            get;
            set;
        }

        public ICollectionView FileInfosView
        {
            get;
            set;
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            string[] droppedFilenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            foreach(var item in droppedFilenames)
            {
                FileInfos.Add(new FileInfo(item));
            }

            //  Need to look at this to add items:
            //  http://msdn.microsoft.com/en-us/library/ms748365.aspx
        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if(!FileInfos.Any())
            {
                MessageBox.Show("Please drag and drop some video files to be merged");
                return;
            }

            if(txtOutputFilename.Text.Trim().Length > 0)
            {
                //  If the output directory doesn't exist, create it:
                if(!Directory.Exists(Path.GetDirectoryName(txtOutputFilename.Text)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(txtOutputFilename.Text));
                }

                //  Put all the paths into a text document
                List<FileInfo> allFiles = GetFileInformationList().ToList();
                string allFilesData = string.Join(Environment.NewLine, (from item in allFiles
                                                         select "file '" + item.FullName + "'").ToArray());
                
                string tempFilename = System.IO.Path.GetTempFileName();
                File.WriteAllText(tempFilename, allFilesData);

                //  Shell out to ffmpeg (using timeout)
                ProcessStartInfo ffmpegPInfo = new ProcessStartInfo();

                ffmpegPInfo.Arguments = string.Format("-f concat -i \"{0}\" -c copy \"{1}\"",
                    tempFilename,
                    txtOutputFilename.Text);

                ffmpegPInfo.FileName = Path.Combine(currentPath, "ffmpeg.exe");

                Process ffmpegProcess = Process.Start(ffmpegPInfo);
                ffmpegProcess.WaitForExit(1800000); // Wait 30 minutes

                //  If it hasn't exited, but it's not responding...
                //  kill the process
                if(!ffmpegProcess.HasExited && !ffmpegProcess.Responding)
                {
                    ffmpegProcess.Kill();
                    MessageBox.Show("ffmpeg timed out or died.  You might want to retry that last operation");
                }
                else
                {
                    //  Reset the list
                    FileInfos.Clear();
                }
            }
            else
            {
                //  Display an error:
                MessageBox.Show("Please enter new merged filename");
                return;
            }

        }

        private void btnClearList_Click(object sender, RoutedEventArgs e)
        {
            FileInfos.Clear();
        }

        private void btnSortByCreateDate_Click(object sender, RoutedEventArgs e)
        {
            FileInfosView.SortDescriptions.Clear();
            FileInfosView.SortDescriptions.Add(new SortDescription("CreationTime", ListSortDirection.Ascending));
        }

        private void btnSortAlpha_Click(object sender, RoutedEventArgs e)
        {
            FileInfosView.SortDescriptions.Clear();
            FileInfosView.SortDescriptions.Add(new SortDescription("FullName", ListSortDirection.Ascending));
        }

        public IEnumerable<FileInfo> GetFileInformationList()
        {
            foreach(var item in this.FileInfosView)
            {
                yield return (FileInfo)item;
            }
        }

    }
}
