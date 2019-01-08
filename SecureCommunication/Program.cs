using SecureCommunication.Common;
using System;
using System.Collections.Concurrent;
using System.Text;
using static SecureCommunication.Common.Protocol;

namespace SecureCommunication
{
    class Program
    {
        static ConcurrentDictionary<string, DeviceModel> DeviceList = new ConcurrentDictionary<string, DeviceModel>();
        static ClientProtocol protocol;
        static UDPHelper udphelper;
        static string SessionID;
        static void Main(string[] args)
        {
            udphelper = new UDPClientHelper(DeviceList, 12346, "39.107.243.182", 12345);
            //udphelper = new UDPClientHelper(DeviceList, 12346, "192.168.1.77", 12345);
            udphelper.StartUDP();

            protocol = new ClientProtocol(DeviceList, udphelper);
            protocol.NewChatEvent += Protocol_NewChatEvent;
            protocol.ReciveMessageEvent += Protocol_ReciveMessageEvent;
            protocol.ReciveSysInfoEvent += Protocol_ReciveSysInfoEvent;
            protocol.GetServerInfo();
            
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }

        private static void Protocol_ReciveSysInfoEvent(byte[] obj)
        {
            switch((MsgType)obj[0])
            {
                case MsgType.C_GetServerInfo:
                    Console.WriteLine(Encoding.Default.GetString(obj,1,obj.Length-1));
                    break;
            }
            //throw new NotImplementedException();
        }

        private static void Protocol_ReciveMessageEvent(byte[] obj)
        {
            //throw new NotImplementedException();
        }

        private static void Protocol_NewChatEvent(byte[] obj)
        {
            //throw new NotImplementedException();
        }
    }
}
