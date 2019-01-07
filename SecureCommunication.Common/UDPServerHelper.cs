using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SecureCommunication.Common
{
    class UDPServerHelper: UDPHelper
    {
        public event Action<string, byte[]> ReciveDataEvent;
        bool server_thread_flag = false;
        Socket udpServer;
        public int LocalPort { get; set; }
        public UDPServerHelper(ConcurrentDictionary<string, DeviceModel> DeviceList, int localPort)
        {
            devicelist= DeviceList;
            LocalPort = localPort;
        }
        public void StopUDPServer()
        {
            server_thread_flag = false;
        }
        public override void StartUDP()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostByName(hostName);
            IPAddress localaddr = localhost.AddressList[0];
            IPEndPoint serverIP = new IPEndPoint(localaddr, LocalPort);

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
                    foreach (var tmp in devicelist)
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
                            DeviceModel model;
                            devicelist.TryRemove(rmKey[i], out model);
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
                    byte[] data = new byte[10240];
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

                    string sessionID = Guid.NewGuid().ToString().Split('-')[0];
                    string ipport = (Remote as IPEndPoint).Address.ToString() + ":" + (Remote as IPEndPoint).Port.ToString();
                    DeviceModel model = new DeviceModel() { IP = Remote, date = DateTime.Now, SessionID = sessionID };
                    devicelist.AddOrUpdate(ipport, model, (k, oldvalue) => oldvalue = model);

                    if (message == EXITMESSAGE || message == HEARTMESSAGE)
                    {
                        udpServer.SendTo(data, length, SocketFlags.None, Remote);
                    }
                    else
                    {
                        byte[] reciveData = new byte[length];
                        Array.Copy(data, reciveData, length);
                        Console.WriteLine(string.Format("{0} recive message from{1}:{2}", datetime, ipport, message));
                        ReciveDataEvent?.Invoke(sessionID, reciveData);
                    }
                }
                udpServer.Close();
            }).Start();
        }

        public override void Send(byte[] sendArray,string remote)
        {
            var send = devicelist.Where(x => x.Value.SessionID == remoteSessionID).FirstOrDefault();
            udpServer?.SendTo(sendArray, sendArray.Length, SocketFlags.None, send.Value.IP);
        }
    }
}
