using HomeTicketWeb.Model;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using HomeTicketWeb.Shared;
using HomeTicketWeb.Components;
using System.ComponentModel.DataAnnotations;

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
        private List<TicketListElem> tickets = new List<TicketListElem>()
        {
            new TicketListElem()
            {
                Id = 1,
                Reference = "test-01",
                Status = "Open",
                Time = DateTime.Now,
                Title = "Test ticket",
                Category = new Model.Category()
                {
                    Id = 1,
                    Name = "System",
                },
                System = new Model.SystemElem()
                {
                    Id = 1,
                    Name = "atihome",
                },
                User = new Model.User()
                {
                    Id = 1,
                    UserName = "Béla",
                    Email = "user@ize.com",
                    Role = "User",
                }
            },
            new TicketListElem()
            {
                Id = 1,
                Reference = "test-02",
                Status = "Open",
                Time = DateTime.Now,
                Title = "Another test ticket",
                Category = new Model.Category()
                {
                    Id = 2,
                    Name = "Network",
                },
                System = new Model.SystemElem()
                {
                    Id = 1,
                    Name = "atihome",
                },
                User = null,
            },
        };

        private TicketFilter filter = new TicketFilter();

        /*=======================================================================================*/
        /* Classes                                                                               */
        /*=======================================================================================*/
        private class TicketFilter
        {
            [Required]
            public string Title { get; set; }

            [Required]
            public string Status { get; set; }

            [Required]
            public string SystemName { get; set; }

            [Required]
            public string CategoryName { get; set; }

            [Required]
            public string Owner { get; set; }
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
        protected override void OnInitialized()
        {
            if(User.Role == null || User.UserName == null)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Unathorized access", "You are not authorized. Login first if you want to do something", AlertBox.AlertBoxType.Error);
                NavManager.NavigateTo("/");
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
    }
}
