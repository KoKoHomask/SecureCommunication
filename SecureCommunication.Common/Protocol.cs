﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SecureCommunication.Common
{
    /// <summary>
    /// 通讯协议
    /// </summary>
    public class Protocol
    {
        public event Action<byte[]> ReciveMessageEvent;
        public event Action<byte[]> NewChatEvent;
        public event Action<byte[]> ReciveSysInfoEvent;
        public enum MsgType
        {
            S_GetServerInfo=0,
            S_GetSessionID,
            S_NewChat,//服务端转发客户端建立会话请求
            S_SendMsg,//服务端转发消息
            C_GetServerInfo,
            C_GetSessionID,
            C_NewChat,//客户端新的会话请求
            C_ReciveMessage,
        }
        public string ServerInfoMsg { get; set; } = "Hello World!";
        protected ConcurrentDictionary<string, DeviceModel> deviceList { get; }
        protected UDPHelper UDPHelper { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceList">设备列表</param>
        /// <param name="uDPHelper">UDP发送</param>
        /// <param name="AutoProcess">是否由协议自动处理</param>
        public Protocol(ConcurrentDictionary<string, DeviceModel> DeviceList,UDPHelper uDPHelper,bool AutoProcess=true)
        {
            deviceList = DeviceList;
            UDPHelper = uDPHelper;
            if (AutoProcess)
                UDPHelper.ReciveDataEvent += AnalysisReciveData;
        }
        public void AnalysisReciveData(string remote,byte[] reciveData)
        {
            var model = new ProcessModel();
            DeviceModel deviceModel;
            byte[] sessionID;
            string idStr;
            switch ((MsgType)reciveData[0])
            {
                case MsgType.S_GetServerInfo://获取服务器信息
                    Console.WriteLine(remote + ": " + byteToHexStr(reciveData));
                    if (deviceList.TryGetValue(remote, out deviceModel))
                    {
                        var sendArray= Encoding.Default.GetBytes(ServerInfoMsg.ToCharArray());
                        List<byte> lst = new List<byte>() { (byte)MsgType.C_GetServerInfo };
                        lst.AddRange(sendArray);
                        UDPHelper.Send(lst.ToArray(), remote);//将服务器信息以明文方式返回
                    }
                    break;
                case MsgType.S_GetSessionID://请求自己的SessionID
                    Console.WriteLine(remote + ": " + byteToHexStr(reciveData));
                    if (deviceList.TryGetValue(remote, out deviceModel))
                    {
                        var sendArray = Encoding.Default.GetBytes(deviceModel.SessionID.ToCharArray());
                        UDPHelper.Send(sendArray, remote);
                    }
                    
                    break;
                case MsgType.S_NewChat://请求与SessionID建立会话(将自己加了密的公钥发给该客户)
                    Console.WriteLine(remote + ": " + byteToHexStr(reciveData));
                    sessionID = GetSessionIDFromReciveArray(reciveData);
                    idStr = Encoding.Default.GetString(sessionID);
                    var device= deviceList.Where(x => x.Value.SessionID == idStr).FirstOrDefault();
                    if (device.Key!=null&&deviceList.TryGetValue(remote, out deviceModel))
                    {
                        var sendArray = ReplaceSessionIDFromArray(
                            Encoding.Default.GetBytes(deviceModel.SessionID.ToCharArray()), reciveData);
                        sendArray[0] = (byte)MsgType.C_NewChat;
                        UDPHelper.Send(sendArray, device.Key);
                    }
                    break;
                case MsgType.S_SendMsg://发送消息给SessionID
                    Console.WriteLine(remote + ": " + byteToHexStr(reciveData));
                    sessionID = GetSessionIDFromReciveArray(reciveData);
                    idStr = Encoding.Default.GetString(sessionID);
                    var deviceTmp = deviceList.Where(x => x.Value.SessionID == idStr).FirstOrDefault();
                    if(deviceTmp.Key!=null && deviceList.TryGetValue(remote, out deviceModel))
                    {
                        var sendArray = ReplaceSessionIDFromArray(
                            Encoding.Default.GetBytes(deviceModel.SessionID.ToCharArray()), reciveData);//将数据包中的接收者替换成发送者
                        sendArray[0] = (byte)MsgType.C_ReciveMessage;//修改消息头
                        UDPHelper.Send(sendArray, deviceTmp.Key);
                    }
                    break;
                case MsgType.C_NewChat:
                    NewChatEvent?.Invoke(reciveData);
                    break;
                case MsgType.C_ReciveMessage:
                    ReciveMessageEvent?.Invoke(reciveData);
                    break;
                default:
                    ReciveSysInfoEvent?.Invoke(reciveData);
                    break;
            }
        }
        /// <summary>
        /// 从接收到的数组中提取sessionid
        /// </summary>
        /// <param name="reciveArray"></param>
        /// <param name="idOffset"></param>
        /// <returns></returns>
        public byte[] GetSessionIDFromReciveArray(byte[] reciveArray,int idOffset=1)
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
        public byte[] SetSessionIDIntoSendArray(byte[] sessionID,byte[] sendArray)
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
        public byte[] ReplaceSessionIDFromArray(byte[] replaceID,byte[] array,int idOffset = 1)
        {
            for (int i = 0; i < replaceID.Length; i++)
            {
                array[i + idOffset] = replaceID[i];
            }
            return array;
        }
        // <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string byteToHexStr(byte[] bytes)
        {
            try
            {
                string returnStr = "?";
                if (bytes != null)
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        returnStr += bytes[i].ToString("X2") + " ";
                    }
                }
                return returnStr;
            }
            catch { }
            return "noop";
        }
    }
    public class ProcessModel
    {
        public string SessionID { get; set; }
        public byte[] SendArray { get; set; }
    }
}
