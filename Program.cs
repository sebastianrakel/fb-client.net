using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Collections;

namespace fb_client.net
{
    class Program
    {
        private static MainWindow _guiWindow;
        private static System.Windows.Application _app;
        private static System.Windows.Forms.NotifyIcon _notify;

        private static string _appIPCName = Application.ExecutablePath;
        private static string _appIPCPort = "SingleInstance";
        private static string _appIPCURI = _appIPCName + "_" + _appIPCPort;
        private static IpcChannel _appIPCChannel;

        public static string updateLink = @"http://sebastianrakel.github.io/fb-client.net/files/fb-client.net.zip";
        public static string releaseNotesLinks = @"http://sebastianrakel.github.io/fb-client.net/releasenotes.txt";


        public static libfbclientnet.filebin filebin;
                
        private static bool _SystrayMode;

        private static IpcRemoteObject _ipcRemoteObj;
        
        [STAThread]
        public static void Main(string[] args)
        {
            _notify = new NotifyIcon();
            _notify.Icon = fb_client.net.Properties.Resources.cloud_icon;
            _notify.Visible = true;
            _notify.DoubleClick += _notify_DoubleClick;
                        
            List<string> filelist = ReadParameter();
                       
            if (!registerIPC())
            {
                if (filelist.Count > 0)
                {
                    sendIPC(filelist[0]);
                }
                else
                {
                    sendIPC("");
                }

                _notify.Visible = false;
                return;
            }

            if (checkForUpdate())
            {
                doUpdate();
            }

            SetShellExtension();

            buildUpNotify();

            GlobalFunctions.CheckForAPIKey();

            filebin = new libfbclientnet.filebin(fb_client.net.Properties.Settings.Default.fb_host, "fb-client.net");
            filebin.APIKey = fb_client.net.Properties.Settings.Default.apikey;
                        
            _app = new System.Windows.Application();
            _app.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
                       

            if (!_SystrayMode) { OpenGUI(); }
            
            if (filelist.Count > 0)
            {
                _guiWindow.SetFileInfoDispatcher(filelist[0]);
            }

            filebin.UploadFinished += filebin_UploadFinished;
            
            _app.Run();                        
        }

        private static bool registerIPC()
        {
            IChannel[] myIChannelArray = ChannelServices.RegisteredChannels;
            for (int i = 0; i < myIChannelArray.Length; i++)
            {
                Debug.WriteLine("Name of Channel: {0}", myIChannelArray[i].ChannelName);
                Debug.WriteLine("Priority of Channel: {0}", myIChannelArray[i].ChannelPriority.ToString());
            }

            if (ChannelServices.GetChannel("fbclient") != null)
            {
                return false;
            }

            try
            {
                Hashtable pProps = new Hashtable();

                pProps.Add("name", _appIPCName);
                pProps.Add("portName", _appIPCName);

                _appIPCChannel = new IpcChannel(pProps, null, null);
                ChannelServices.RegisterChannel(_appIPCChannel, false);

                RemotingConfiguration.RegisterWellKnownServiceType(typeof(IpcRemoteObject), _appIPCURI, WellKnownObjectMode.Singleton);
                                
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static void sendIPC(string path)
        {
            IpcClientChannel channel = new IpcClientChannel();
                       
            ChannelServices.RegisterChannel(channel, true);

            // Get an instance of the remote object.
            _ipcRemoteObj = Activator.GetObject(typeof(IpcRemoteObject), "ipc://" + _appIPCName + "/" + _appIPCURI) as IpcRemoteObject;

            if (path.Length > 0)
            {
                _ipcRemoteObj.uploadFile(path);
            }
            else
            {
                _ipcRemoteObj.showGUI();
            }            
        }

        private static void filebin_UploadFinished(object sender, libfbclientnet.UploadFinishedEventArgs e)
        {
            try
            {                
                Clipboard.SetText(e.Result.URL, TextDataFormat.Text);
                _notify.ShowBalloonTip(10000, "upload with success", e.Result.URL + "\n link copied to clipboard!", System.Windows.Forms.ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        public static void SetShellExtension()
        {
            string runningDir = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string batFileName = "fb-client_setExtension.bat";

            if (Registry.ClassesRoot.OpenSubKey("*\\shell\\Paste to Filebin\\command") == null && System.IO.File.Exists(runningDir + "\\" + batFileName))
            {
                ProcessStartInfo procInfo = new ProcessStartInfo();
                procInfo.UseShellExecute = true;
                procInfo.FileName = batFileName;
                procInfo.WorkingDirectory = runningDir;
                procInfo.Verb = "runas";
                Process.Start(procInfo);
            }
        }

        public static void OpenGUIDispatcher()
        {
            _app.Dispatcher.BeginInvoke(new Action(OpenGUI));            
        }

        private static void OpenGUI()
        {
            if (_guiWindow == null || !_guiWindow.IsVisible)
            {
                _guiWindow = new MainWindow();

                _guiWindow.Closed += guiWindow_Closed;
            }

            _guiWindow.Show();
            _guiWindow.Activate();
        }

        private static void guiWindow_Closed(object sender, EventArgs e)
        {
            _guiWindow = null;
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

                System.Drawing.Image img = SnippingTool.Snip();

                if (img != null)
                {
                    string pTMPPath = System.IO.Path.GetTempPath() + "screenshot.png";

                    img.Save(pTMPPath);

                    uploadFile(pTMPPath);
                }

            }
            catch (Exception ex)
            {
                fb_messageBox.ShowBox(ex);
            }
        }

        public static void uploadFile(string path)
        {
            if (_guiWindow != null && _guiWindow.IsVisible)
            {
                _guiWindow.SetFileInfoDispatcher(path);
            }

            filebin.UploadFileAsync(path);
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
                            case "-installUpdate":
                                installUpdate();
                                break;
                        }
                    }
                }
            }

            return retList;
        }

        private static bool checkForUpdate()
        {
            System.IO.FileInfo destFile = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            System.Net.WebRequest sourceWebReq = System.Net.WebRequest.Create(updateLink);
            System.Net.HttpWebResponse sourceWebRes = (System.Net.HttpWebResponse)sourceWebReq.GetResponse();

            return sourceWebRes.LastModified > destFile.LastWriteTime;            
        }

        private static void doUpdate()
        {
            System.IO.DirectoryInfo destDir;
            UpdateWindow frmUpdate = new UpdateWindow();

            if (frmUpdate.ShowDialog() == true)
            {
                System.IO.FileInfo currentFile = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
                destDir = currentFile.Directory.CreateSubdirectory("temp");

                System.IO.FileInfo destFile = new System.IO.FileInfo(destDir.FullName + @"\" + currentFile.Name);

                System.IO.File.Copy(currentFile.FullName, destDir.FullName + @"\" + currentFile.Name, true);
                ProcessStartInfo startInfo = new ProcessStartInfo(destFile.FullName, @"-installUpdate");
                Process.Start(startInfo);

                Application.Exit();
            }
        }

        private static void installUpdate()
        {
            System.IO.FileInfo tempFile = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Ionic.Zip.ZipFile zipFile = new Ionic.Zip.ZipFile(tempFile.Directory.GetFiles("fb-client.net.zip")[0].FullName);
            zipFile.ExtractAll(tempFile.Directory.Parent.FullName);

            System.IO.FileInfo destFile = new System.IO.FileInfo(tempFile.Directory.Parent.FullName + @"\" + tempFile.Name);

            destFile.LastWriteTime = DateTime.Now;

            ProcessStartInfo startInfo = new ProcessStartInfo(destFile.FullName);
            Process.Start(startInfo);

            Application.Exit();
        }

        private static void cleanUp()
        {
            System.IO.FileInfo currentFile = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string zipPath = currentFile.Directory.FullName + @"\fb-client.net.zip";


            if (System.IO.Directory.Exists(currentFile.Directory.FullName + @"\temp"))
            {
                System.IO.Directory.Delete(currentFile.Directory.FullName + @"\temp", true);

                if (System.IO.File.Exists(zipPath))
                {
                    System.IO.File.Delete(zipPath);
                }
            }            
        }

    }
}
