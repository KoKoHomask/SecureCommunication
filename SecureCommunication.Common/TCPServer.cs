using SecureCommunication.Interface;
using SecureCommunication.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication.ExtendedProtection;
using System.Text;

namespace SecureCommunication.Common
{
    public class TCPServer
    {
        public int BufLen { get; private set; } = 4096;
        TcpListener Listener { get; }
        List<IProtocol> Protocol { get; }
        public TCPServer(string Address, int Port, List<IProtocol> protocol)
        {
            var address = IPAddress.Parse(Address);
            Listener = new TcpListener(address, Port);
            Protocol = protocol;
            Listener.Start();
            Listener.BeginAcceptSocket(ListenerBeginCall, Listener);
        }
        public TCPServer(string Address, int Port, List<IProtocol> protocol,int bufLen)
        {
            BufLen = bufLen;
            var address = IPAddress.Parse(Address);
            Listener = new TcpListener(address, Port);
            Protocol = protocol;
            Listener.Start();
            Listener.BeginAcceptSocket(ListenerBeginCall, Listener);
        }
        void ListenerBeginCall(IAsyncResult iResult)
        {
            var listener = iResult.AsyncState as TcpListener;
            Socket clientSocket = listener.EndAcceptSocket(iResult);
            ConnectModel connect = new ConnectModel() { rData = new byte[BufLen], client = clientSocket };
            clientSocket.BeginReceive(connect.rData, 0, BufLen, 0, ResultCallBace, connect);
            if (clientSocket.Connected)
                Console.WriteLine("\nClient Connected!!\n==================\nCLient IP {0}\n", clientSocket.RemoteEndPoint);
            listener.BeginAcceptSocket(ListenerBeginCall, listener);
        }
        void ResultCallBace(IAsyncResult asyncCall)
        {
            try
            {
                ProtocolBackModel res = null;
                ConnectModel connect = asyncCall.AsyncState as ConnectModel;
                var len = connect.client.EndReceive(asyncCall);
                for (int i = 0; i < Protocol.Count; i++)
                {
                    var _protocol = Protocol[i];
                    res = _protocol.RecieveDataProcess(connect, len);
                    if (res.Array != null && res.Array.Length > 0)
                        connect.client.Send(res.Array);

                    if (res.Status != BackStatus.ABANDONED)//这个协议对本次请求有特殊要求
                        break;//跳出循环
                }
                //此处预留后期扩展成协议池
                switch (res.Status)
                {
                    case BackStatus.ABANDONED:
                        connect.client.Shutdown(SocketShutdown.Both);
                        break;
                    case BackStatus.NOOP:
                        break;
                    case BackStatus.WAITNEXT:
                        connect.client.BeginReceive(connect.rData, 0, BufLen, 0, ResultCallBace, connect);
                        break;
                }
            }
            catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }           
        }
    }
}
