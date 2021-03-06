﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Threading;
using System.IO;

using System.Windows.Forms;

//改动的地方 //lxw

namespace ChatClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketControl socket;

        System.Action<Message> ReceiveAction;
        System.Action AccessAction;
        System.Action<string> LogAction;

        Thread listenThread;
        public static string ServerIP;
        FileStream MyFileStream;
        string filepath = "";
        int Size;
        //lxw
        Login ln;

        //属性
        public SocketControl Socket
        {
            get { return this.socket; }
        }
        public Login Ln
        {
            get { return this.ln; }
        }
        
        
        public MainWindow()
        {
            InitializeComponent();

            ServerIP = IPTB.Text;
            this.Closed += new EventHandler(thread_Closed);

            ActionInit();//建立托管
            this.Title = "Charlie";

            ThreadStart Listen = new ThreadStart(this.ListenStart);//监听
            listenThread = new Thread(Listen);
            listenThread.Start();

            //lxw
            ln = new Login(this);
            ln.Show();
        }

        //建立托管
        [STAThread]
        public void ActionInit()
        {
            //建立连接后准备接受
            AccessAction = () =>
            {
                this.Dispatcher.Invoke((Action)delegate()
                {
                    String friendIP = socket.ClientSockets.RemoteEndPoint.ToString();
                    InfoTB.Text += String.Format("连接成功. 对方IP:{0} \n", friendIP);

                    try
                    {
                        socket.Receive(AccessAction,ReceiveAction, LogAction);
                    }
                    catch (Exception exp)
                    {
                        System.Windows.MessageBox.Show(exp.Message, "错误");
                    }
                });

            };

            //接收，集中处理JSON对象
            #region ReceiveAction = msg => {...}
            ReceiveAction = msg =>
            {
                User user = new User();
                // lxw ADD HERE.
                switch (msg.Type)
                {
                    #region case 1: 用户列表
                    case 1://用户列表[接收]
                        {
                            /*InfoTB.Dispatcher.Invoke((Action)delegate()
                            {
                                UpdateGetMsgTextBox(false, msg.MessageContent);
                            });*/                            
                            /*string[] userNameList = msg.MessageContent.Split(',');
                            ln.Uag.updateUserList(userNameList);*/

                            // 接收到的msg.MessageContent是一个List<User>的JSON字符串
                            List<User> userList = JsonConvert.DeserializeObject<List<User>>(msg.MessageContent);
                            ln.Uag.updateUserList(userList);
                        }
                        break;
                    #endregion

                    #region case 3: 查询是否在线的请求
                    case 3://查询是否在线的请求[接收该包 then 发送新包]
                        {
                            // 收到 type == 3后,发送type == 4(在线请求的回应)的包
                            Message newmsg = new Message();
                            newmsg.FromUserName = this.ln.UserName;//"Charlie";
                            //newmsg.ToUserName = "Lee";
                            //newmsg.DateLine = DateTime.Now.ToString();
                            newmsg.Type = 4;    //在线请求的回应
                            //newmsg.MessageContent = "在线";
                            try
                            {
                                this.socket.Send(newmsg);
                            }
                            catch (Exception ecp)
                            {
                                System.Windows.MessageBox.Show(ecp.Message, "错误");
                                return;
                            }
                        }
                        break;
                    #endregion

                    #region case 5: 接收文字消息
                    case 5://接收文字消息
                        {
                            // 1.显示出来[与该FromUserName的聊天窗口已经打开] 或者 2.添加到FromUserName的MsgList中[还没有打开聊天窗口]
                            int index = 0;
                            UserInList fromUser = null;
                            getUserIndex(ref fromUser, ref index, msg.FromUserName);
                            if (fromUser.ComWin == null)     // 2. 没有打开聊天窗口
                            {
                                fromUser.MsgList.Add(msg);
                                // UserAndGroup窗体中提示有未读消息
                                this.ln.Uag.updateUserListItem(index, fromUser.MsgList.Count);
                            }
                            else    // 1. 已经打开聊天窗口
                            {
                                fromUser.ComWin.updateMsgTextBox(msg.FromUserName + "  " + msg.DateLine + " :\r\n" +
                                                            msg.MessageContent + "\r\n");
                            }                             
                        }
                        break;
                    #endregion

                    #region case 6: 群消息
                    case 6:// 群消息
                        {
                            int index = 0;
                            GroupInList fromGroup = null;
                            //index 对于群聊天来说暂时没有使用
                            getGroupIndex(ref fromGroup, ref index, msg.GroupName);
                            if (fromGroup.ComGroupWin != null)     // 已经打开群聊天窗口
                            {
                                fromGroup.ComGroupWin.updateMsgTextBox(msg.FromUserName + "  " + msg.DateLine + " :\r\n" +
                                                            msg.MessageContent + "\r\n");
                            }

                            #region 暂时不处理
                            else    // 没有打开群聊天窗口
                            {
                                // 暂时不处理
                            }
                            #endregion
                        }
                        break;
                    #endregion

                    #region case 7: 对"发送文件请求"进行处理, 即发送回应
                    case 7:// 对"发送文件请求"进行处理, 即发送回应
                        {
                            /*InfoTB.Dispatcher.Invoke((Action)delegate()
                            {
                                UpdateGetMsgTextBox(false, msg.FromUserName + "准备向您发送文件" + msg.MessageContent);
                            });*/
                            
                            Message newmsg = new Message();
                            newmsg.FromUserName = this.ln.UserName;//"Charlie";
                            newmsg.ToUserName = msg.FromUserName;//"Lee";
                            newmsg.DateLine = DateTime.Now.ToString();
                            newmsg.Type = 8; //文件请求的应答
                            newmsg.MessageContent = "拒绝";
                            newmsg.FilePath = msg.FilePath;

                            if (
                                System.Windows.MessageBox.Show(string.Format("是否接收文件 {0}，来自 {1}。", msg.MessageContent, msg.FromUserName),
                                                "接收文件",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question)
                                == 
                                MessageBoxResult.Yes
                                )
                            {
                                newmsg.MessageContent = "同意";
                            }

                            try
                            {
                                socket.Send(newmsg);
                            }
                            catch (Exception ecp)
                            {
                                System.Windows.MessageBox.Show(ecp.Message, "错误");
                                return;
                            }
                        }
                        break;
                    #endregion

                    #region case 8: 文件请求回应
                    case 8://文件请求回应
                        {
                            // 查找要处理的用户
                            int index = 0;
                            UserInList fromUser = null;
                            getUserIndex(ref fromUser, ref index, msg.FromUserName);
                            
                             if (msg.MessageContent == "同意")
                            {
                                /*InfoTB.Dispatcher.Invoke((Action)delegate()
                                {
                                    UpdateGetMsgTextBox(false, "对方同意接受文件！");
                                });*/

                                try
                                {
                                    // 提示对方同意接收
                                    try
                                    {
                                        if (fromUser.ComWin == null)     // 2. 没有打开聊天窗口
                                        {
                                            fromUser.MsgList.Add(msg);
                                            // UserAndGroup窗体中提示有未读消息
                                            this.ln.Uag.updateUserListItem(index, fromUser.MsgList.Count);
                                        }
                                        else    // 1. 已经打开聊天窗口
                                        {
                                            fromUser.ComWin.updateMsgTextBox(msg.FromUserName + "  " + msg.DateLine + " :\r\n" +
                                                                        "同意接收文件" + "\r\n");
                                        }
                                    }
                                    catch
                                    {
                                        throw new Exception("显示提示信息出错.");
                                    }

                                    //创建一个文件对象
                                    FileInfo EzoneFile = new FileInfo(msg.FilePath);
                                    //打开文件流  
                                    FileStream EzoneStream = EzoneFile.OpenRead();
                                    int PacketSize = 1024;
                                    byte[] data = new byte[PacketSize];

                                    int PacketCount = (int)(EzoneStream.Length / PacketSize);
                                    //最后一个包的大小  
                                    int LastDataPacket = (int)(EzoneStream.Length - ((long)(PacketSize * PacketCount)));

                                    Message Filemsg = new Message();
                                    Filemsg.FromUserName = this.Ln.UserName;//"Charlie";
                                    Filemsg.ToUserName = msg.FromUserName;//"Lee";
                                    Filemsg.DateLine = DateTime.Now.ToString();
                                    Filemsg.Type = 9; //文件内容
                                    Filemsg.FilePath = msg.FilePath;
                                    Filemsg.MessageID = PacketCount + 1;//将这个作为文件块数量传递

                                    for (int i = 0; i < PacketCount; i++)
                                    {
                                        EzoneStream.Read(data, 0, data.Length);
                                        Filemsg.MessageContent = Encoding.Default.GetString(data);
                                        socket.Send(Filemsg);
                                        Thread.Sleep(100);
                                    }
                                    if (LastDataPacket != 0)
                                    {
                                        data = new byte[LastDataPacket];
                                        EzoneStream.Read(data, 0, data.Length);
                                        Filemsg.MessageContent = Encoding.Default.GetString(data);
                                        socket.Send(Filemsg);
                                    }

                                    EzoneStream.Close();

                                    // 提示文件发送完毕
                                    try
                                    {
                                        if (fromUser.ComWin == null)     // 2. 没有打开聊天窗口
                                        {
                                            fromUser.MsgList.Add(msg);
                                            // UserAndGroup窗体中提示有未读消息
                                            this.ln.Uag.updateUserListItem(index, fromUser.MsgList.Count);
                                        }
                                        else    // 1. 已经打开聊天窗口
                                        {
                                            fromUser.ComWin.updateMsgTextBox(msg.FromUserName + "  " + msg.DateLine + " :\r\n" +
                                                                        "文件发送完毕" + "\r\n");
                                        }
                                    }
                                    catch
                                    {
                                        throw new Exception("显示提示信息出错.");
                                    }
                                }
                                catch (Exception ee)
                                { }
                            }
                            else
                            {
                                /*InfoTB.Dispatcher.Invoke((Action)delegate()
                                {
                                    UpdateGetMsgTextBox(false, msg.MessageContent);
                                });*/

                                //提示对方拒绝接收文件
                                try
                                {
                                    if (fromUser.ComWin == null)     // 2. 没有打开聊天窗口
                                    {
                                        fromUser.MsgList.Add(msg);
                                        // UserAndGroup窗体中提示有未读消息
                                        this.ln.Uag.updateUserListItem(index, fromUser.MsgList.Count);
                                    }
                                    else    // 1. 已经打开聊天窗口
                                    {
                                        fromUser.ComWin.updateMsgTextBox(msg.FromUserName + "  " + msg.DateLine + " :\r\n" +
                                                                    "拒绝接收文件" + "\r\n");
                                    }
                                }
                                catch
                                {
                                    throw new Exception("显示提示信息出错.");
                                }
                            }
                        }
                        break;
                    #endregion

                    #region case 9: 文件内容
                    case 9://文件内容
                        try
                        {
                            byte[] dataRecv = Encoding.Default.GetBytes(msg.MessageContent);
                            
                            if (filepath == "" || filepath != msg.FilePath) // 不是同一个文件,则新开一个文件(写入该文件)
                            {
                                ThreadStart ts = new ThreadStart(recvFileName);
                                Thread thread = new Thread(ts);
                                thread.SetApartmentState(ApartmentState.STA);
                                thread.Start();                                

                                //SaveFileDialog sfd = new SaveFileDialog();
                                //sfd.ShowDialog();
                                
                                // 同步
                                thread.Join();

                                // FileMode这一个字段{就决定了,能否支持多文件传输.}   还是有问题的(因为filepath和Size的共享问题).
                                //MyFileStream = new FileStream(msg.FilePath + "1", FileMode.Create, FileAccess.Write);                                
                                MyFileStream = new FileStream(this.filepath, FileMode.OpenOrCreate, FileAccess.Write);

                                this.filepath = msg.FilePath;    // 上一次写入的文件, 该变量多个线程共享
                                Size = msg.MessageID;   // 该变量多个线程共享
                            }
                            else //同一个文件
                            {
                                // 什么都不做,继续接收即可
                            }
                            MyFileStream.Write(dataRecv, 0, dataRecv.Length);
                            Size--;

                            if (Size == 0)
                            {
                                MyFileStream.Close();
                                /*InfoTB.Dispatcher.Invoke((Action)delegate()
                                {
                                    UpdateGetMsgTextBox(false, msg.FilePath + "接受完成！");
                                });*/

                                // 查找要处理的用户
                                int index = 0;
                                UserInList fromUser = null;
                                getUserIndex(ref fromUser, ref index, msg.FromUserName);
                                try
                                {
                                    if (fromUser.ComWin == null)     // 2. 没有打开聊天窗口
                                    {
                                        fromUser.MsgList.Add(msg);
                                        // UserAndGroup窗体中提示有未读消息
                                        this.ln.Uag.updateUserListItem(index, fromUser.MsgList.Count);
                                    }
                                    else    // 1. 已经打开聊天窗口
                                    {
                                        fromUser.ComWin.updateMsgTextBox(msg.FromUserName + "  " + msg.DateLine + " :\r\n" +
                                                                    "文件接收完成!" + "\r\n");
                                    }
                                }
                                catch
                                {
                                    throw new Exception("显示提示信息出错.");
                                }
                            }
                        }
                        catch
                        {
                            /*InfoTB.Dispatcher.Invoke((Action)delegate()
                            {
                                UpdateGetMsgTextBox(false, "接受错误！");
                            });*/
                        }
                        break;
                    #endregion

                    #region case 10: 加群请求(群主接收)
                    case 10: // 加群请求(群主接收)
                        {
                            Message newmsg = new Message();
                            newmsg.FromUserName = this.ln.UserName;//"Charlie";
                            newmsg.ToUserName = msg.FromUserName;//"Lee";
                            newmsg.DateLine = DateTime.Now.ToString();
                            newmsg.Type = 11; //加群请求的应答
                            newmsg.MessageContent = "拒绝";
                            newmsg.IsJoin = 0;     // NOTE, 须有
                            newmsg.GroupName = msg.GroupName;

                            if (
                                System.Windows.MessageBox.Show(string.Format("是否同意 {0} 加入群 {1}。", msg.FromUserName, msg.MessageContent),
                                                "加群请求",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question)
                                ==
                                MessageBoxResult.Yes
                                )
                            {
                                newmsg.MessageContent = "同意";
                                newmsg.IsJoin = 1; // NOTE, 须有
                            }
                           

                            try
                            {
                                socket.Send(newmsg);
                            }
                            catch (Exception ecp)
                            {
                                System.Windows.MessageBox.Show(ecp.Message, "错误");
                                return;
                            }
                        }
                        break;
                    #endregion

                    #region case 11: 建群回应 | 加群回应
                    case 11://加群回应(请求者接收) | 建群回应(群主接收)
                        {
                            if (msg.MessageContent.IndexOf("成功建群") > 0)     // 建群回应(群主接收)
                            {
                                // 把msg.GroupName添加群列表
                                this.Ln.Uag.addIntoGroupList(msg.GroupName);
                            }
                            else    // 加群回应(请求者接收)
                            {
                                if (msg.IsJoin == 1)    // 同意加群
                                {
                                    System.Windows.MessageBox.Show(msg.MessageContent,
                                                "同意加群",
                                                MessageBoxButton.OK,
                                                MessageBoxImage.Information);
                                    // 把msg.GroupName添加群列表
                                    this.Ln.Uag.addIntoGroupList(msg.GroupName);
                                }
                                else    // 拒绝加群(请求者接收)
                                {
                                    System.Windows.MessageBox.Show(msg.MessageContent,
                                                "拒绝加群",
                                                MessageBoxButton.OK,
                                                MessageBoxImage.Information);
                                }
                            }
                        }
                        break;
                    #endregion
                        
                    #region case 12: 群成员请求的应答
                    case 12://群成员请求的应答
                        {
                            // 得到群成员列表
                            // 接收到的msg.MessageContent是一个List<string>的JSON字符串
                            List<string> memberList = JsonConvert.DeserializeObject<List<string>>(msg.MessageContent);
                            int length = this.Ln.Uag.GroupNameList.Count;
                            for(int i = 0; i < length; ++i)
                            {
                                if(this.Ln.Uag.GroupNameList[i].GroupName == msg.GroupName)
                                {
                                    this.Ln.Uag.GroupNameList[i].ComGroupWin.updateMemberList(memberList);
                                    break;
                                }
                            }
                        }
                        break;
                    #endregion
                }
            };
            #endregion

            //日志信息
            LogAction = msg =>
            {
                InfoTB.Dispatcher.Invoke((Action)delegate()
                {
                    UpdateGetMsgTextBox(false, msg);
                });
            };
        }

        // 文件另存为线程
        public void recvFileName()
        {
            //string fileName = (string)arg;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.ShowDialog();
            this.filepath = sfd.FileName;
        }
        

        // 得到当前是和哪个用户在聊天, 以及他在用户列表中的index
        public void getUserIndex(ref UserInList fromUser, ref int index, string srcUser)
        {
            foreach (UserInList uil in this.ln.Uag.UserInList_List)
            {
                if (uil.USER.UserName == srcUser)    // 找到要处理的用户
                {
                    fromUser = uil;
                    break;
                }
                index++;
            }
        }

        // 得到当前是和哪个用户在聊天, 以及他在用户列表中的index
        public void getGroupIndex(ref GroupInList fromGroup, ref int index, string groupName)
        {
            foreach (GroupInList gil in this.ln.Uag.GroupNameList)
            {
                if (gil.GroupName == groupName)    // 找到要处理的用户
                {
                    fromGroup = gil;
                    break;
                }
                index++;
            }
        }

        //启动监听(子线程)
        public void ListenStart()
        {
            InfoTB.Dispatcher.Invoke((Action)delegate()
            {
                UpdateGetMsgTextBox(false, "准备连接...");
            });
            this.socket = new ClientSocket();
            try
            {
                this.socket.Access(ServerIP, this.AccessAction);
            }
            catch (Exception ecp)
            {                
                InfoTB.Dispatcher.Invoke((Action)delegate()
                {
                    UpdateGetMsgTextBox(false, "错误");
                });
            }
        }
        

        //程序关闭，停止线程
        public void thread_Closed(object sender, EventArgs e)
        {
            listenThread.Abort();
            Environment.Exit(0);//释放进程
            System.Windows.Application.Current.Shutdown(-1);
        }

        private void UpdateGetMsgTextBox(bool sendMsg, string message)
        {
            string appendText;
            appendText = message;
            
            InfoTB.Text += appendText + "\n";         
        }


        // Login Button: 发送"上线包"
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //string message = txtSendMsg.Text.Trim();
            //if (string.IsNullOrEmpty(message))
            //{
            //    MessageBox.Show("消息内容不能为空!", "错误");
            //    txtSendMsg.Focus();
            //    return;
            //}
            Message msg = new Message();
            msg.FromUserName = "Charlie";
            msg.ToUserName = "Lee";
            msg.DateLine = DateTime.Now.ToString();
            msg.Type = 0;
            msg.GroupName = "633";
            msg.MessageContent = "Hello!";

            try
            {
                socket.Send(msg);
            }
            catch (Exception ecp)
            {
                System.Windows.MessageBox.Show(ecp.Message, "错误");
                return;
            }
        }

        // 发送"加群请求"
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Message msg = new Message();
            msg.FromUserName = "Charlie";
            msg.ToUserName = "Lee";
            msg.DateLine = DateTime.Now.ToString();
            msg.Type = 10;  //加群请求
            msg.GroupName = "633";
            msg.MessageContent = "Hello!";

            try
            {
                socket.Send(msg);
            }
            catch (Exception ecp)
            {
                System.Windows.MessageBox.Show(ecp.Message, "错误");
                return;
            }
        }

        // 发送 "发送文件请求"
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Message msg = new Message();
                    msg.FromUserName = "Charlie";
                    msg.ToUserName = "Lee";
                    msg.DateLine = DateTime.Now.ToString();
                    msg.Type = 7;   //发送文件请求
                    msg.MessageContent = ofd.FileName;
                    msg.FilePath = ofd.FileName;                    

                    try
                    {
                        socket.Send(msg);
                    }
                    catch (Exception ecp)
                    {
                        System.Windows.MessageBox.Show(ecp.Message, "错误");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }            
        }
    }
}
