using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatClient
{
    public class UserInList
    {
        // user对象: 需要userName 和 isOnline
        private User user;
        // 当前用户是否打开了与user的聊天窗口
        //private bool isOpenWin;
        // 与user的聊天窗口: 如果打开了的话为对象,未打开则为null
        private Comunicate comWin;
        // 存储消息: 主要针对未打开聊天窗口的情况
        private List<Message> msgList;  // 因为既要有时间又要有内容,所以List的类型须为Message

        public User USER { get { return this.user; } set { this.user = value; } }
        public Comunicate ComWin { get { return this.comWin; } set { this.comWin = value; } }
        public List<Message> MsgList { get { return this.msgList; } set { this.msgList = value; } }

        public UserInList(User user)
        {
            this.USER = user;
            //this.isOpenWin = false;
            this.ComWin = null;
            this.MsgList = new List<Message>();
        }
    }
}
