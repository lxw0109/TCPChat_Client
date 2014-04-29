using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
//改动的地方 //lxw
namespace ChatClient
{
    public abstract class SocketControl
    {
        // 客户端不用该数组
        public Socket[] ServerSockets = new Socket[10];
        // 与服务器通信的socket 所有的client只与server通信.
        public Socket ClientSockets;

        public abstract void Access(string IP, System.Action AccessAciton);


        //发送消息的函数
        public void Send(Message msg)
        {
            //Socket.Connected: 指示 Socket 在最近操作时是否连接到远程资源。
            if (ClientSockets.Connected == false)
            {
                throw new Exception("还没有建立连接, 不能发送消息");
            }
            Byte[] msag = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
            ClientSockets.BeginSend(msag, 0, msag.Length, SocketFlags.None,
                ar =>
                {
                    try
                    {
                        //lxw
                        ClientSockets.EndSend(ar);
                    }
                    catch 
                    {
                        throw new Exception("Send Error-lxw");
                    }
                }, null);
        }

        //接受函数
        // ReceiveAction: 大的switch
        public void Receive(System.Action AccessAction, System.Action<Message> ReceiveAction, System.Action<string> LogAction)
        {
            //如果消息超过1024个字节, 收到的消息会分为(总字节长度/1024 +1)条显示
            Byte[] msg = new byte[10240];
            //异步的接受消息
            ClientSockets.BeginReceive(msg, 0, msg.Length, SocketFlags.None,
                ar =>
                {
                    //对方断开连接时, 这里抛出Socket Exception
                    try
                    {
                        ClientSockets.EndReceive(ar);
                        //ReceiveAction(Encoding.UTF8.GetString(msg).Trim('\0', ' '));
                        Message msag = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(msg).Trim('\0', ' '));
                        msag.FromIP = ClientSockets.RemoteEndPoint.ToString().Split(':')[0];
                        ReceiveAction(msag);
                        Receive(AccessAction, ReceiveAction, LogAction);
                    }
                    catch
                    {
                        LogAction("服务器" + ClientSockets.RemoteEndPoint.ToString() + "断开连接！");
                        //Access(MainWindow.ServerIP, AccessAction);
                    }
                }, null);
        }
    }

    public class ClientSocket : SocketControl
    {
        //客户端重载Access函数
        public override void Access(string IP, System.Action AccessAciton)
        {
            ClientSockets = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientSockets.Bind(new IPEndPoint(IPAddress.Any, 6331));

            //服务器的IP和端口
            IPEndPoint serverIP;
            try
            {
                serverIP = new IPEndPoint(IPAddress.Parse(IP), 6333);
            }
            catch
            {
                throw new Exception(String.Format("{0}不是一个有效的IP地址!", IP));
            }

            //客户端只用来向指定的服务器发送信息,不需要绑定本机的IP和端口,不需要监听
            try
            {
                base.ClientSockets.BeginConnect(serverIP, ar =>
                {
                    try
                    {
                        //lxw
                        base.ClientSockets.EndConnect(ar);

                        // connect之后就调用AccessAciton(); 
                        // 在AccessAciton()中调用Receive(),然后就一直Receive()下去;
                        AccessAciton();
                    }
                    catch
                    {
                       //throw new Exception("长时间没有连接成功");
                    }
                }, null);
            }
            catch
            {
                throw new Exception(string.Format("尝试连接{0}不成功!", IP));
            }
        }
    }

}
