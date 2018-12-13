using System;
using System.Net;
using System.Threading;
using SecureCommunication.Common;

namespace SecureCommunication.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Protocol protocol = new Protocol();
            UDPHelper udphelper = new UDPHelper(protocol.DeviceList);
            string hostName = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostByName(hostName);
            IPAddress localaddr = localhost.AddressList[0];
            IPEndPoint EndPoint = new IPEndPoint(localaddr, 12345);//传递IPAddress和Port
            udphelper.StartUDPServer(EndPoint);
            udphelper.ReciveDataEvent += Udphelper_ReciveDataEvent;

            Console.WriteLine("\n\n按[F4]键退出。");
            ConsoleKey key;
            while (true)
            {
                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.F4)
                {
                    Console.WriteLine("end waiting for udp data.");
                    udphelper.StopUDPServer();
                    break;
                }
                Thread.Sleep(1);
            }
        }

        private static void Udphelper_ReciveDataEvent(string arg1, byte[] arg2)
        {
            
        }
    }
}
