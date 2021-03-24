using HomeTicketWeb.Model;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using HomeTicketWeb.Shared;
using HomeTicketWeb.Components;

namespace HomeTicketWeb.Pages.Dashboard
{
    public partial class Dashboard : ComponentBase
    {
        LoginInfo _loginInfo = new LoginInfo();
        string valami;

        [CascadingParameter] MainLayout Layout { get; set; }

        public List<string> lista = new List<string>()
        {
            new string("Egy"),
            new string("Kettő"),
            new string("Mi az hogy kettű??")
        };

        protected override void OnInitialized()
        {
            if(User.Role == null || User.UserName == null)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Unathorized access", "You are not authorized. Login first if you want to do something", AlertBox.AlertBoxType.Error);
                NavManager.NavigateTo("/");
            }
        }

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
            Layout.AlertBox.SetAlert("Login failed", "Missing login credentials", AlertBox.AlertBoxType.Info);
            Console.WriteLine("ShowAlert() befejezve");
        }

        public void TestAlert1()
        {
            Layout.AlertBox.SetAlert("Test alert", "Call with Onconfirmed info", AlertBox.AlertBoxType.Info, TestConfirm);
        }

        public void TestAlert2()
        {
            Layout.AlertBox.SetAlert("Test alert", "Call with Onconfirmed warning", AlertBox.AlertBoxType.Warning, TestConfirm);
        }

        public void TestAlert3()
        {
            Layout.AlertBox.SetAlert("Test alert", "Call with Onconfirmed error", AlertBox.AlertBoxType.Error, TestConfirm);
        }

        public void TestAlert4()
        {
            Layout.AlertBox.SetAlert("Test alert", "Call with Onconfirmed question", AlertBox.AlertBoxType.Question, DeleteFromLista, TestCancel);
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
