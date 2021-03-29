using HomeTicketWeb.Model;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using HomeTicketWeb.Shared;
using HomeTicketWeb.Components;
using System.ComponentModel.DataAnnotations;
using System.Timers;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Net;

namespace HomeTicketWeb.Pages.Dashboard
{
    /*********************************************************************************************/
    /* This class is for handling stuff on dashboard (Dashboard.razor) component                 */
    /*********************************************************************************************/
    public partial class Dashboard : ComponentBase
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
        private List<TicketListElem> tickets = new List<TicketListElem>();
        private List<TicketListElem> filteredTickets = new List<TicketListElem>();
        private TicketFilter filter = new TicketFilter();

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
                await LoadPersonalTickets();

                if (DashboardPageState.FilterFindDone)
                    await FilterTicketSubmit();
            }

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: LoadPersonalTickets                                                    */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method filters the personal tickets and those which are unassigned and upload    */
        /* them into a list                                                                      */
        /*---------------------------------------------------------------------------------------*/
        private async Task LoadPersonalTickets()
        {
            /*-----------------------------------------------------------------------------------*/
            /* Get unassigned open tickets first                                                 */
            /*-----------------------------------------------------------------------------------*/
            var unassignedRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/ticket/list/filter?unassigned=true&status=Open");
            if(unassignedRequest.StatusCode != System.Net.HttpStatusCode.OK)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Ticket listing", "Personal ticket listing has failed", AlertBox.AlertBoxType.Error);
                return;
            }

            var unassignedList = JsonSerializer.Deserialize<List<TicketListElem>>(await unassignedRequest.Content.ReadAsStringAsync());

            /*-----------------------------------------------------------------------------------*/
            /* Get personal tickets                                                              */
            /*-----------------------------------------------------------------------------------*/
            var ownedRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/ticket/list/filter?username={User.UserName}&status=Open");
            if (ownedRequest.StatusCode != System.Net.HttpStatusCode.OK)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Ticket listing", "Personal ticket listing has failed", AlertBox.AlertBoxType.Error);
                return;
            }

            var ownedList = JsonSerializer.Deserialize<List<TicketListElem>>(await ownedRequest.Content.ReadAsStringAsync());

            /*-----------------------------------------------------------------------------------*/
            /* Create the final list                                                             */
            /*-----------------------------------------------------------------------------------*/
            tickets = new List<TicketListElem>();
            tickets = tickets.Concat(unassignedList).ToList();
            tickets = tickets.Concat(ownedList).ToList();
            tickets = tickets.OrderBy(s => s.Id).Distinct().ToList();

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
        /* Function name: GetDetails                                                             */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method navigate inot a new window, where the details can be  seen about ticket   */
        /*---------------------------------------------------------------------------------------*/
        private void GetDetails(int id)
        {
            TaskBarMenuItem nextPage = new TaskBarMenuItem($"Details of #{id}", $"/dashboard/details/{id}", $"img/ticket-icon.png", $"Details of #{id}", null, $"Details{id}");
            Layout.Bar.AddOpenedApp(nextPage);
            NavManager.NavigateTo(nextPage.Route);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: CloseFilterPopup                                                       */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method hide the popup window wherer ticket can be filtered                       */
        /*---------------------------------------------------------------------------------------*/
        private void CloseFilterPopup()
        {
            DashboardPageState.IsPopUpShowed = false;
            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: FIlterTicketSubmit                                                     */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Request ticket listing based on filters                                               */
        /*---------------------------------------------------------------------------------------*/
        public async Task FilterTicketSubmit()
        {
            filteredTickets = new List<TicketListElem>();
            /*-----------------------------------------------------------------------------------*/
            /* Build the parameters and send the request                                         */
            /*-----------------------------------------------------------------------------------*/
            string link = $"{Configuration["ServerAddress"]}/ticket/list/filter?";

            if(!string.IsNullOrEmpty(DashboardPageState.filter.Title) && !string.IsNullOrWhiteSpace(DashboardPageState.filter.Title))
                link += $"title={DashboardPageState.filter.Title}&";

            if (!string.IsNullOrEmpty(DashboardPageState.filter.Status) && !string.IsNullOrWhiteSpace(DashboardPageState.filter.Status))
                link += $"status={DashboardPageState.filter.Status}&";

            if (!string.IsNullOrEmpty(DashboardPageState.filter.SystemName) && !string.IsNullOrWhiteSpace(DashboardPageState.filter.SystemName))
                link += $"system={DashboardPageState.filter.SystemName}&";

            if (!string.IsNullOrEmpty(DashboardPageState.filter.CategoryName) && !string.IsNullOrWhiteSpace(DashboardPageState.filter.CategoryName))
                link += $"category={DashboardPageState.filter.CategoryName}&";

            if (DashboardPageState.filter.Owner == "*Unassigned*")
                link += $"unassigned=true";
            else if (!string.IsNullOrEmpty(DashboardPageState.filter.Owner) && !string.IsNullOrWhiteSpace(DashboardPageState.filter.Owner))
                link += $"username={DashboardPageState.filter.Owner}&";

            var filterRequest = await Http.GetAsync(link);
            if(filterRequest.StatusCode != HttpStatusCode.OK)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Ticket listing", "Ticket could not listed", AlertBox.AlertBoxType.Error);
                return;
            }

            filteredTickets = JsonSerializer.Deserialize<List<TicketListElem>>(await filterRequest.Content.ReadAsStringAsync());

            /*-----------------------------------------------------------------------------------*/
            /* Request is done, hide the popup                                                   */
            /*-----------------------------------------------------------------------------------*/
            DashboardPageState.IsPopUpShowed = false;
            DashboardPageState.FilterFindDone = true;

            StateHasChanged();
        }

        private async Task AssignTicket(int id)
        {
            var assignRequest = await Http.PutAsync($"{Configuration["ServerAddress"]}/ticket/assign?ticketid={id}&username={User.UserName}", null);
            if(assignRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponde = JsonSerializer.Deserialize<GeneralMessage>(await assignRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Ticket assigment", $"Assignment failed: {badResponde.Message}", AlertBox.AlertBoxType.Warning);
                return;
            }

            await LoadPersonalTickets();
        }

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

            await LoadPersonalTickets();
        }

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

            await LoadPersonalTickets();
        }
    }
}
