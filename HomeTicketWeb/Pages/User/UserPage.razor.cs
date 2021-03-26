using HomeTicketWeb.Components;
using HomeTicketWeb.Model;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Pages.User
{
    /*********************************************************************************************/
    /* This class is for the user setting methods and properties (UserPage.razor)                */
    /*********************************************************************************************/
    public partial class UserPage : ComponentBase
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
        [CascadingParameter] public MainLayout Layout { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Private, local variables and objects                                                  */
        /*---------------------------------------------------------------------------------------*/
        private TreeMenu TMenu;                                                          // Reference for TreeMenu component
        private List<TreeMenuItem> UserMenu = new List<TreeMenuItem>()                   // Feed treemenu with the required menupoints
        {
            new TreeMenuItem() { Title = "Display data", Section = "User management", Id = 1 },
            new TreeMenuItem() { Title = "Change your profile", Section = "User management", Id = 2 },
        };

        /*=======================================================================================*/
        /* Methods                                                                               */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Function name: OnInitialized                                                          */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Check authority during component initailization. If failed, navigate the login screen */
        /*---------------------------------------------------------------------------------------*/
        protected override void OnInitialized()
        {
            if (User.Role == null || User.UserName == null)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Unathorized access", "You are not authorized. Login first if you want to do something", AlertBox.AlertBoxType.Error);

                if (Layout != null)
                    if (Layout.Bar != null)
                        Layout.Bar.RemoveOpenedApp("/user");

                NavManager.NavigateTo("/");
            }
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: UpdateState                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Eventcallback for 'AfterClick' parameter of TreeMenu component. It runs after click   */
        /* has happened on an item in the menu                                                   */
        /*---------------------------------------------------------------------------------------*/
        public void UpdateState()
        {
            // Save the actual selected menu to the injected object
            if (TMenu != null)
                if (TMenu.ActMenu != null)
                    UserPageState.ActMenu = TMenu.ActMenu;

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: CloseWindow                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Eventcallback for 'WindowTitle' component. This defines what should happen if somebody*/
        /* click to the 'X' in the right corner                                                  */
        /*---------------------------------------------------------------------------------------*/
        private void CloseWindow()
        {
            // CLose and remove the application via 'TaskBar' component
            if (Layout != null)
                if (Layout.Bar != null)
                    Layout.Bar.RemoveOpenedApp(NavManager.Uri.Substring(NavManager.BaseUri.Length - 1));
        }
    }
}
