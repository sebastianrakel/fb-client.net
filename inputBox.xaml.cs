using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace fb_client.net
{
    /// <summary>
    /// Interaction logic for inputBox.xaml
    /// </summary>
    public partial class inputBox : Window
    {
        public string Input {
            get { return this.InputText.Text ; }
            set { this.InputText.Text = value; }
        }

        public inputBox()
        {
            InitializeComponent();

            BitmapFrame bmpFrame;

            MemoryStream iconStream = new MemoryStream();

            fb_client.net.Properties.Resources.cloud_icon_png.Save(iconStream, System.Drawing.Imaging.ImageFormat.Png);
            iconStream.Seek(0, SeekOrigin.Begin);
            bmpFrame = BitmapFrame.Create(iconStream);

            this.Icon = bmpFrame; 
        }        

        public static string ShowInput(string title, string defaultValue)
        {
            inputBox input = new inputBox();
            
            //set pre-defined value
            if (defaultValue != null && defaultValue.Length > 0) { input.Input = defaultValue; }

            //set title
            if (title != null && title.Length > 0)
            {
                input.titleLabel.Content = title;                
            }

            if (input.ShowDialog() == true)
            {
                return input.Input;                   
            } else {
                return "";
            }
            
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Input = "";
            this.DialogResult = false;
            this.Close();
        }
    }
}
