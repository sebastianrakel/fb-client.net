using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fb_client.net
{
    public class IpcRemoteObject : MarshalByRefObject
    {        
        public void uploadFile(string path)
        {
            Program.uploadFile(path);
        }
    }
}
