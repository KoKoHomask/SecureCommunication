using SecureCommunication.Interface;
using SecureCommunication.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication.ExtendedProtection;
using System.Text;

namespace SecureCommunication.Base
{
    public class TCPServer
    {
        const int BufLen = 4096;
        TcpListener Listener { get; }
        IProtocol Protocol { get; }
        public TCPServer(string Address, int Port,IProtocol protocol)
        {
            var address = IPAddress.Parse(Address);
            Listener = new TcpListener(address, Port);
            Protocol = protocol;
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
            var connect = asyncCall.AsyncState as ConnectModel;
            var len = connect.client.EndReceive(asyncCall);
            Protocol.RecieveDataProcess(connect, len);
            connect.client.BeginReceive(connect.rData, 0, BufLen, 0, ResultCallBace, connect);            
        }
    }
}
