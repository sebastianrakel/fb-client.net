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

        public static libfbclientnet.filebin filebin;

        private static bool _SystrayMode;
        
        [STAThread]
        public static void Main(string[] args)
        {
            _notify = new NotifyIcon();
            _notify.Icon = fb_client.net.Properties.Resources.cloud_icon;
            _notify.Visible = true;
            _notify.DoubleClick += _notify_DoubleClick;
                        
            buildUpNotify();

            GlobalFunctions.CheckForAPIKey();

            filebin = new libfbclientnet.filebin("https://paste.xinu.at", "fb-client.net");
            filebin.APIKey = fb_client.net.Properties.Settings.Default.apikey;
                        
            _app = new System.Windows.Application();
            _app.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
                        
            OpenGUI();
            
            _app.Run();                        
        }

        private static void OpenGUI()
        {
            if (_guiWindow == null || !_guiWindow.IsLoaded)
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

        private static void _notify_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                OpenGUI();
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
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
                OpenGUI();
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        private static List<string> ReadParameter()
        {
            List<string> retList = new List<string>();

            for (int i = 1; i <= Environment.GetCommandLineArgs().Length - 1; i++)
            {
                if (i > 0)
                {
                    if (System.IO.File.Exists(Environment.GetCommandLineArgs()[i]))
                    {
                        retList.Add(Environment.GetCommandLineArgs()[i]);
                    }
                    else
                    {
                        switch (Environment.GetCommandLineArgs()[i])
                        {
                            case "-systray":
                                _SystrayMode = true;
                                break;
                        }
                    }
                }
            }

            return retList;
        }
    }
}
