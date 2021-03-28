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

            await LoadPersonalTickets();

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
            DashboardPageState.filter = new TicketFilter();
            DashboardPageState.IsPopUpShowed = false;

            /*-----------------------------------------------------------------------------------*/
            /* Build the parameters and send the request                                         */
            /*-----------------------------------------------------------------------------------*/
            var filterJson = JsonSerializer.Serialize<TicketFilter>(DashboardPageState.filter);
            var filterRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/ticket/list/filter?username={User.UserName}&status=Open");
        }
    }
}
