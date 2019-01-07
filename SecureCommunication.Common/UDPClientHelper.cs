using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SecureCommunication.Common
{
    public class UDPClientHelper:UDPHelper
    {
        bool client_thread_flag = false;
        long client_HeartTick = 0;
        static object lockobj = new object();
        UdpClient udpClient;
        IPEndPoint udpServerEndPoint;
        public int LocalPort { get; set; }
        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public UDPClientHelper(ConcurrentDictionary<string, DeviceModel> DeviceList, int localPort, string serverAddress, int serverPort)
        {
            devicelist = DeviceList;
            LocalPort = localPort;
            ServerAddress = serverAddress;
            ServerPort = serverPort;
        }
        public void StopUDPClient()
        {
            client_thread_flag = false;
        }

        public override void StartUDP()
        {
            udpClient?.Close();
            udpClient = new UdpClient(LocalPort);
            udpClient?.Send(Encoding.Default.GetBytes(HEARTMESSAGE), Encoding.Default.GetBytes(HEARTMESSAGE).Length, ServerAddress, ServerPort);
            IPAddress IPadr = IPAddress.Parse(ServerAddress);
            udpServerEndPoint = new IPEndPoint(IPadr, ServerPort);
            //EndPoint Remote = ipep;
            client_thread_flag = true;
            new Thread(() => {
                while (client_thread_flag)//心跳包
                {
                    Thread.Sleep(10);
                    lock (lockobj)
                    {
                        client_HeartTick++;
                    }
                    if (client_HeartTick % 500 == 0)
                    {
                        byte[] byteArray = Encoding.Default.GetBytes(HEARTMESSAGE);
                        udpClient?.Send(byteArray, byteArray.Length, ServerAddress, ServerPort);
                    }
                    if (client_HeartTick % 1000 == 0)
                    {
                        Console.WriteLine("unable to connect to UDP server,it may be after the network is restored");
                    }
                }
                new Thread(() =>
                {
                    byte[] array = new byte[50];
                    //DateTime lastdate = DateTime.Now;
                    while (client_thread_flag)
                    {
                        byte[] data = new byte[1024];
                        try
                        {
                            data = udpClient.Receive(ref udpServerEndPoint);

                            lock (lockobj)
                            {
                                client_HeartTick = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("error：{0}", ex.Message));
                            break;
                        }
                    }
                    udpClient.Close();
                }).Start();
            }).Start();
        }
        public override void Send(byte[] byteArray,string remote)
        {
            udpClient?.Send(byteArray, byteArray.Length, ServerAddress, ServerPort);
            lock (lockobj)
            {
                client_HeartTick = 0;
            }
        }
    }
}
