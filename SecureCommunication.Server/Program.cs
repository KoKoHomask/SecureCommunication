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
        static void Main(string[] args)
        {
            UDPHelper udphelper = new UDPServerHelper(DeviceList, 12345);
            protocol = new Protocol(DeviceList,udphelper);
            
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
