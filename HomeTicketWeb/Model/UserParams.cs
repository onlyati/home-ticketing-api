using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public class UserStatus : IShareDataModel
    {
        public TreeMenuItem ActMenu { get; set; }

        public UserStatus()
        {
            ActMenu = new TreeMenuItem();
        }

        public void SetNull()
        {
            if (ActMenu != null)
                ActMenu.SetNull();
        }
    }
}
