using HomeTicketWeb.Components;
using HomeTicketWeb.Model;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Pages
{
    /*********************************************************************************************/
    /* This class is for the login page methods and properties (MainPage.razor)                  */
    /*********************************************************************************************/
    public partial class MainPage : ComponentBase
    {
        /*=======================================================================================*/
        /* Variables and objects                                                                 */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Parameters                                                                            */
        /*---------------------------------------------------------------------------------------*/

        /*---------------------------------------------------------------------------------------*/
        /* Get cascaded values                                                                   */
        /*---------------------------------------------------------------------------------------*/
        [CascadingParameter] MainLayout Layout { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Private, local variables and objects                                                  */
        /*---------------------------------------------------------------------------------------*/
        private LoginInfo Info = new LoginInfo();

        /*=======================================================================================*/
        /* Methods                                                                               */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Function name: Login                                                                  */
        /*                                                                                       */
        /* Description:                                                                          */
        /* It is a just a dummy function for testing Blazor components                           */
        /*---------------------------------------------------------------------------------------*/
        private void Login()
        {
            // Accept some dummy users
            if (Info.UserName == "God" && Info.Password == "admin")
            {
                User.UserName = Info.UserName;
                User.Role = UserRole.Admin;
                User.Email = "god@izé.com";
            }
            else if (Info.UserName == "Béla" && Info.Password == "user")
            {
                User.UserName = Info.UserName;
                User.Role = UserRole.User;
                User.Email = "bela@izé.com";
            }
            else
            {
                // If ID is wrong, then give a warning
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Invalid username or password", "Your username or password is invalid, sorry you can't get in :-(", AlertBox.AlertBoxType.Warning);
            }

            // Save login value to share it across pages
            Info = new LoginInfo();
            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: Logoff                                                                 */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Dummy logoff, clear variables in memory                                               */
        /*---------------------------------------------------------------------------------------*/
        private void Logoff()
        {
            // Set UserInfo to null (thus become unatuhorized)
            User.UserName = null;
            User.Role = null;
            User.Email = null;
            Info = new LoginInfo();

            // Close all opened applications
            if (Layout != null)
                if (Layout.Bar != null)
                    Layout.Bar.ClearOpenedApp();

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: ValidateError                                                          */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Username and/or password is/are missing                                               */
        /*---------------------------------------------------------------------------------------*/
        private void ValidateError()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Missing login credentials", "Username and password fields are mandatory!", AlertBox.AlertBoxType.Info);
        }
    }
}
