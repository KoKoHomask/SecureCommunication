using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SecureCommunication.Common
{
    /// <summary>
    /// 通讯协议
    /// </summary>
    public class Protocol
    {
        public ConcurrentDictionary<string, DeviceModel> DeviceList { get; }
        public Protocol()
        {
            DeviceList = new ConcurrentDictionary<string, DeviceModel>();
        }
        //public byte[] AnalysisReciveData(byte[] reciveData)
        //{
        //}
    }
}
