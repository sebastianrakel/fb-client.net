using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fb_client.net
{
    class GlobalFunctions
    {          
        public static void CheckForAPIKey()
        {
            if(fb_client.net.Properties.Settings.Default.apikey.Length == 0) {
                SetFBAPIKey();
            }            
        }

        public static void SetFBAPIKey(string apikey = "")
        {            
            if (apikey != null && apikey.Length == 0)
            {
                apikey = inputBox.ShowInput("please, input apikey","");
            }

            if (apikey.Length > 0)
            {
                fb_client.net.Properties.Settings.Default.apikey = apikey;
                fb_client.net.Properties.Settings.Default.Save();
            }
        }
    }
}
