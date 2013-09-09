using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms; 

namespace fb_client.net
{
    class Program
    {
        private static MainWindow _guiWindow;
        private static System.Windows.Application _app;
        private static System.Windows.Forms.NotifyIcon _notify;        
        
        [STAThread]
        public static void Main(string[] args)
        {
            _notify = new NotifyIcon();
            _notify.Icon = fb_client.net.Properties.Resources.cloud_icon;
            _notify.Visible = true;

            buildUpNotify();

            _app = new System.Windows.Application();
            _app.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
                        
            OpenGUI();
            
            _app.Run();                        
        }

        private static void OpenGUI()
        {
            if (_guiWindow == null)
            {
                _guiWindow = new MainWindow();                
            }

            _guiWindow.Show();
        }

        private static void buildUpNotify()
        {
            ContextMenuStrip pContextMenu = new ContextMenuStrip();
            ToolStripItem pTMPContextMenuItem = null;

            pTMPContextMenuItem = pContextMenu.Items.Add("Show");
            pTMPContextMenuItem.Click += ShowFBWindow_Click;

            pTMPContextMenuItem = pContextMenu.Items.Add("-");

            pTMPContextMenuItem = pContextMenu.Items.Add("Take Screenshot");
            pTMPContextMenuItem.Click += TakeScreenshot_Click;

            pTMPContextMenuItem = pContextMenu.Items.Add("-");

            pTMPContextMenuItem = pContextMenu.Items.Add("Quit");
            pTMPContextMenuItem.Click += CloseApplication_Click;

            _notify.ContextMenuStrip = pContextMenu;
        }

        private static void closeApplication() {
            _notify.Visible = false;

            if (_app != null) { _app.Shutdown(); }
        }

        private static void CloseApplication_Click(object sender, EventArgs e)
        {
            try
            {
                closeApplication();
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private static void TakeScreenshot_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private static void ShowFBWindow_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }
    }
}
