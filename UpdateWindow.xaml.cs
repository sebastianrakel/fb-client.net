using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace fb_client.net
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
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
                this.BtnUpdate.Visibility = System.Windows.Visibility.Visible;
                this.UpdateProgress.Visibility = System.Windows.Visibility.Hidden;
                SetReleaseNotes();
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private void SetReleaseNotes() {
            ReleaseNotesView.Navigate(new Uri(Program.releaseNotesLinks));            
        }

        private void UpdateApplication()
        {
            Thread pUpdateThread = new System.Threading.Thread(new ThreadStart(UpdateApplicationThread));

            pUpdateThread.SetApartmentState(ApartmentState.STA);
            pUpdateThread.Start();

          
        }

        private void UpdateApplicationThread()
        {
            System.Net.WebRequest webReq = System.Net.WebRequest.Create(Program.updateLink);
            System.Net.WebResponse webRes = (System.Net.HttpWebResponse)webReq.GetResponse();
                       
            using (System.IO.FileStream pFileStream = new System.IO.FileStream(System.IO.Path.GetFileName(new Uri(Program.updateLink).LocalPath), FileMode.Create))
            {
                using (BinaryReader pBinaryReader = new BinaryReader(webRes.GetResponseStream()))
                {
                    using (BinaryWriter pBinaryWriter = new BinaryWriter(pFileStream))
                    {
                        byte[] pBuffer = new byte[1025];
                        int pBytesRead = 0;
                        int pBytesDownloadProgress = 0;
                        
                        do {
	                        pBytesRead = pBinaryReader.Read(pBuffer, 0, 1024);
	                        pBytesDownloadProgress += pBytesRead;
    	
	                        pBinaryWriter.Write(pBuffer, 0, pBytesRead);
                            SetProgressDispatcher(pBytesRead, webRes.ContentLength);
                        } while (!(pBytesRead == 0));

                        // alle Dateien schließen
                        pBinaryWriter.Close();
                        pBinaryReader.Close();
                        pFileStream.Close();
                    }
                }
            }           
            CloseWindowDispatcher();
        }

        private void CloseWindowDispatcher()
        {
            this.Dispatcher.Invoke(new Action(CloseWindow), null);
        }

        private void CloseWindow()
        {
            this.DialogResult = true;
            this.Close();
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

            this.UpdateProgress.Maximum = 100;

            if (values[1] == 0)
                return;

            percentValue = (int)((values[0] * 100) / values[1]);

            this.UpdateProgress.Value = percentValue;            
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            this.BtnUpdate.Visibility = System.Windows.Visibility.Hidden;
            this.UpdateProgress.Visibility = System.Windows.Visibility.Visible;
            UpdateApplication();
        }
    }
}
