using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public class TicketListElem
    {
        public int Id { get; set; }

        public string Reference { get; set; }

        public string Status { get; set; }

        public DateTime Time { get; set; }

        public string Title { get; set; }

        public Category Category { get; set; }

        public SystemElem System { get; set; }

        public User User { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public SystemElem System { get; set; }
    }

    public class SystemElem
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }
    }
}
