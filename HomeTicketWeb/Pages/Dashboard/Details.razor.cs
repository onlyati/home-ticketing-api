using HomeTicketWeb.Components;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        [Parameter]
        public int id { get; set; }

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
            if (User.Role == null || User.UserName == null)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Unathorized access", "You are not authorized. Login first if you want to do something", AlertBox.AlertBoxType.Error);
                NavManager.NavigateTo("/");
            }
        }
    }
}
