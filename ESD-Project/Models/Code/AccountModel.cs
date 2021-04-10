using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESD_Project.Models.Code
{
    public class AccountModel
    {
        private ModelESD db = new ModelESD();

        public User GetByID(string username)
        {
            return db.Users.SingleOrDefault(x => x.Username == username);
        }

        public List<string> GetListCreadentials(string userName)
        {
            var user = db.Users.FirstOrDefault(x => x.Username == userName);
            var data = (from a in db.Credentials
                        join b in db.GroupMembers on a.GroupId equals b.GroupId
                        join c in db.Roles on a.RoleId equals c.RoleId
                        where b.GroupId == user.GroupId
                        select new
                        {
                            RoleId = a.RoleId,
                            GroupId = a.GroupId
                        }).AsEnumerable().Select(x => new Credential()
                        {
                            RoleId = x.RoleId,
                            GroupId = x.GroupId
                        });
            return data.Select(x => x.RoleId).ToList();
        }

        public int Login(string username, string password) {
            var result = db.Users.SingleOrDefault(x => x.Username == username);
            if (result == null)
            {
                return 0;
            }
            else 
            {
                if (result.AccountStatus == false)
                {
                    return -1;
                }
                else 
                {
                    if (result.Password == password)
                    {
                        return 1;
                    }
                    else
                    {
                        return -2;
                    }
                }   
            }
        }
    }
}