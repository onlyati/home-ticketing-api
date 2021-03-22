using HomeTicketWeb.Model;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeTicketWeb.Components;
using HomeTicketWeb.Shared;

namespace HomeTicketWeb.Pages
{
    public partial class LoginPage : ComponentBase
    {
        AlertBox box;
        LoginInfo _loginInfo = new LoginInfo();
        string valami;

        public List<string> lista = new List<string>()
        {
            new string("Egy"),
            new string("Kettő"),
            new string("Mi az hogy kettű??")
        };

        public void DeleteFromLista()
        {
            lista.Remove("Egy");
            StateHasChanged();
        }

        public void LoginRequest()
        {
            valami = $"{_loginInfo.UserName} - {_loginInfo.Password}";
        }

        public void ShowAlert()
        {
            Console.WriteLine("ShowAlert() elkezdve");
            box.SetAlert("Login failed", "Missing login credentials", AlertBox.AlertBoxType.Info);
            Console.WriteLine("ShowAlert() befejezve");
        }

        public void TestAlert1()
        {
            box.SetAlert("Test alert", "Call with Onconfirmed info", AlertBox.AlertBoxType.Info, TestConfirm);
        }

        public void TestAlert2()
        {
            box.SetAlert("Test alert", "Call with Onconfirmed warning", AlertBox.AlertBoxType.Warning, TestConfirm);
        }

        public void TestAlert3()
        {
            box.SetAlert("Test alert", "Call with Onconfirmed error", AlertBox.AlertBoxType.Error, TestConfirm);
        }

        public void TestAlert4()
        {
            box.SetAlert("Test alert", "Call with Onconfirmed question", AlertBox.AlertBoxType.Question, DeleteFromLista, TestCancel);
        }

        public void TestConfirm()
        {
            Console.WriteLine("Hej, lefutottam");
        }

        public void TestCancel()
        {
            Console.WriteLine("Hej, én is lefutottam");
        }
    }
}
