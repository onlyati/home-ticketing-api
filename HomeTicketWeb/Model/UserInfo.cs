using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public class UserInfo
    {
        public string UserName { get; set; } = null;

        public UserRole? Role { get; set; } = null;
    }

    public enum UserRole
    {
        User,
        Admin
    }
}
