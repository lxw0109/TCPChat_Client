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
        //通过该对象使用MainWindow中的各个函数.
        MainWindow win;

        public Login(MainWindow obj)
        {
            InitializeComponent();
            this.win = obj;
            //没有调用thread_Closed啊?
            this.Closed += new EventHandler(this.win.thread_Closed);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = (Button)sender;
            switch (bt.Tag.ToString())
            {                
                case "login":
                    {
                        this.Hide();
                        UserAndGroup uag = new UserAndGroup();
                        uag.Show();

                        // 发送上线包
                        Message msg = new Message();
                        msg.FromUserName = "Charlie";
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
                    break;
            }
        }
    }
}
