using HomeTicketWeb.Components;
using HomeTicketWeb.Model;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private void SendRequest()
        {
            if (User.Role != null && User.UserName != null)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Successfully request", "Ticket has been opened", AlertBox.AlertBoxType.Info);
            }
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
