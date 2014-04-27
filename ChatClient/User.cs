using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatClient
{
    public class User
    {
        public int UserID;
        string userName;

        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }
        int isOnline;

        public int IsOnline
        {
            get { return isOnline; }
            set { isOnline = value; }
        }
        int hasUnreadMessage;

        public int HasUnreadMessage
        {
            get { return hasUnreadMessage; }
            set { hasUnreadMessage = value; }
        }
        string iP;

        public string IP
        {
            get { return iP; }
            set { iP = value; }
        }

    }
}
