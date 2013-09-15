using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public MainWindow()
        {
            InitializeComponent();
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

            Program.filebin.UploadFileAsync(filePath);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {

        }        
    }
}
