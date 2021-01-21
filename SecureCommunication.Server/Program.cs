using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using SecureCommunication.Common;
using SecureCommunication.Interface;
using SecureCommunication.Model;
using SecureCommunication.Protocol;

namespace SecureCommunication.Server
{
    class Program
    {
        static TCPServer server;
        static void Main(string[] args)
        {

            List<IProtocol> protocols = new List<IProtocol>();
            protocols.Add(new TestProtocol());
            server = new TCPServer("0.0.0.0", 12345, protocols);

            Console.WriteLine("\n\n Press [F4] to exit。");
            ConsoleKey key;
            while (true)
            {
                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.F4)
                {
                    Console.WriteLine("end waiting for udp data.");
                    break;
                }
                Thread.Sleep(1);
            }
        }

        //private static void Udphelper_ReciveDataEvent(string arg1, byte[] arg2)
        //{
        //    protocol.AnalysisReciveData(arg1, arg2);
        //}
    }
}
