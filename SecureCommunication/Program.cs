using SecureCommunication.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            Console.Write("Input localPort:");
            string localPort = Console.ReadLine();
            udphelper = new UDPClientHelper(DeviceList, int.Parse(localPort), "39.107.243.182", 12345);
            //udphelper = new UDPClientHelper(DeviceList, int.Parse(localPort), "192.168.1.77", 12345);
            udphelper.StartUDP();

            protocol = new ClientProtocol(DeviceList, udphelper);
            protocol.NewChatEvent += Protocol_NewChatEvent;
            protocol.ReciveMessageEvent += Protocol_ReciveMessageEvent;
            protocol.ReciveSysInfoEvent += Protocol_ReciveSysInfoEvent;
            protocol.ExchangeKeyEvent += Protocol_ExchangeKeyEvent;
            protocol.GetServerInfo();
            
            Console.WriteLine("Waiting....");
            Wait:
            string input = Console.ReadLine();
            input = input.Substring(0, SessionID.Length);
            if (SessionID == "")
                goto Wait;
            MyselfRSA = new RSAHelper();

            string str = "";
            Random rd = new Random();
            for (int i = 0; i < 1000; i++)
                str += rd.Next().ToString();
            randomKey = str;
            MyselfRSA = new RSAHelper();
            var publicKey = MyselfRSA.KeyToXmlString;
            string singleSecurity = CustomerSecurity.Encrypt(publicKey, randomKey);//得到公钥的简单加密
            protocol.RequestChat(input, Encoding.Default.GetBytes(singleSecurity));

        }
        private static void Protocol_ExchangeKeyEvent(byte[] obj)
        {
            //if (MyselfRSA == null) MyselfRSA = new RSAHelper();
            var sidArry = protocol.GetSessionIDFromReciveArray(obj);
            List<byte> lst = new List<byte>(obj);
            for(int i=0;i<1+ sidArry.Length;i++)
            {
                lst.RemoveAt(0);
            }
            byte[] dataArray = lst.ToArray();
            string data = CustomerSecurity.Decrypt(Encoding.Default.GetString(dataArray), randomKey);
            List<byte> sendLst = new List<byte>() { 0x00 };
            
            sendLst.AddRange(sidArry);
            if(data.IndexOf("RSAKeyValue")>=0)
            {
                OtherRSA = new RSAHelper(data);
                string myPubKey = MyselfRSA.KeyToXmlString;
                var myPKArray= OtherRSA.Encrypt(Encoding.Default.GetBytes(myPubKey));
                protocol.SendMessage(sidArry, myPKArray);
                Console.WriteLine("Get target public key ready.");
                
            }
            else//还是被加密的字符串，取下自己的key后回传过去
            {
                protocol.ExchangeKey(sidArry, data);
            }
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
            var sidArry = protocol.GetSessionIDFromReciveArray(obj);
            List<byte> lst = new List<byte>(obj);
            for (int i = 0; i < 1 + sidArry.Length; i++)
            {
                lst.RemoveAt(0);
            }
            byte[] dataArray = lst.ToArray();
            byte[] reciveDataArray = MyselfRSA.Decrypt(dataArray);
            string reciveData = Encoding.Default.GetString(reciveDataArray);

            if (OtherRSA==null)
            {
                OtherRSA = new RSAHelper(reciveData);
                byte[] reponseOK = Encoding.Default.GetBytes("Ready!");
                protocol.SendMessage(sidArry, OtherRSA.Encrypt(reponseOK));
                Console.WriteLine("get a rsa public key");
            }
            else
            {
                
                Console.WriteLine(reciveData);
            }
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
