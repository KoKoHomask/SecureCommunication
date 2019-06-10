using System;
using System.Collections.Generic;
using System.Text;

namespace SecureCommunication.Common
{

    class TCPServerHelper : TCPHelper
    {
        public override event Action<string, byte[]> ReciveDataEvent;
        
        public TCPServerHelper()
        {

        }

        public override void Send(byte[] sendArray, string remote)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
