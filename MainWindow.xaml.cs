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
                this.Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(fb_client.net.Properties.Resources.cloud_icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                SetInfo();
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private void SetInfo()
        {
            Run runProgramm = new Run(string.Format("Programm: {0}", System.Windows.Forms.Application.ProductName));
            Run runVersion = new Run(string.Format("Version: {0}.{1}", System.Windows.Forms.Application.ProductVersion.ToString()));
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


    }
}
