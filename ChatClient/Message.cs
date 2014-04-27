using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatClient
{
    public class Message
    {
        public int MessageID;
        string fromUserName;
        string toUserName;
        int isRead;
        string dateLine;
        int type;
        string messageContent;
        string filePath;
        string groupName;
        string fromIP;
        int isJoin;

        public string FromUserName
        {
            get { return fromUserName; }
            set { fromUserName = value; }
        }        

        public string ToUserName
        {
            get { return toUserName; }
            set { toUserName = value; }
        }

        public int IsRead
        {
            get { return isRead; }
            set { isRead = value; }
        }

        public string DateLine
        {
            get { return dateLine; }
            set { dateLine = value; }
        }
        
        public int Type
        {
            get { return type; }
            set { type = value; }
        }
        
        public string MessageContent
        {
            get { return messageContent; }
            set { messageContent = value; }
        }
        
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        public string GroupName
        {
            get { return groupName; }
            set { groupName = value; }
        }
        
        public string FromIP
        {
            get { return fromIP; }
            set { fromIP = value; }
        }
        
        public int IsJoin
        {
            get { return isJoin; }
            set { isJoin = value; }
        }
    }
}
