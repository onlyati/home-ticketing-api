using HomeTicketWeb.Components;
using HomeTicketWeb.Model;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

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
        private ChangeUser ChangeInfo = new ChangeUser();                                // Use as model for EditForm
        private List<Category> userCategories = new List<Category>();

        /*=======================================================================================*/
        /* Classes                                                                               */
        /*=======================================================================================*/
        private class ChangeUser
        {
            public string Email { get; set; }

            public string Password { get; set; }

            [Required]
            public string OldPassword { get; set; }
        }

        /*=======================================================================================*/
        /* Methods                                                                               */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Function name: OnInitialized                                                          */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Check authority during component initailization. If failed, navigate the login screen */
        /*---------------------------------------------------------------------------------------*/
        protected override async Task OnInitializedAsync()
        {
            bool check = await RefreshService.RefreshToken(js, User, Configuration["ServerAddress"], Http, NavManager);
            if (!check)
                CloseWindow();

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: UpdateState                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Eventcallback for 'AfterClick' parameter of TreeMenu component. It runs after click   */
        /* has happened on an item in the menu                                                   */
        /*---------------------------------------------------------------------------------------*/
        public async Task UpdateState()
        {
            // Save the actual selected menu to the injected object
            if (TMenu != null)
                if (TMenu.ActMenu != null)
                    UserPageState.ActMenu = TMenu.ActMenu;

            await LoadData();

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: LoadData()                                                             */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method loads every data, depends which menupoint is active                       */
        /*---------------------------------------------------------------------------------------*/
        private async Task LoadData()
        {
            /*-----------------------------------------------------------------------------------*/
            /* Load the first subpage (Display user data                                         */
            /*-----------------------------------------------------------------------------------*/
            if (UserPageState.ActMenu.Id == 1 || UserPageState.ActMenu.Id == 0)
            {
                // Refresh all data on display data panel
                var infoRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/user/info");
                if (infoRequest.StatusCode == HttpStatusCode.OK)
                {
                    var goodResponse = JsonSerializer.Deserialize<UserInfo>(await infoRequest.Content.ReadAsStringAsync());
                    User.UserName = goodResponse.UserName;
                    User.Email = goodResponse.Email;
                    User.Role = goodResponse.Role;

                    // Get categores which belongs to the user
                    var allCategoryRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/category/list/user?value={User.UserName}");
                    if (allCategoryRequest.StatusCode == HttpStatusCode.OK)
                    {
                        userCategories = JsonSerializer.Deserialize<List<Category>>(await allCategoryRequest.Content.ReadAsStringAsync());
                        userCategories = userCategories.OrderBy(s => s.System.Name).ThenBy(s => s.Name).ToList();
                    }
                    else
                    {
                        // Categories could not be listed
                        var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await infoRequest.Content.ReadAsStringAsync());
                        if (Layout != null)
                            if (Layout.AlertBox != null)
                                Layout.AlertBox.SetAlert("User settings", $"Category fetch from server is failed: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                    }
                }
                else
                {
                    // User info could not be loaded
                    var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await infoRequest.Content.ReadAsStringAsync());
                    if (Layout != null)
                        if (Layout.AlertBox != null)
                            Layout.AlertBox.SetAlert("User settings", $"User data fetch from server is failed: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                }
            }
            /*-----------------------------------------------------------------------------------*/
            /* Load the second subpage (change user data)                                        */
            /*-----------------------------------------------------------------------------------*/
            else if (UserPageState.ActMenu.Id == 2)
            {
                Console.WriteLine("Másik oldal vagyok");
            }
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

        /*---------------------------------------------------------------------------------------*/
        /* Function name: ChangeUserInfo                                                         */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Adjust user information (email or password)                                           */
        /*---------------------------------------------------------------------------------------*/
        private void ChangeUserInfo()
        {
            string oldpw = null;
            if (User.UserName == "God")
                oldpw = "admin";
            if (User.UserName == "Béla")
                oldpw = "user";

            if(oldpw != ChangeInfo.OldPassword)
            {
                Layout.AlertBox.SetAlert("User modification", "Old password does not match", AlertBox.AlertBoxType.Error);
                ChangeInfo = new ChangeUser();
                return;
            }

            bool changed = false;
            if (!string.IsNullOrEmpty(ChangeInfo.Email) && !string.IsNullOrWhiteSpace(ChangeInfo.Email))
            {
                User.Email = ChangeInfo.Email;
                changed = true;
            }

            if (!string.IsNullOrEmpty(ChangeInfo.Password) && !string.IsNullOrWhiteSpace(ChangeInfo.Password))
            {
                changed = true;
            }

            if (changed)
                Layout.AlertBox.SetAlert("User modification", "User information has been updated", AlertBox.AlertBoxType.Info);
            else
                Layout.AlertBox.SetAlert("User modification", "Nothing has changed", AlertBox.AlertBoxType.Info);

            ChangeInfo = new ChangeUser();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: ChangeUserInfoMissing                                                  */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Error message, something is missing in the input                                      */
        /*---------------------------------------------------------------------------------------*/
        private void ChangeUserInfoMissing()
        {
            Layout.AlertBox.SetAlert("User modification", "Current password must be specified as verification", AlertBox.AlertBoxType.Warning);
        }
    }
}
