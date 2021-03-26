using HomeTicketWeb.Components;
using HomeTicketWeb.Model;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Pages.Admin
{
    /*********************************************************************************************/
    /* This class is for admin page methods and properties                                       */
    /*********************************************************************************************/
    public partial class AdminPage : ComponentBase  
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
        private List<TreeMenuItem> UserMenu = new List<TreeMenuItem>()                   // Feed TreeMenu component with data, what menu is required
        {
            new TreeMenuItem() { Title = "User adjustment", Section = "User management", Id = 1 },
            new TreeMenuItem() { Title = "User-Group assigments", Section = "User management", Id = 2 },
            new TreeMenuItem() { Title = "System adjustment", Section = "Sytem management", Id = 3 },
            new TreeMenuItem() { Title = "Category adjustment", Section = "Category management", Id = 4 },
            new TreeMenuItem() { Title = "Category-System assignments", Section = "Category management", Id = 5 },
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
            if (User.Role != UserRole.Admin || User.UserName == null)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Unathorized access", "You are not authorized. Login first if you want to do something", AlertBox.AlertBoxType.Error);

                if (Layout != null)
                    if (Layout.Bar != null)
                        Layout.Bar.RemoveOpenedApp("/admin");

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
            if (TMenu != null)
                if (TMenu.ActMenu != null)
                    AdminPageState.ActMenu = TMenu.ActMenu;

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
            if (Layout != null)
                if (Layout.Bar != null)
                    Layout.Bar.RemoveOpenedApp(NavManager.Uri.Substring(NavManager.BaseUri.Length - 1));
        }
    }
}
