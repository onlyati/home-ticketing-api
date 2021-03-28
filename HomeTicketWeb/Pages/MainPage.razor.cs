using HomeTicketWeb.Components;
using HomeTicketWeb.Model;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

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
        protected override async Task OnInitializedAsync()
        {
            bool check = await RefreshService.RefreshToken(js, User, Configuration["ServerAddress"], Http, NavManager);
            if(!check)
            {
                User.UserName = null;
                User.Email = null;
                User.Role = null;
            }
            StateHasChanged();
        }
        /*---------------------------------------------------------------------------------------*/
        /* Function name: Login                                                                  */
        /*                                                                                       */
        /* Description:                                                                          */
        /* It is a just a dummy function for testing Blazor components                           */
        /*---------------------------------------------------------------------------------------*/
        private async Task Login()
        {
            JSCalls call = new JSCalls(js);
            string link;

            // Token was not OK, or did not exist, then login
            string content = JsonSerializer.Serialize<LoginInfo>(Info);
            link = $"{Configuration["ServerAddress"]}/user/login";
            var response = await Http.PostAsync(link, new StringContent(content, Encoding.UTF8, "application/json"));

            if(response.StatusCode == HttpStatusCode.OK)
            {
                // Deserialize the JSON into object
                var tokenJson = JsonSerializer.Deserialize<TokenModel>(await response.Content.ReadAsStringAsync());

                // Save the tokens into local storage
                call.SetLocalStorage("AuthToken", tokenJson.AuthToken);
                call.SetLocalStorage("RefreshToken", tokenJson.RefreshToken);

                // Add Bearer token into the header
                Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenJson.AuthToken);

                // Get user information
                link = $"{Configuration["ServerAddress"]}/user/info?username={Info.UserName}";
                var infoResponse = await Http.GetAsync(link);
                if(infoResponse.StatusCode != HttpStatusCode.OK)
                {
                    if (Layout != null)
                        if (Layout.AlertBox != null)
                            Layout.AlertBox.SetAlert("User login", "Error during fetching user data", AlertBox.AlertBoxType.Error);
                }

                var userTemp = JsonSerializer.Deserialize<UserInfo>(await infoResponse.Content.ReadAsStringAsync());
                User.UserName = userTemp.UserName;
                User.Email = userTemp.Email;
                User.Role = userTemp.Role;

                // Start a refresh token timer
                if (!RefreshTimer.Enabled)
                {
                    Console.WriteLine($"Refresh timer is started {DateTime.Now}");
                    RefreshTimer = new Timer();
                    RefreshTimer.Interval = Convert.ToInt32(Configuration["RefreshInterval"]);
                    RefreshTimer.Elapsed += Layout.RefreshTimeOnElapsed;
                    RefreshTimer.Start();
                }
            }
            else if(response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("User login", "Username or password is invalid", AlertBox.AlertBoxType.Error);
            }
            else
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("User login", "Login has failed due to severe error", AlertBox.AlertBoxType.Error);
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

            // Clear the tokens
            JSCalls call = new JSCalls(js);
            call.RemoveLocalStorage("AuthToken");
            call.RemoveLocalStorage("RefreshToken");

            // Close all opened applications
            if (Layout != null)
                if (Layout.Bar != null)
                    Layout.Bar.ClearOpenedApp();

            RefreshTimer.Dispose();

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
