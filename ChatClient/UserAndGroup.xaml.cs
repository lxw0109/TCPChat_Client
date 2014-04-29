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
        string usersName;
        string isOnline;
        string ifNewMsg;
        public string UsersName { get { return this.usersName; } set { this.usersName = value;} }
        public string IsOnline { get { return this.isOnline; } set { this.isOnline = value; } }
        public string IfNewMsg { get { return this.ifNewMsg; } set { this.ifNewMsg = value; } }
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
        // groupListview中的组的信息
        private List<GroupInList> groupNameList = new List<GroupInList>();
        // 用户的列表 绑定显示数据
        public ObservableCollection<UsersList> collection = new ObservableCollection<UsersList>();
        // 组的列表 绑定显示数据
        public ObservableCollection<string> groupCollection = new ObservableCollection<string>();

        // 属性
        public List<UserInList> UserInList_List
        {
            get { return this.userInList_List; }
        }
        public List<GroupInList> GroupNameList
        {
            get { return this.groupNameList; }
        }

        public UserAndGroup(MainWindow obj)
        {
            InitializeComponent();
            this.win = obj;
            this.Title = "Welcome, " + this.win.Ln.UserName + "!";
            this.Closed += new EventHandler(this.win.thread_Closed);
        }

        // 更新组列表
        public void updateGroupList(List<string> groupList)
        {
            // 两个list得同步, 都得先清空一下
            this.groupListview.Dispatcher.Invoke((Action)delegate()
            {
                groupCollection.Clear();
                groupNameList.Clear();
            });

            int length = groupList.Count;
            for (int i = 0; i < length; ++i)
            {
                string str = groupList[i];
                if (!string.IsNullOrEmpty(str))
                {
                    this.groupListview.Dispatcher.Invoke((Action)delegate()
                    {
                        groupCollection.Add(str);
                    });
                    this.groupNameList.Add(new GroupInList(str));
                }
            }
        }

        // 在组列表中, 添加组
        public void addIntoGroupList(string groupName)
        {
            if (!string.IsNullOrEmpty(groupName))
            {
                this.groupListview.Dispatcher.Invoke((Action)delegate()
                {
                    groupCollection.Add(groupName);
                });
                this.groupNameList.Add(new GroupInList(groupName));
            }
        }

        // 更新组列表中的某一项的信息
        public void updateGroupListItem(int index, int number)   // index要更新的项的索引
        {
            // 这个方法在GroupList中应该用不到
        }

        // 更新用户列表
        public void updateUserList(List<User> userList)
        {
            // 两个list得同步, 都得先清空一下
            this.listview.Dispatcher.Invoke((Action)delegate()
            {
                collection.Clear();
                this.userInList_List.Clear();
            });

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
                            UsersName = "   " + str,//"lxw",
                            IsOnline = (userList[i].IsOnline == 1) ? " " : "   不在线", // 不在线才显示"N"
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
                    string name = collection[index].UsersName;
                    string online = collection[index].IsOnline;
                    collection[index] = new UsersList
                        {
                            UsersName = name,
                            IsOnline = online,
                            IfNewMsg = "   " + number.ToString()
                        };
                    //collection[index].IfNewMsg = number.ToString();
                }
                else 
                {
                    string name = collection[index].UsersName;
                    string online = collection[index].IsOnline;
                    collection[index] = new UsersList
                    {
                        UsersName = name,
                        IsOnline = online,
                        IfNewMsg = ""
                    };
                    //collection[index].IfNewMsg = "";
                }
            });
        }

        // 发送 创建/加入 群的请求包
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.groupNameTextBox.Text.Trim()))
            {
                System.Windows.MessageBox.Show("请输入合法的群名!");
            }
            else
            {
                string input = this.groupNameTextBox.Text;
                this.groupNameTextBox.Text = "";
                
                // 往服务器发送这个消息
                Message msg = new Message();
                msg.FromUserName = this.win.Ln.UserName;//"Charlie";
                //msg.ToUserName = "";    // 无所谓
                msg.DateLine = DateTime.Now.ToString();
                msg.Type = 10;   // 文字消息
                msg.GroupName = input;
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

        // 必须使用属性才行
        public ObservableCollection<UsersList> Collection
        { get { return collection; } }

        public ObservableCollection<string> GroupCollection
        { get { return groupCollection; } }

        
        // 选择某个用户, 设置并打开窗口
        private void listview_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = sender as ListView;
            int index = lv.SelectedIndex;
            if (index < 0)
                return;
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

        private void grouplistview_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = sender as ListView;
            int index = lv.SelectedIndex;
            if (index < 0)
                return;
            if (this.groupNameList[index].ComGroupWin != null) //已经打开了
            {
                //这句可以不要
                MessageBox.Show("群会话窗口已经打开");
            }
            else
            {
                ComunicateGroup groupWin = new ComunicateGroup(this.win, index);
                
                this.groupNameList[index].ComGroupWin = groupWin;
                
                // 更新群成员列表
                // 1.发送 type == 12 的包, 返回成员列表
                // 往服务器发送这个消息
                Message msg = new Message();
                msg.FromUserName = this.win.Ln.UserName;//"Charlie";
                //msg.ToUserName = "";    // 无所谓
                msg.DateLine = DateTime.Now.ToString();
                msg.Type = 12;   // 文字消息
                msg.GroupName = this.groupNameList[index].GroupName;

                try
                {
                    this.win.Socket.Send(msg);
                }
                catch (Exception ecp)
                {
                    System.Windows.MessageBox.Show(ecp.Message, "错误");
                    return;
                }

                groupWin.Show();
            }
        }
    }
}
