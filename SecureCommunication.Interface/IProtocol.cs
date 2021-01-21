using SecureCommunication.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecureCommunication.Interface
{
    public interface IProtocol
    {
        ProtocolBackModel RecieveDataProcess(ConnectModel connect, int len);
    }
}
