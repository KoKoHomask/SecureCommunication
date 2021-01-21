using System;
using System.Collections.Generic;
using System.Text;

namespace SecureCommunication.Interface
{
    public interface IChannel
    {
        void Start();
        void Stop();
        void Send(byte[] sendArray, string remote);
    }
}
