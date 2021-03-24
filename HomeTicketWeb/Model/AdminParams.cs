using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public class TreeMenuItem
    {
        public string Section { get; set; }

        public string Title { get; set; }

        public bool Selected { get; set; } = false;

        public string CheckSelected()
        {
            if (Selected)
                return "active";
            return "";
        }
    }

    public class AdminAddUser
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public void SetNull()
        {
            UserName = null;
            Email = null;
            Password = null;
        }
    }

    public class AdminAddSystem
    {
        [Required]
        public string Name { get; set; }

        public void SetNull()
        {
            Name = null;
        }
    }

    public class AdminAddCategory
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string System { get; set; }

        public void SetNull()
        {
            Name = null;
            System = null;
        }
    }
}
