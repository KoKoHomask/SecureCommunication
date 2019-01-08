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
        static RSAHelper MyselfRSA;
        static RSAHelper OtherRSA;
        static string SessionID;
        static string randomKey;
        static bool BreakReponse = false;
        static void Main(string[] args)
        {
            Console.Write("Input server address:");
            string address = Console.ReadLine();
            udphelper = new UDPClientHelper(DeviceList, 12346, address, 12345);
            udphelper.StartUDP();

            protocol = new ClientProtocol(DeviceList, udphelper);
            protocol.NewChatEvent += Protocol_NewChatEvent;
            protocol.ReciveMessageEvent += Protocol_ReciveMessageEvent;
            protocol.ReciveSysInfoEvent += Protocol_ReciveSysInfoEvent;
            protocol.ExchangeKeyEvent += Protocol_ExchangeKeyEvent;
            protocol.GetServerInfo();
            
            Console.WriteLine("Waiting....");
            Wait:
            string input= Console.ReadLine();
            if (SessionID == "")
                goto Wait;

        }

        private static void Protocol_ExchangeKeyEvent(byte[] obj)
        {
            
        }

        private static void Protocol_ReciveSysInfoEvent(byte[] obj)
        {
            switch((MsgType)obj[0])
            {
                case MsgType.C_GetServerInfo:
                    Console.WriteLine(Encoding.Default.GetString(obj,1,obj.Length-1));
                    protocol.GetSessionID();
                    break;
                case MsgType.C_GetSessionID:
                    SessionID = (Encoding.Default.GetString(obj, 1, obj.Length - 1));
                    Console.WriteLine("SessionID:" + SessionID);
                    Console.WriteLine("Now you can connected orther people with your session id");
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
            if (BreakReponse) return;
            byte[] sid = protocol.GetSessionIDFromReciveArray(obj);
            string sidStr = Encoding.Default.GetString(sid);
            Console.Write("Have new request" + sidStr + ",agree?:");
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Y) {//同意请求
                //生成一个随机加密的字符串
                
                string str = "";
                Random rd = new Random();
                for (int i = 0; i < 1000; i++)
                    str += rd.Next().ToString();
                randomKey = str;
                MyselfRSA = new RSAHelper();
                var publicKey = MyselfRSA.KeyToXmlString;
                string singleSecurity = CustomerSecurity.Encrypt(publicKey, randomKey);//得到公钥的简单加密
                byte[] sidArray= protocol.GetSessionIDFromReciveArray(obj);
                protocol.ExchangeKey(sidArray, singleSecurity);
            }
            //throw new NotImplementedException();
        }
    }
}
