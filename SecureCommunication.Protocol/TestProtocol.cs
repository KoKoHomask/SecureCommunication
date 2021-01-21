using SecureCommunication.Interface;
using SecureCommunication.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecureCommunication.Protocol
{
    public class TestProtocol : IProtocol
    {

        ProtocolBackModel IProtocol.RecieveDataProcess(ConnectModel connect, int len)
        {
            ProtocolBackModel model = new ProtocolBackModel();
            var strArray = Encoding.Default.GetBytes("hello world");
            byte[] array = new byte[len + strArray.Length];
            Array.Copy(strArray, array, strArray.Length);
            Array.Copy(connect.rData, 0, array, strArray.Length, len);
            model.Array = array;
            model.Status = BackStatus.WAITNEXT;
            return model;
        }
    }
}
