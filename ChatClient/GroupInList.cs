using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatClient
{
    public class GroupInList
    {
        private string groupName;
        //private int index;
        private ComunicateGroup comGroupWin;
        private List<string> memberList;

        public string GroupName { get { return this.groupName; } set { this.groupName = value; } }
        //public string Index { get { return this.index; } set { this.index = value; } }
        public ComunicateGroup ComGroupWin { get { return this.comGroupWin; } set { this.comGroupWin = value; } }
        public List<string> MemberList { get { return this.memberList; } set { this.memberList = value; } }

        public GroupInList(string groupName)
        {
            this.groupName = groupName;
            this.comGroupWin = null;
            this.memberList = new List<string>();
        }
    }
}
