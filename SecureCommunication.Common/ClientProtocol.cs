using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SecureCommunication.Common
{
    public class ClientProtocol : Protocol, IClientProtocol
    {
        public ClientProtocol(ConcurrentDictionary<string, DeviceModel> DeviceList, UDPHelper uDPHelper, bool AutoProcess = true)
            : base(DeviceList, uDPHelper, AutoProcess)
        {
        }

        public void ExchangeKey(byte[] targetSid,string securityStr)
        {
            List<byte> send = new List<byte>() { (byte)MsgType.S_ExchangeKey };
            send.AddRange(targetSid);
            send.AddRange(Encoding.Default.GetBytes(securityStr));
            UDPHelper.Send(send.ToArray(), "");
        }

        public void GetServerInfo()
        {
            UDPHelper.Send(new byte[] { (byte)MsgType.S_GetServerInfo }, "");
            //throw new NotImplementedException();
        }

        public void GetSessionID()
        {
            UDPHelper.Send(new byte[] { (byte)MsgType.S_GetSessionID }, "");
        }
        public void SendMessage(byte[] targetSid,byte[] msgArray)
        {
            List<byte> send = new List<byte>() { (byte)MsgType.S_SendMsg };
            send.AddRange(targetSid);
            send.AddRange(msgArray);
            UDPHelper.Send(send.ToArray(), "");
        }
        public void RequestChat(string targetSid,byte[] requestArray)
        {
            List<byte> send = new List<byte>() { (byte)MsgType.S_NewChat };
            send.AddRange(Encoding.Default.GetBytes(targetSid));
            send.AddRange(requestArray);
            UDPHelper.Send(send.ToArray(), "");
        }
    }
}
