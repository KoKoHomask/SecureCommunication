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
        public event Action<byte[]> GetServerInfoEvent;
        public event Action<byte[]> GetSessionIDEvent;
        public event Action<byte[]> SetUpSessionEvent;
        public event Action<byte[]> SendMessageEvent;
        public enum 消息类型
        {
            获取服务器信息=0,
            请求sessionID,
            请求建立会话,
            发送消息,
        }
        public ConcurrentDictionary<string, DeviceModel> DeviceList { get; }
        public Protocol()
        {
            DeviceList = new ConcurrentDictionary<string, DeviceModel>();
        }
        public ProcessModel AnalysisReciveData(string remote,byte[] reciveData)
        {
            var model = new ProcessModel();
            switch ((消息类型)reciveData[0])
            {
                case 消息类型.获取服务器信息://获取服务器信息
                    GetServerInfoEvent?.Invoke(reciveData);
                    break;
                case 消息类型.请求sessionID://请求自己的SessionID
                    GetSessionIDEvent?.Invoke(reciveData);
                    break;
                case 消息类型.请求建立会话://请求与SessionID建立会话(将自己加了密的公钥发给该客户)
                    SetUpSessionEvent?.Invoke(reciveData);
                    break;
                case 消息类型.发送消息://发送消息给SessionID
                    SendMessageEvent?.Invoke(reciveData);
                    break;
                default:
                    break;
            }
            return null;
        }
        /// <summary>
        /// 从接收到的数组中提取sessionid
        /// </summary>
        /// <param name="reciveArray"></param>
        /// <param name="idOffset"></param>
        /// <returns></returns>
        private byte[] GetSessionIDFromReciveArray(byte[] reciveArray,int idOffset=1)
        {
            string sessionID = Guid.NewGuid().ToString().Split('-')[0];
            byte[] array= Encoding.Default.GetBytes(sessionID.ToCharArray());
            for(int i=0;i<array.Length;i++)
            {
                array[i] = reciveArray[i + idOffset];
            }
            return array;
        }
        /// <summary>
        /// 向发送数组中添加sessionid
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="sendArray"></param>
        /// <returns></returns>
        private byte[] SetSessionIDIntoSendArray(byte[] sessionID,byte[] sendArray)
        {
            var lst = new List<byte>(sessionID);
            lst.AddRange(sendArray);
            return lst.ToArray();
        }
        /// <summary>
        /// 替换数组中的sessionid
        /// </summary>
        /// <param name="replaceID"></param>
        /// <param name="array"></param>
        /// <param name="idOffset"></param>
        /// <returns></returns>
        private byte[] ReplaceSessionIDFromArray(byte[] replaceID,byte[] array,int idOffset = 1)
        {
            for (int i = 0; i < replaceID.Length; i++)
            {
                array[i + idOffset] = replaceID[i];
            }
            return array;
        }
    }
    public class ProcessModel
    {
        public string SessionID { get; set; }
        public byte[] SendArray { get; set; }
    }
}
