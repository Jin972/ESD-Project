using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESD_Project.Common
{
    [Serializable]
    public class UserLogin
    {
        public long ID { get; set; }

        public string Name { get; set; }

        public string Username { get; set; }

        public long? MajorID { get; set; }

        public int AvatarId { get; set; }

        public string GroupId { get; set; }
    }
}