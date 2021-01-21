using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SecureCommunication.Model
{
    public class ConnectModel
    {
        public Socket client { get; set; }
        public byte[] rData { get; set; }
    }
}
