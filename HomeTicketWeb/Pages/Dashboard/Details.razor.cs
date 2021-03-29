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
using System.Threading.Tasks;
using System.Timers;

namespace HomeTicketWeb.Pages.Dashboard
{
    /*********************************************************************************************/
    /* This class is for handling stuff on details (Details.razor) component                     */
    /*********************************************************************************************/
    public partial class Details : ComponentBase
    {
        /*=======================================================================================*/
        /* Variables and objects                                                                 */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Parameters                                                                            */
        /*---------------------------------------------------------------------------------------*/
        [Parameter]
        public int id { get; set; }
        
        /*---------------------------------------------------------------------------------------*/
        /* Get cascaded values                                                                   */
        /*---------------------------------------------------------------------------------------*/
        [CascadingParameter] public MainLayout Layout { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Private, local variables and objects                                                  */
        /*---------------------------------------------------------------------------------------*/
        private TicketDetails details = new TicketDetails();
        private string PageTitle;

        private CreateTicket AddLog = new CreateTicket();
        private bool ShowAddLogWindow = false;

        private bool Loading = false;

        private bool ShowChangeWindow = false;
        private ChangeTicketTemplate ChangeTicket;
        private List<SystemElem> Systems;
        private List<Category> Categories;

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

            if (check)
            {
                if (Layout != null)
                    if (Layout.Bar != null)
                        if (Layout.Bar.OpenedAppsCount() == 0)
                        {
                            NavManager.NavigateTo("/");
                            return;
                        }
            }

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: OnParameterSetAsync                                                    */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Because OnInitialiezedAsync runs only once when the component is created thsu that    */
        /* not able to refresh details in case of multiple Details component navigation.         */
        /*---------------------------------------------------------------------------------------*/
        protected override async Task OnParametersSetAsync()
        {
            ShowAddLogWindow = false;
            ShowChangeWindow = false;
            AddLog.SetNull();

            ChangeTicket = new ChangeTicketTemplate();
            Systems = null;
            Categories = null;

            details = new TicketDetails();
            PageTitle = $"Details of #{id}";
            Loading = true;
            // Need some wait, else the async process will be chatoci between page change
            await Task.Delay(500);
            await LoadDetails();
            Loading = false;
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: LoadDetails                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* It loads details of and ID which was passed via parameter                             */
        /*---------------------------------------------------------------------------------------*/
        private async Task LoadDetails()
        {
            var detailsRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/ticket/details?id={id}");
            if(detailsRequest.StatusCode != HttpStatusCode.OK)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Get details", "Ticket details could not be collected", AlertBox.AlertBoxType.Error);
                return;
            }

            details = JsonSerializer.Deserialize<TicketDetails>(await detailsRequest.Content.ReadAsStringAsync());
            foreach (var item in details.Logs)
            {
                item.Details.Replace(Environment.NewLine, "<br />");
            }

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

        /*---------------------------------------------------------------------------------------*/
        /* Function name: AssignTicket                                                           */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Assign ticket to they who wants to claim it                                           */
        /*---------------------------------------------------------------------------------------*/
        private async Task AssignTicket(int id)
        {
            var assignRequest = await Http.PutAsync($"{Configuration["ServerAddress"]}/ticket/assign?ticketid={id}&username={User.UserName}", null);
            if (assignRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponde = JsonSerializer.Deserialize<GeneralMessage>(await assignRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Ticket assigment", $"Assignment failed: {badResponde.Message}", AlertBox.AlertBoxType.Warning);
                return;
            }

            await LoadDetails();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: UnassignTicket                                                         */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Methid is called if user want to get rid of from the ticket                           */
        /*---------------------------------------------------------------------------------------*/
        private async Task UnassignTicket(int id)
        {
            var unassignRequest = await Http.PutAsync($"{Configuration["ServerAddress"]}/ticket/unassign?ticketid={id}", null);
            if (unassignRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponde = JsonSerializer.Deserialize<GeneralMessage>(await unassignRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Ticket assigment", $"Assignment failed: {badResponde.Message}", AlertBox.AlertBoxType.Warning);
                return;
            }

            await LoadDetails();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: CloseTicket                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Close ticket if it is already assigned to the user                                    */
        /*---------------------------------------------------------------------------------------*/
        private async Task CloseTicket(int id)
        {
            var deleteRequest = await Http.PutAsync($"{Configuration["ServerAddress"]}/ticket/close?id={id}", null);
            if (deleteRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponde = JsonSerializer.Deserialize<GeneralMessage>(await deleteRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Ticket assigment", $"Assignment failed: {badResponde.Message}", AlertBox.AlertBoxType.Warning);
                return;
            }

            await LoadDetails();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: AddLogRecord                                                           */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method append a new log entry for the selected ticket                            */
        /*---------------------------------------------------------------------------------------*/
        private async Task AddLogRecord()
        {
            // Data validation
            if(AddLog.Summary == null)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Add new log record", "Summary is missing", AlertBox.AlertBoxType.Error);
                return;
            }

            // Assemble the request and send it
            AddLog.CategoryName = details.Header.Category.Name;
            AddLog.Reference = details.Header.Reference;
            AddLog.SystemName = details.Header.System.Name;
            AddLog.Title = details.Header.Title;
            var requestJson = JsonSerializer.Serialize<CreateTicket>(AddLog);
            var request = await Http.PostAsync($"{Configuration["ServerAddress"]}/ticket/create", new StringContent(requestJson, Encoding.UTF8, "application/json"));
            if(request.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await request.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Add new log record", $"Add log record is failed: {badResponse.Message}", AlertBox.AlertBoxType.Error);
            }

            ShowAddLogWindow = false;

            await LoadDetails();
        }

        private async Task ChangeTicketSubmit()
        {
            if((string.IsNullOrWhiteSpace(ChangeTicket.CategoryName) || string.IsNullOrWhiteSpace(ChangeTicket.CategoryName)) &&
               (string.IsNullOrWhiteSpace(ChangeTicket.SystemName) || string.IsNullOrWhiteSpace(ChangeTicket.SystemName)) &&
               (string.IsNullOrWhiteSpace(ChangeTicket.Reference) || string.IsNullOrWhiteSpace(ChangeTicket.Reference)) &&
               (string.IsNullOrWhiteSpace(ChangeTicket.Title) || string.IsNullOrWhiteSpace(ChangeTicket.Title)))
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Change ticket", "Nothing is specified to change", AlertBox.AlertBoxType.Info);
                return;
            }

            ChangeTicket.Id = id;

            var changeJson = JsonSerializer.Serialize<ChangeTicketTemplate>(ChangeTicket);
            var changeRequest = await Http.PutAsync($"{Configuration["ServerAddress"]}/ticket/change", new StringContent(changeJson, Encoding.UTF8, "application/json"));
            if(changeRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await changeRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Change ticket", $"Change failer: {badResponse.Message}", AlertBox.AlertBoxType.Warning);
            }
            else
            {
                var goodResponse = JsonSerializer.Deserialize<GeneralMessage>(await changeRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Change ticket", $"Change is done: {goodResponse.Message}", AlertBox.AlertBoxType.Info);
            }

            ChangeTicket = new ChangeTicketTemplate();

            await LoadDetails();

            ShowChangeWindow = false;
        }

        private async Task LoadCategories(ChangeEventArgs e)
        {
            if(e.Value == null)
            {
                Categories = null;
                return;
            }

            if(string.IsNullOrEmpty(e.Value.ToString()) || string.IsNullOrWhiteSpace(e.Value.ToString()))
            {
                Categories = null;
                return;
            }

            ChangeTicket.SystemName = e.Value.ToString();

            var categoriesRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/category/list/system?value={e.Value.ToString()}");
            if(categoriesRequest.StatusCode != HttpStatusCode.OK)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Change ticket", "Fetching categories is failed", AlertBox.AlertBoxType.Error);
                return;
            }

            Categories = JsonSerializer.Deserialize<List<Category>>(await categoriesRequest.Content.ReadAsStringAsync());
        }

        private async Task DisplayChangeWindow()
        {
            var systemRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/system/get?id={details.Header.System.Id}");
            if(systemRequest.StatusCode != HttpStatusCode.OK)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Change ticket", "Fetching systems is failed", AlertBox.AlertBoxType.Error);
                return;
            }

            var selectedSystem = JsonSerializer.Deserialize<SystemElem>(await systemRequest.Content.ReadAsStringAsync());
            Systems = new List<SystemElem>();
            Systems.Add(selectedSystem);

            ShowChangeWindow = true;
        }
    }
}
