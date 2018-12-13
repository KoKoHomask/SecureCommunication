﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace SecureCommunication.Common
{
    public class UDPModel
    {
        public EndPoint IP { get; set; }
        public string SessionID { get; set; }
        public DateTime date { get; set; }
    }
    public class UDPHelper
    {
        const string HEARTMESSAGE = "heartmessage";
        const string EXITMESSAGE = "exitexitttit";
        #region udpServer
        private static readonly ConcurrentDictionary<string, UDPModel> list = new ConcurrentDictionary<string, UDPModel>();//Dictionary
        bool server_thread_flag = false;
        Socket udpServer;
        public void StopUDPServer()
        {
            server_thread_flag = false;
        }
        public void StartUDPServer(IPEndPoint serverIP)
        {
            udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpServer.Bind(serverIP);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)ipep;
            server_thread_flag = true;
            new Thread(() => {//清理列表中无效UDP
                List<string> rmKey = new List<string>();
                while (server_thread_flag)
                {
                    rmKey.Clear();
                    long now = DateTime.Now.Ticks;
                    foreach (var tmp in list)
                    {
                        if (new TimeSpan(now - tmp.Value.date.Ticks).TotalSeconds > 30)
                        {
                            rmKey.Add(tmp.Key);
                        }
                    }
                    if (rmKey.Count != 0)
                    {
                        for (int i = 0; i < rmKey.Count; i++)
                        {
                            UDPModel model;
                            list.TryRemove(rmKey[i], out model);
                            Console.WriteLine("clear expried ipendpoint{0}", rmKey[i]);
                        }
                    }
                    Thread.Sleep(1);
                }
            }).Start();
            new Thread(() =>
            {
                while (server_thread_flag)
                {
                    byte[] data = new byte[1024];
                    int length = 0;
                    try
                    {
                        length = udpServer.ReceiveFrom(data, ref Remote);//接受来自服务器的数据
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("error：{0}", ex.Message));
                        break;
                    }
                    string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string message = Encoding.UTF8.GetString(data, 0, length);

                    string ipport = (Remote as IPEndPoint).Address.ToString() + ":" + (Remote as IPEndPoint).Port.ToString();
                    UDPModel model = new UDPModel() { IP = Remote, date = DateTime.Now, SessionID = Guid.NewGuid().ToString().Split('-')[0] };
                    list.AddOrUpdate(ipport, model, (k, oldvalue) => oldvalue = model);

                    if (message == EXITMESSAGE || message == HEARTMESSAGE)
                    {
                        udpServer.SendTo(data, length, SocketFlags.None, Remote);
                    }
                    else
                    {
                        Console.WriteLine(string.Format("{0} recive message from{1}:{2}", datetime, ipport, message));
                        var send = list.Where(x => x.Key != ipport).ToList();
                        for (int i = 0; i < send.Count(); i++)
                        {
                            udpServer.SendTo(data, length, SocketFlags.None, send[i].Value.IP);
                            Console.WriteLine("send message to{0}", (send[i].Value.IP as IPEndPoint).Address.ToString());
                        }
                    }
                }
                udpServer.Close();
            }).Start();
        }
        public void UDPServerSend(byte[] sendArray,string remoteSessionID)
        {
            var send = list.Where(x => x.Value.SessionID == remoteSessionID).FirstOrDefault();
            udpServer?.SendTo(sendArray, sendArray.Length, SocketFlags.None, send.Value.IP);
        }
        #endregion

        #region udpClient
        bool client_thread_flag = false;
        long client_HeartTick = 0;
        static object lockobj = new object();
        UdpClient udpClient;

        public void StopUDPClient()
        {
            client_thread_flag = false;
        }

        public void StartUDPClient(int localPort, string ServerAddress, int ServerPort)
        {
            udpClient?.Close();
            udpClient = new UdpClient(localPort);
            udpClient?.Send(Encoding.Default.GetBytes(HEARTMESSAGE), Encoding.Default.GetBytes(HEARTMESSAGE).Length, ServerAddress, ServerPort);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = ipep;
            client_thread_flag = true;
            new Thread(() => {
                while (client_thread_flag)
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
                udpClient?.Close();
            }).Start();
        }
        public void UDPClientSend(byte[] byteArray, string ServerAddress, int ServerPort)
        {
            udpClient?.Send(byteArray, byteArray.Length, ServerAddress, ServerPort);
            lock (lockobj)
            {
                client_HeartTick = 0;
            }
        }
        #endregion

    }
}
