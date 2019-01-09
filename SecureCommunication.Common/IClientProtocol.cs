using System;
using System.Collections.Generic;
using System.Text;

namespace SecureCommunication.Common
{
    interface IClientProtocol
    {
        void GetServerInfo();
        void GetSessionID();
        void RequestChat(string targetSid, byte[] requestArray);
        void ExchangeKey(byte[] targetSid, string securityStr);
        void SendMessage(byte[] targetSid, byte[] msgArray);
    }
}
