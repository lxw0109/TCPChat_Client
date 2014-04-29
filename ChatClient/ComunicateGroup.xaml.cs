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

using System.Windows.Forms;

using System.Collections.ObjectModel;

namespace ChatClient
{
    /// <summary>
    /// Comunicate.xaml 的交互逻辑
    /// </summary>
    public partial class ComunicateGroup : Window
    {
        // 通过该对象使用MainWindow中的各个函数.
        MainWindow win;
        // 当前聊天的群组
        string chatGroup;
        int index;
        // 组成员的列表 绑定显示数据
        public ObservableCollection<string> memberCollection = new ObservableCollection<string>();

        // 属性
        public ObservableCollection<string> MemberCollection
        { get { return memberCollection; } }

        // 更新组成员列表
        public void updateMemberList(List<string> memberList)
        {
            // 先清空一下
            this.memberListview.Dispatcher.Invoke((Action)delegate()
            {
                memberCollection.Clear();
            });

            int length = memberList.Count;
            for (int i = 0; i < length; ++i)
            {
                if (!string.IsNullOrEmpty(memberList[i]))
                {
                    this.memberListview.Dispatcher.Invoke((Action)delegate()
                    {
                        memberCollection.Add(memberList[i]);
                    });
                }
            }
        }

        public ComunicateGroup(MainWindow obj, int index)
        {
            InitializeComponent();
            this.win = obj;
            this.chatGroup = this.win.Ln.Uag.GroupNameList[index].GroupName;
            this.Title += ": \"" + this.chatGroup + "\"";
            this.index = index;
            this.Closed += new EventHandler(comGroupClosed);
        }

        // 关闭该聊天窗口后要更新GroupNameList中的ComGroupWin属性
        public void comGroupClosed(object sender, EventArgs e)
        {
            this.win.Ln.Uag.GroupNameList[this.index].ComGroupWin = null;
        }

        public void updateMsgTextBox(string content)
        {
            this.msgTextBox.Dispatcher.Invoke((Action)delegate()
            {
                this.msgTextBox.Text += content;
            });            
        }

        // 更新群成员列表
        public void updateMemberList()
        {
 
        }

        // 发送群消息
        private void Button_Click(object sender, RoutedEventArgs e)
        {
           if (string.IsNullOrEmpty(this.inTextBox.Text.Trim()))
            {
                System.Windows.MessageBox.Show("请输入信息再发送!");
            }
            else 
            {
                string input = this.inTextBox.Text;
                this.inTextBox.Text = "";
                this.msgTextBox.Text += this.win.Ln.UserName + "  " + DateTime.Now.ToString() + " :\r\n"
                    + input + "\r\n";

                // 往服务器发送这个消息
                Message msg = new Message();
                msg.FromUserName = this.win.Ln.UserName;//"Charlie";
                //msg.ToUserName = this.chatGroup;//"Lee";
                msg.DateLine = DateTime.Now.ToString();
                msg.Type = 6;   // 群文字消息
                msg.GroupName = this.chatGroup;
                msg.MessageContent = input;

                try
                {
                    this.win.Socket.Send(msg);
                }
                catch (Exception ecp)
                {
                    System.Windows.MessageBox.Show(ecp.Message, "错误");
                    return;
                }
            }
        }

        private void Button_click1(object sender, RoutedEventArgs e)
        {
            this.memberListview.Dispatcher.Invoke((Action)delegate()
            {
                MemberCollection.Add("lxw");
            });
            //this.memberListview.Items.Add("Lee");
        }
    }
}
