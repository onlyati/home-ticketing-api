using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public class AdminAddUser
    {
        public string UserName { get; set; }

        public string Email { get; set; }
    }

    public class AdminAddSystem
    {
        public string Name { get; set; }
    }

    public class AdminAddCategory
    {
        public string Name { get; set; }

        public string System { get; set; }
    }
}
