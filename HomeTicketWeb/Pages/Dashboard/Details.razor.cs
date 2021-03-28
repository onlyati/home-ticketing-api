using HomeTicketWeb.Components;
using HomeTicketWeb.Model;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        /*---------------------------------------------------------------------------------------*/
        /* Get cascaded values                                                                   */
        /*---------------------------------------------------------------------------------------*/
        [CascadingParameter] public MainLayout Layout { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Private, local variables and objects                                                  */
        /*---------------------------------------------------------------------------------------*/
        [Parameter] public int id { get; set; }
        private TicketDetails details = new TicketDetails();
        private string PageTitle;


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
                PageTitle = $"Details of #{id}";
                await LoadDetails();
            }

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: LoadDetails                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* It loads details of and ID which was passed via parameter                             */
        /*---------------------------------------------------------------------------------------*/
        private async Task LoadDetails()
        {
            details = new TicketDetails();
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
    }
}
