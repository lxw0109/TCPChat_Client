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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        // 通过该对象使用MainWindow中的各个函数.
        MainWindow win;
        // 程序最主要的窗体.
        UserAndGroup uag;
        //属性
        public UserAndGroup Uag
        {
            get { return this.uag; }
        }

        public Login(MainWindow obj)
        {
            InitializeComponent();
            this.win = obj;

            this.Closed += new EventHandler(this.win.thread_Closed);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = (Button)sender;
            switch (bt.Tag.ToString())
            {
                #region login
                case "login":
                    {
                        string userName = this.userNameTextBox.Text.Trim();
                        if (string.IsNullOrEmpty(userName))
                        {
                            MessageBox.Show("请输入合法的用户名");
                        }
                        else
                        {
                            this.Hide();
                            uag = new UserAndGroup(this.win);
                            uag.Show();

                            /*//public int MessageID;
                            string fromUserName;
                            string toUserName;
                            //int isRead;
                            string dateLine;
                            int type;
                            string messageContent;
                            //string filePath;
                            string groupName;
                            //string fromIP;
                            //int isJoin;*/

                            // 发送上线包
                            Message msg = new Message();
                            // lxw
                            msg.FromUserName = userName;//"Charlie";
                            msg.ToUserName = "Lee";
                            msg.DateLine = DateTime.Now.ToString();
                            msg.Type = 0;
                            msg.GroupName = "633";
                            msg.MessageContent = "Hello!";

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
                #endregion
            }
        }
    }
}
