using HomeTicketWeb.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    /*********************************************************************************************/
    /* This file contains definitions which belongs to AdminPage.razor. Via this object,         */
    /* information there is saved into a DI obeject                                              */
    /*********************************************************************************************/
    public class AdminStatus : IShareDataModel
    {
        public AdminAddCategory AdminCat { get; set; }                                   // Object to save category related administration data

        public AdminAddSystem AdminSys { get; set; }                                     // Object to save system related adminstration data

        public AdminAddUser AdminUser { get; set; }                                      // Object to save user related administartion  data

        public TreeMenuItem ActMenu { get; set; }                                        // Actually selected menu on the page

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

    public class AdminAddUser : IShareDataModel
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

    public class AdminAddSystem : IShareDataModel
    {
        [Required]
        public string Name { get; set; }

        public void SetNull()
        {
            Name = null;
        }
    }

    public class AdminAddCategory : IShareDataModel
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
