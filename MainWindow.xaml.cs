using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace fb_client.net
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _UploadFilePath;
     
        public MainWindow()
        {
            InitializeComponent();

            BitmapFrame bmpFrame;

            MemoryStream iconStream = new MemoryStream();
            
            fb_client.net.Properties.Resources.cloud_icon_png.Save(iconStream, System.Drawing.Imaging.ImageFormat.Png);
            iconStream.Seek(0, SeekOrigin.Begin);
            bmpFrame = BitmapFrame.Create(iconStream);
            

            this.Icon = bmpFrame; 
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SetInfo();

                Program.filebin.UploadFinished += filebin_UploadFinished;
                Program.filebin.UploadProgress += filebin_UploadProgress;
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        void filebin_UploadProgress(object sender, libfbclientnet.UploadProgressEventArgs e)
        {
            SetProgressDispatcher(e.UploadCurrent, e.UploadTotal);
        }

        private void SetProgressDispatcher(double value, double maxValue)
        {
            this.Dispatcher.Invoke(new Action<double[]>(SetProgress), new double[] {
			value,
			maxValue
		});
        }

        private void SetProgress(double[] values)
        {
            int percentValue = 0;

            this.uploadProgressBar.Maximum = 100;

            if (values[1] == 0)
                return;

            percentValue = (int)((values[0] * 100) / values[1]);

            this.uploadProgressBar.Value = percentValue;
            this.labelUploadProgress.Content = values[0] + " / " + values[1];
        }


        void filebin_UploadFinished(object sender, libfbclientnet.UploadFinishedEventArgs e)
        {
            try
            {
                SetClipboardTextDispatcher(e.Result.URL);
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private void SetInfo()
        {
            Run runProgramm = new Run(string.Format("Programm: {0} \n", System.Windows.Forms.Application.ProductName));
            Run runVersion = new Run(string.Format("Version: {0} \n", System.Windows.Forms.Application.ProductVersion.ToString()));
            Run runSource = new Run("Source: ");
            Run runSourceLink = new Run("https://github.com/sebastianrakel/fb-client.net");

            Hyperlink hyperLink = new Hyperlink(runSourceLink);
            hyperLink.NavigateUri = new Uri("https://github.com/sebastianrakel/fb-client.net");

            hyperLink.RequestNavigate += hyperLink_requested;
                        
            this.InfoTextBlock.Inlines.Clear();
            this.InfoTextBlock.Inlines.Add(runProgramm);
            this.InfoTextBlock.Inlines.Add(runVersion);
            this.InfoTextBlock.Inlines.Add(runSource);
            this.InfoTextBlock.Inlines.Add(hyperLink);

            this.Title = string.Format("{0} - {1}", System.Windows.Forms.Application.ProductName, System.Windows.Forms.Application.ProductVersion);
        }

        private void hyperLink_requested(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(e.Uri.ToString());
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        
        private void btnUploadText_Click(object sender, RoutedEventArgs e)
        {
            string filePath = System.IO.Path.GetTempPath();

            if (this.inputTextBox.Text.Length == 0)
            {
                fb_messageBox.ShowBox("no input given");
                return;
            }

            if(!filePath.EndsWith("\\")) { filePath += "\\"; }

            filePath += "stdin";

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, false))
            {
                sw.Write(this.inputTextBox.Text);
            }

            this.uploadFile(filePath);
        }

        private void uploadFile(string filePath)
        {
            this.clipboardLink.Visibility = System.Windows.Visibility.Hidden;
            this.btnClipboardCopy.Visibility = System.Windows.Visibility.Hidden;
            this.uploadProgressBar.Visibility = System.Windows.Visibility.Visible;
            this.labelUploadProgress.Visibility = System.Windows.Visibility.Visible;

            Program.filebin.UploadFileAsync(filePath);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                historyListView.Items.Clear();
                foreach (libfbclientnet.filebin_item historyItem in Program.filebin.GetUploadHistory())
                {
                    historyListView.Items.Add(historyItem);
                }

                labelLastRefresh.Content = "last refresh: " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss tt");
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private void btnSearchUploadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog fileDlg = new Microsoft.Win32.OpenFileDialog();

                
                fileDlg.Multiselect = false;

                if (fileDlg.ShowDialog() == true)
                {
                    SetFileInfo(fileDlg.FileName);
                    this.btnUploadFile.Visibility = System.Windows.Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        public void SetClipboardTextDispatcher(string text)
        {
            this.Dispatcher.BeginInvoke(new Action<string>(SetClipboardText), text);
        }

        private void SetClipboardText(string text)
        {
            this.uploadProgressBar.Visibility = System.Windows.Visibility.Hidden;
            this.labelUploadProgress.Visibility = System.Windows.Visibility.Hidden;
            this.clipboardLink.Visibility = System.Windows.Visibility.Visible;
            this.btnClipboardCopy.Visibility = System.Windows.Visibility.Visible;

            this.clipboardLink.Text = text.Trim();

            this.clipboardLink.Focus();
            this.clipboardLink.SelectAll();
        }
        
        public void SetFileInfoDispatcher(string filePath)
        {
            this.Dispatcher.BeginInvoke(new Action<string>(SetFileInfo), filePath);
        }

        private void SetFileInfo(string filePath)
        {
            _UploadFilePath = filePath;

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);

            System.Text.StringBuilder stringbuild = new System.Text.StringBuilder();
            
            stringbuild.AppendLine("Filename: " + fileInfo.Name);
            stringbuild.AppendLine("Size (bytes): " + fileInfo.Length);

            this.inputDragDrop.Content = stringbuild.ToString();

            this.btnUploadFile.Visibility = System.Windows.Visibility.Visible;
            tpUploadFile.Focus();
        }

        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.uploadFile(_UploadFilePath);
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private void inputDragDrop_Drop(object sender, DragEventArgs e)
        {
            List<string> fileNames = null;

            try
            {
                fileNames = new List<string>((string[])e.Data.GetData(DataFormats.FileDrop));

                switch (fileNames.Count)
                {
                    case 1:
                        SetFileInfo(fileNames[0]);
                        break;
                    default:
                        if (fileNames.Count > 1)
                        {
                            fb_messageBox.ShowBox("at the moment only one file, sorry");
                        }
                        else
                        {
                            fb_messageBox.ShowBox("something went wrong with this file, sorry");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private void btnClipboardCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.clipboardLink.Text.Trim().Length == 0)
                    return;

                Clipboard.SetText(this.clipboardLink.Text);
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private void SetPreview()
        {
            string pMIMEType = null;
            Uri pURI = null;
            libfbclientnet.filebin_item pTMPFileInfo;

            try
            {
                pURI = new System.Uri("about:blank");

                if (this.checkShowPreview.IsChecked  == true && this.historyListView.SelectedItem != null)
                {
                    pTMPFileInfo = (libfbclientnet.filebin_item)historyListView.SelectedItem;
                    pMIMEType = pTMPFileInfo.MIMEType.ToLower();

                    if (pMIMEType.StartsWith("image/"))
                    {
                        pURI = new Uri(pTMPFileInfo.Link);                        
                    } else if (pMIMEType == "text/plain") {
                        pURI = new Uri(pTMPFileInfo.Link);                        
                    }                    
                }

                historyPreview.Source = pURI;
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private void checkShowPreview_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if(this.IsLoaded) {
                    this.historyPreview.Visibility = this.checkShowPreview.IsChecked == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                    this.HistorySplitter.Visibility = this.checkShowPreview.IsChecked == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                    this.HistoryRightColumn.Width = new GridLength(0.4, GridUnitType.Star);
                    SetPreview();
                }
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private void historyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetPreview();
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }
    }
}
