using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace SecureCommunication.Common
{
    public class DeviceModel
    {
        public EndPoint IP { get; set; }
        public string SessionID { get; set; }
        public DateTime date { get; set; }
    }
    public abstract class UDPHelper
    {
        protected const string HEARTMESSAGE = "heartmessage";
        protected const string EXITMESSAGE = "exitexitttit";
        protected ConcurrentDictionary<string, DeviceModel> devicelist;
        public abstract void Send(byte[] sendArray, string remote);
        public abstract void StartUDP();
    }
}
