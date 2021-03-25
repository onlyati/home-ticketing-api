using HomeTicketWeb.Components;
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

        public int Id { get; set; }

        public bool Selected { get; set; } = false;

        public void SetNull()
        {
            Section = null;
            Title = null;
            Id = 0;
            Selected = false;
        }
    }

    public class AdminStatus
    {
        public AdminAddCategory AdminCat { get; set; }
        public AdminAddSystem AdminSys { get; set; }
        public AdminAddUser AdminUser { get; set; }
        public TreeMenuItem ActMenu { get; set; }

        public AdminStatus()
        {
            AdminCat = new AdminAddCategory();
            AdminSys = new AdminAddSystem();
            AdminUser = new AdminAddUser();
            ActMenu = new TreeMenuItem();
        }

        public void SetNull()
        {
            if (AdminCat != null)
                AdminCat.SetNull();
            if (AdminSys != null)
                AdminSys.SetNull();
            if (AdminUser != null)
                AdminUser.SetNull();
            if (ActMenu != null)
                ActMenu.SetNull();
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
