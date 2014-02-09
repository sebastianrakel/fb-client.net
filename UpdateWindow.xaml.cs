using System;
using System.Collections.Generic;
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

            char[] buffer = new char[1024];
            int bytesRead = 0;
            int bytesToRead = (int)webRes.ContentLength;
            int n = 0;

            using (System.IO.StreamReader sr = new System.IO.StreamReader(webRes.GetResponseStream()))
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(System.IO.Path.GetFileName(new Uri(Program.updateLink).LocalPath), false))
                {   
                    while (bytesToRead > 0)
                    {
                        n = sr.Read(buffer,0, buffer.Length);
                        if (n == 0) { break; }
                        bytesRead += n;
                        bytesToRead -= n;
                        sw.Write(buffer);

                        SetProgressDispatcher(bytesRead, webRes.ContentLength);
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
