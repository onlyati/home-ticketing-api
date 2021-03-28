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
            JSCalls call = new JSCalls(js);
            string link = $"{Configuration["ServerAddress"]}";

            // Check that user already logon
            TokenModel refresh = new TokenModel();
            refresh.AuthToken = call.GetLocalStorage("AuthToken");
            refresh.RefreshToken = call.GetLocalStorage("RefreshToken");
            if (refresh.AuthToken == null || refresh.RefreshToken == null)
                return;

            Console.WriteLine("Token was not null");
            var tokenJson = JsonSerializer.Serialize<TokenModel>(refresh);

            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refresh.AuthToken);
            var newTokenRequest = await Http.PostAsync($"{link}/user/refresh-token", new StringContent(tokenJson, Encoding.UTF8, "application/json"));
            Console.WriteLine($"Refresh token, RC: {newTokenRequest.StatusCode}");
            switch(newTokenRequest.StatusCode)
            {
                case HttpStatusCode.OK:
                    // No action, token is OK
                    // Update UserInfo
                    var updUserInfo1 = await Http.GetAsync($"{link}/user/info");
                    if (updUserInfo1.StatusCode == HttpStatusCode.OK)
                    {
                        var userTemp = JsonSerializer.Deserialize<UserInfo>(await updUserInfo1.Content.ReadAsStringAsync());
                        User.UserName = userTemp.UserName;
                        User.Email = userTemp.Email;
                        User.Role = userTemp.Role;
                    }
                    break;
                case HttpStatusCode.Continue:
                    // New token was created
                    var newToken = JsonSerializer.Deserialize<TokenModel>(await newTokenRequest.Content.ReadAsStringAsync());
                    call.SetLocalStorage("AuthToken", newToken.AuthToken);
                    call.SetLocalStorage("RefreshToken", newToken.RefreshToken);
                    Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken.AuthToken);
                    // Update UserInfo
                    var updUserInfo2 = await Http.GetAsync($"{link}/user/info");
                    if(updUserInfo2.StatusCode == HttpStatusCode.OK)
                    {
                        var userTemp = JsonSerializer.Deserialize<UserInfo>(await updUserInfo2.Content.ReadAsStringAsync());
                        User.UserName = userTemp.UserName;
                        User.Email = userTemp.Email;
                        User.Role = userTemp.Role;
                    }
                    break;
                case HttpStatusCode.Unauthorized:
                    // Authorization error, go the main page
                    call.RemoveLocalStorage("AuthToken");
                    call.RemoveLocalStorage("RefreshToken");
                    NavManager.NavigateTo("/");
                    break;
                case HttpStatusCode.NotFound:
                    // Authorization error, go the main page
                    call.RemoveLocalStorage("AuthToken");
                    call.RemoveLocalStorage("RefreshToken");
                    NavManager.NavigateTo("/");
                    break;
                default:
                    call.RemoveLocalStorage("AuthToken");
                    call.RemoveLocalStorage("RefreshToken");
                    break;
            }

            Console.WriteLine($"Mainpage init CONTINUE: {User.UserName};{User.Email};{User.Role}");
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
