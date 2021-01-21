using SecureCommunication.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SecureCommunication
{
    class Program
    {
        
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 12345);
            var ns=client.GetStream();
            ns.Write(Encoding.Default.GetBytes("test data"));
            System.Threading.Thread.Sleep(100);
            byte[] rec = new byte[1024];
            var len=ns.Read(rec, 0, rec.Length);
            Console.WriteLine(Encoding.Default.GetString(rec));
        }

    }
}
