using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SecureCommunication.Common
{
    public abstract class TCPHelper
    {
        public abstract event Action<string, byte[]> ReciveDataEvent;
        protected const string HEARTMESSAGE = "heartmessage";
        protected const string EXITMESSAGE = "exitexitttit";
        protected ConcurrentDictionary<string, DeviceModel> devicelist;
        public abstract void Send(byte[] sendArray, string remote);
        public abstract void Start();
        public abstract void Stop();
    }
}
//http://developer.51cto.com/art/200907/137263.htm
//https://github.com/MrDesjardins/SimpleHttpProxy/blob/master/SimpleHttpProxy/Program.cs
