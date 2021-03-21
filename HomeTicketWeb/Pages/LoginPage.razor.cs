using HomeTicketWeb.Model;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Pages
{
    public partial class LoginPage : ComponentBase
    {
        LoginInfo _loginInfo = new LoginInfo();
        string valami;

        public async Task LoginRequest()
        {
            valami = $"{_loginInfo.UserName} - {_loginInfo.Password}";
        }
    }
}
