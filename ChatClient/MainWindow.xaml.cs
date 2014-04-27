using System;
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
        //属性
        public SocketControl Socket
        {
            get { return socket; }
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
            Login ln = new Login(this);
            ln.Show();
        }

        //建立托管
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
                        MessageBox.Show(exp.Message, "错误");
                    }
                });

            };

            //接收，集中处理JSON对象
            #region ReceiveAction = msg => {...}
            ReceiveAction = msg =>
            {
                User user = new User();

                switch (msg.Type)
                {
                    case 1://用户列表
                        InfoTB.Dispatcher.Invoke((Action)delegate()
                        {
                            UpdateGetMsgTextBox(false, msg.MessageContent);
                        });
                        break;
                        ////////////添加消息事件
                    case 11://加群回应
                        InfoTB.Dispatcher.Invoke((Action)delegate()
                        {
                            UpdateGetMsgTextBox(false, msg.MessageContent);
                        });
                        break;
                    case 7://发送文件请求
                        InfoTB.Dispatcher.Invoke((Action)delegate()
                        {
                            UpdateGetMsgTextBox(false, msg.FromUserName+"准备向您发送文件"+msg.MessageContent);
                        });

                        Message newmsg = new Message();
                        newmsg.FromUserName = "Charlie";
                        newmsg.ToUserName = "Lee";
                        newmsg.DateLine = DateTime.Now.ToString();
                        newmsg.Type = 8;
                        newmsg.MessageContent = "拒绝";
                        newmsg.FilePath = msg.FilePath;

                        if (MessageBox.Show(string.Format(
                        "是否接收文件 {0}，来自 {1}。",
                        msg.MessageContent,
                        msg.FromUserName),
                        "接收文件",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            newmsg.MessageContent = "同意";
                        }
                        try
                        {
                            socket.Send(newmsg);
                        }
                        catch (Exception ecp)
                        {
                            MessageBox.Show(ecp.Message, "错误");
                            return;
                        }

                        break;
                    case 8://文件请求回应
                        if (msg.MessageContent == "同意")
                        {
                            InfoTB.Dispatcher.Invoke((Action)delegate()
                            {
                                UpdateGetMsgTextBox(false, "对方同意接受文件！");
                            });
                            try
                            {
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
                                Filemsg.FromUserName = "Charlie";
                                Filemsg.ToUserName = "Lee";
                                Filemsg.DateLine = DateTime.Now.ToString();
                                Filemsg.Type = 9;
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
                            }
                            catch (Exception ee)
                            { }
                        }
                        else
                        {
                            InfoTB.Dispatcher.Invoke((Action)delegate()
                            {
                                UpdateGetMsgTextBox(false, msg.MessageContent);
                            });
                        }
                        break;
                    case 9://文件内容
                        try
                        {
                            byte[] dataRecv = Encoding.Default.GetBytes(msg.MessageContent);

                            if (filepath == "" || filepath != msg.FilePath)
                            {
                                MyFileStream = new FileStream(msg.FilePath+"1", FileMode.Create, FileAccess.Write);
                                filepath = msg.FilePath;
                                Size = msg.MessageID;
                            }
                            MyFileStream.Write(dataRecv, 0, dataRecv.Length);
                            Size--;

                            if (Size == 0)
                            {
                                MyFileStream.Close();
                                InfoTB.Dispatcher.Invoke((Action)delegate()
                                {
                                    UpdateGetMsgTextBox(false, msg.FilePath + "接受完成！");
                                });
                            }
                        }
                        catch
                        {
                            InfoTB.Dispatcher.Invoke((Action)delegate()
                            {
                                UpdateGetMsgTextBox(false,  "接受错误！");
                            });
                        }
                        break;
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
            Application.Current.Shutdown(-1);
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
                MessageBox.Show(ecp.Message, "错误");
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
                MessageBox.Show(ecp.Message, "错误");
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
                        MessageBox.Show(ecp.Message, "错误");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }            
        }
    }
}
