using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    /*********************************************************************************************/
    /* Class which helps to store infoirmation about a user                                      */
    /*********************************************************************************************/
    public class UserInfo
    {
        public string UserName { get; set; } = null;

        public UserRole? Role { get; set; } = null;

        public string Email { get; set; } = null;
    }

    /*********************************************************************************************/
    /* Possible user role types                                                                  */
    /*********************************************************************************************/
    public enum UserRole
    {
        User,
        Admin
    }
}
