using System;
using System.Collections.Generic;
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
    /// Interaction logic for fb_messageBox.xaml
    /// </summary>
    public partial class fb_messageBox : Window
    {
        public static bool ShowBox(string text, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            bool? result;

            fb_messageBox msgBox = new fb_messageBox();

            msgBox.SetText(text);
            msgBox.SetButtons(buttons);

            result = msgBox.ShowDialog();

            if (result == null || result == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void ShowBox(Exception ex)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            fb_messageBox msgBox = new fb_messageBox();

            stringBuilder.AppendLine(string.Format("Message ---{0}", ex.Message));
            stringBuilder.AppendLine(string.Format("HelpLink ---{0}", ex.HelpLink));
            stringBuilder.AppendLine(string.Format("Source ---{0}", ex.Source));
            stringBuilder.AppendLine(string.Format("StackTrace ---{0}", ex.StackTrace));
            stringBuilder.AppendLine(string.Format("TargetSite ---{0}", ex.TargetSite));

            msgBox.SetText(stringBuilder.ToString());
            msgBox.btnCopyToClipboard.Visibility = System.Windows.Visibility.Visible;
            msgBox.ShowDialog();

            System.Windows.Threading.Dispatcher.Run();
        }


        public fb_messageBox()
        {
            InitializeComponent();
        }

        public void SetText(string text)
        {
            this.MessageTextBox.Text = text;
        }

        public void SetButtons(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.YesNo:
                    btnOk.Visibility = System.Windows.Visibility.Hidden;
                    btnYes.Visibility = System.Windows.Visibility.Visible;
                    btnNo.Visibility = System.Windows.Visibility.Visible;
                    break;
                case MessageBoxButton.OK:
                    btnOk.Visibility = System.Windows.Visibility.Visible;
                    btnYes.Visibility = System.Windows.Visibility.Hidden;
                    btnNo.Visibility = System.Windows.Visibility.Hidden;
                    break;
            }
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
             Clipboard.SetText(MessageTextBox.Text.Trim());
        }
    }
}
