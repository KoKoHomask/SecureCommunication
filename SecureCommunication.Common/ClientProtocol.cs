using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SecureCommunication.Common
{
    public class ClientProtocol : Protocol, IClientProtocol
    {
        public ClientProtocol(ConcurrentDictionary<string, DeviceModel> DeviceList, UDPHelper uDPHelper, bool AutoProcess = true)
            : base(DeviceList, uDPHelper, AutoProcess)
        {
        }

        public void GetServerInfo()
        {
            UDPHelper.Send(new byte[] { (byte)MsgType.S_GetServerInfo }, "");
            //throw new NotImplementedException();
        }
    }
}
