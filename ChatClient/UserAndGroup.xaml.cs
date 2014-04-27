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
//lxw
using System.Collections.ObjectModel;

namespace ChatClient
{
    // 为了listview的显示, 与GridView绑定数据
    public class UsersList
    {
        public string UsersName { get; set; }
        public string IsOnline { get; set; }
        public string IfNewMsg { get; set; }
    }
    /// <summary>
    /// UserAndGroup.xaml 的交互逻辑
    /// </summary>
    public partial class UserAndGroup : Window
    {
        // 通过该对象使用MainWindow中的各个函数.
        MainWindow win;
        // Just for trial.
        private int number = 0;
        // listview中的用户的信息
        private List<UserInList> userInList_List = new List<UserInList>();
        // 绑定显示数据
        public ObservableCollection<UsersList> collection = new ObservableCollection<UsersList>();
        
        // 属性
        public List<UserInList> UserInList_List
        {
            get { return this.userInList_List; }
        }

        public UserAndGroup(MainWindow obj)
        {
            InitializeComponent();
            this.win = obj;
            this.Closed += new EventHandler(this.win.thread_Closed);
        }

        // 更新用户列表
        public void updateUserList(List<User> userList)
        {
            int length = userList.Count;
            for (int i = 0; i < length; ++i)
            {
                string str = userList[i].UserName;
                if (!string.IsNullOrEmpty(str))
                {
                    this.listview.Dispatcher.Invoke((Action)delegate()
                    {
                        //this.listview.Items.Add(str);
                        collection.Add(new UsersList
                        {
                            UsersName = str,//"lxw",
                            IsOnline = (userList[i].IsOnline == 1) ? " " : "N", // 不在线才显示"N"
                            IfNewMsg = ""
                        });
                    });
                    this.userInList_List.Add(new UserInList(userList[i]));
                }
            }
        }

        // 更新用户列表中的某一项的信息
        public void updateUserListItem(int index, int number)   // index要更新的项的索引
        {
            this.listview.Dispatcher.Invoke((Action)delegate()
            {
                if (number != 0)
                {
                    collection[index].IfNewMsg = number.ToString();
                }
                else 
                {
                    collection[index].IfNewMsg = "";
                }
            });
        }

        // Simulate adding elements into the listview.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ++number;
            collection.Add(new UsersList
            {
                UsersName = "lxw",
                IsOnline = "Y",
                IfNewMsg = "*" + number.ToString()
            });
        }

        // 必须使用属性才行
        public ObservableCollection<UsersList> Collection
        { get { return collection; } }
        
        // 选择某个用户, 设置并打开窗口
        private void listview_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = sender as ListView;
            int index = lv.SelectedIndex;
            if (this.userInList_List[index].ComWin != null) //已经打开了
            {
                //这句可以不要
                MessageBox.Show("会话窗口已经打开");
            }
            else 
            {
                Comunicate com = new Comunicate(this.win, index);
                com.Show();
                
                // 离线消息处理                
                int length = this.userInList_List[index].MsgList.Count;
                if(length == 0) // 没有离线消息
                {
                }
                else    // 有离线消息
                {
                    // 清空
                    com.clearMsgTextBox();

                    for (int i = 0; i < length; ++i)
                    {
                        Message msg = this.userInList_List[index].MsgList[i];
                        com.updateMsgTextBox(msg.FromUserName + "  " + msg.DateLine + " :\r\n" +
                                             msg.MessageContent + "\r\n");
                    }
                    this.updateUserListItem(index, 0);  // 清除主界面中"新消息"提醒
                }
                this.userInList_List[index].ComWin = com;
            }            
        }
    }
}
