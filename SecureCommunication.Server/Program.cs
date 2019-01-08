using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using SecureCommunication.Common;

namespace SecureCommunication.Server
{
    class Program
    {
        static ConcurrentDictionary<string, DeviceModel> DeviceList = new ConcurrentDictionary<string, DeviceModel>();
        static Protocol protocol;
        static UDPHelper udphelper;
        static void Main(string[] args)
        {
            string hostName = Dns.GetHostName();
            IPAddress iPAddress;
            IPHostEntry localhost = Dns.GetHostEntry(hostName);
            choseList:
            if (localhost.AddressList.Length > 1)
            {
                Console.WriteLine("==========");
                for (int i=0;i<localhost.AddressList.Length;i++)
                {
                    Console.WriteLine(i+":" + localhost.AddressList[i]);
                }
                Console.WriteLine("Chose Host:");
                var choseIndex = Console.ReadKey().KeyChar;
                int index = choseIndex - 48;
                if(index<0||index>=localhost.AddressList.Length)
                {
                    Console.WriteLine("Error Input!");
                    goto choseList;
                }
                iPAddress = localhost.AddressList[index];
            }
            else iPAddress= localhost.AddressList[0];


            udphelper = new UDPServerHelper(DeviceList, iPAddress, 12345);
            protocol = new Protocol(DeviceList,udphelper,false);
            
            udphelper.StartUDP();
            udphelper.ReciveDataEvent += Udphelper_ReciveDataEvent;
            Console.WriteLine("\n\n按[F4]键退出。");
            ConsoleKey key;
            while (true)
            {
                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.F4)
                {
                    Console.WriteLine("end waiting for udp data.");
                    udphelper.StopUDP();
                    break;
                }
                Thread.Sleep(1);
            }
        }

        private static void Udphelper_ReciveDataEvent(string arg1, byte[] arg2)
        {
            protocol.AnalysisReciveData(arg1, arg2);
        }
    }
}
