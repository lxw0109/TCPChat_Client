using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatClient
{
    /// <summary>
    /// Comunicate.xaml 的交互逻辑
    /// </summary>
    public partial class Comunicate : Window
    {
        // 通过该对象使用MainWindow中的各个函数.
        MainWindow win;
        // 当前聊天的用户名(跟哪个用户在聊天) -- 每个窗体只能有一个聊天的对象
        string chatUser;
        int index;

        public Comunicate(MainWindow obj, int index)
        {
            InitializeComponent();
            this.win = obj;
            this.chatUser = this.win.Ln.Uag.UserInList_List[index].USER.UserName;
            this.index = index;
            this.Closed += new EventHandler(comClosed);
        }

        // 关闭该聊天窗口后要更新UserInList_List中的ComWin属性
        public void comClosed(object sender, EventArgs e)
        {
            this.win.Ln.Uag.UserInList_List[this.index].ComWin = null;
        }

        public void updateMsgTextBox(string content)
        {
            this.msgTextBox.Dispatcher.Invoke((Action)delegate()
            {
                this.msgTextBox.Text += content;
            });            
        }

        public void clearMsgTextBox()
        {
            this.msgTextBox.Dispatcher.Invoke((Action)delegate()
            {
                this.msgTextBox.Text = "";
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = (Button)sender;
            switch (bt.Tag.ToString())
            {
                case "send":
                    {
                        if (string.IsNullOrEmpty(this.inTextBox.Text.Trim()))
                        {
                            MessageBox.Show("请输入信息再发送!");
                        }
                        else 
                        {
                            // 此处需要用delegate吗?
                            string input = this.inTextBox.Text;
                            this.inTextBox.Text = "";
                            this.msgTextBox.Text += this.win.Ln.UserName + "  " + DateTime.Now.ToString() + " :\r\n" 
                                + input + "\r\n";                            
                            
                            // 往服务器发送这个消息
                            //type , fromUserName , toUserName, DateLine , messageContent
                            Message msg = new Message();
                            // lxw
                            msg.FromUserName = this.win.Ln.UserName;//"Charlie";
                            msg.ToUserName = this.chatUser;//"Lee";
                            msg.DateLine = DateTime.Now.ToString();
                            msg.Type = 5;   // 文字消息
                            msg.GroupName = "633";
                            msg.MessageContent = input;

                            try
                            {
                                this.win.Socket.Send(msg);
                            }
                            catch (Exception ecp)
                            {
                                MessageBox.Show(ecp.Message, "错误");
                                return;
                            }
                        }
                    }
                    break;
                case "file":
                    { 
                    }
                    break;
                case "history":
                    { 
                    }
                    break;
            }
        }
    }
}
