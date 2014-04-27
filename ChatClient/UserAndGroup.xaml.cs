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
    /// UserAndGroup.xaml 的交互逻辑
    /// </summary>
    public partial class UserAndGroup : Window
    {
        // 通过该对象使用MainWindow中的各个函数.
        MainWindow win;
        // Just for trial.
        private int number = 0;


        public UserAndGroup(MainWindow obj)
        {
            InitializeComponent();
            this.win = obj;
            this.Closed += new EventHandler(this.win.thread_Closed);
        }

        public void updateUserList(string[] userNameList)
        {
            int length = userNameList.Length;
            for (int i = 0; i < length; ++i)
            {
                this.listview.Dispatcher.Invoke((Action)delegate()
                {
                    this.listview.Items.Add(userNameList[i]);
                });
            }
        }

        // Simulate adding elements into the listview.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ++number;
            this.listview.Items.Add(number);
        }

        // When choose someone, then open the window for communicating.
        private void listview_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Comunicate com = new Comunicate();
            com.Show();
        }
    }
}
