using HomeTicketWeb.Components;
using HomeTicketWeb.Model;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;

namespace HomeTicketWeb.Pages.Open
{
    /*********************************************************************************************/
    /* This class is for the ticket creation methods and properties (OpenTicket.razor)           */
    /*********************************************************************************************/
    public partial class OpenTicket : ComponentBase
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
        private List<SystemElem> Systems = new List<SystemElem>();
        private List<Category> Categories = new List<Category>();        

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

            if(check)
            {
                await LoadSystems();
                if (OpenPageState.SystemName != null)
                    await LoadCategories(null);
            }

            StateHasChanged();
        }

        private async Task LoadSystems()
        {
            Systems = new List<SystemElem>();

            var systemRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/system/list/all");
            if(systemRequest.StatusCode != HttpStatusCode.OK)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Create ticket", "Systems could not list", AlertBox.AlertBoxType.Error);
                return;
            }

            Systems = JsonSerializer.Deserialize<List<SystemElem>>(await systemRequest.Content.ReadAsStringAsync());
            Systems = Systems.OrderBy(s => s.Name).ToList();

            StateHasChanged();
        }

        private async Task LoadCategories(ChangeEventArgs e)
        {
            Categories = new List<Category>();

            if(e != null)
                if (e.Value == null)
                    return;
                else
                    OpenPageState.SystemName = e.Value.ToString();

            var categoryRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/category/list/system?value={OpenPageState.SystemName}");
            if (categoryRequest.StatusCode != HttpStatusCode.OK)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Create ticket", "Categories could not list", AlertBox.AlertBoxType.Error);
                return;
            }

            Categories = JsonSerializer.Deserialize<List<Category>>(await categoryRequest.Content.ReadAsStringAsync());
            Categories = Categories.OrderBy(s => s.Name).ToList();

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: MissingInput                                                           */
        /*                                                                                       */
        /* Description:                                                                          */
        /* If ticket creation input data validation is failed, this is called                    */
        /*---------------------------------------------------------------------------------------*/
        private void MissingInput()
        {
            if (User.Role != null && User.UserName != null)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Missing input", "One or more field has not been filled with value", AlertBox.AlertBoxType.Warning);
            }
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: SendRequest                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Send ticket open request to the server (actully just dummy)                           */
        /*---------------------------------------------------------------------------------------*/
        private async Task SendRequest()
        {
            /*-----------------------------------------------------------------------------------*/
            /* Build the request and send                                                        */
            /*-----------------------------------------------------------------------------------*/
            OpenPageState.Reference = $"{User.UserName}-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}-{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}";
            var createJson = JsonSerializer.Serialize<CreateTicket>(OpenPageState);
            var createRequest = await Http.PostAsync($"{Configuration["ServerAddress"]}/ticket/create", new StringContent(createJson, Encoding.UTF8, "application/json"));
            if(createRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await createRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Successfully request", $"Request failed: {badResponse.Message}", AlertBox.AlertBoxType.Error);
            }

            /*-----------------------------------------------------------------------------------*/
            /* It seems, everythign was fine                                                     */
            /*-----------------------------------------------------------------------------------*/
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Successfully request", "Ticket has been opened", AlertBox.AlertBoxType.Info);

            OpenPageState.SetNull();
            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: CloseWindow                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Eventcallback for 'WindowTitle' component. This defines what should happen if somebody*/
        /* click to the 'X' in the right corner                                                  */
        /*---------------------------------------------------------------------------------------*/
        public void CloseWindow()
        {
            // Close and remove application via 'TaskBar' component
            if (Layout != null)
                if (Layout.Bar != null)
                    Layout.Bar.RemoveOpenedApp(NavManager.Uri.Substring(NavManager.BaseUri.Length - 1));
        }
    }
}
