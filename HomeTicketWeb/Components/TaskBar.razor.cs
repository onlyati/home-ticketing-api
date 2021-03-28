using HomeTicketWeb.Model;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Components
{
    /*********************************************************************************************/
    /* This class is for 'TaskBar' component                                                     */
    /*********************************************************************************************/
    public partial class TaskBar : ComponentBase
    {
        /*=======================================================================================*/
        /* Variables and properties                                                              */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Parameters                                                                            */
        /*---------------------------------------------------------------------------------------*/
        [Parameter] public List<TaskBarMenuItem> AppList { get; set; }                   // Possible applications are passed via this list

        /*---------------------------------------------------------------------------------------*/
        /* Cascaded peroperties                                                                  */
        /*---------------------------------------------------------------------------------------*/
        [CascadingParameter] public MainLayout Layout { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Private, local variables                                                              */
        /*---------------------------------------------------------------------------------------*/
        private List<TaskBarMenuItem> OpenedApps = new List<TaskBarMenuItem>();          // Tracking opened applications on the page

        private ContextMenu CMenu;                                                       // Reference for ContextMenu component
        private TaskBarMenuItem SelectedItem;                                            // Selected item on the taskbar (for ContextMenu)
        private int DistanceFromLeft;                                                    // X position of ContextMenu

        private bool IsVisible = false;                                                  // Hide/Show the taskbar box
        private string DescText;                                                         // Variable for taskbar box binding
        private string DescTitle;                                                        // Variable for taskbar box binding
        private string DescIcon;                                                         // Variable for taskbar box binding

        /*=======================================================================================*/
        /* Methods                                                                               */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Function name: ContextMenu                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This function is called of user press a right click on taskbar on an opened app       */
        /*---------------------------------------------------------------------------------------*/
        private void ContextMenu(MouseEventArgs e, TaskBarMenuItem item)
        {
            // Check that right-click is pressed
            if (e.Button == 2)
            {
                // Set X position of ContextMenu component, then show it
                JSCalls call = new JSCalls(js);
                var dimensions = call.GetBounds(item.DivId);
                DistanceFromLeft = Convert.ToInt32(dimensions.Left);

                SelectedItem = item;
                CMenu.Show();
            }
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: ConextMenuOpen                                                         */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This is a method, which is executed if user clikc to 'open' action in the context menu*/
        /*---------------------------------------------------------------------------------------*/
        public void ConextMenuOpen()
        {
            NavManager.NavigateTo(SelectedItem.Route);
            CMenu.Hide();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: ConextMenuClose                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This is a method, which is executed if user clikc to close action in the context menu */
        /*---------------------------------------------------------------------------------------*/
        public void ConextMenuClose()
        {
            RemoveOpenedApp(SelectedItem.Route);
            CMenu.Hide();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: AddOpenedApp                                                           */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Add new application to the taskbar                                                    */
        /*---------------------------------------------------------------------------------------*/
        public void AddOpenedApp(string route)
        {
            // Main page is not need to be added
            if (route == "/" || route == "/index")
                return;

            // Looking for defined application
            TaskBarMenuItem app = null;
            foreach (var item in AppList)
            {
                if (item.CompareRoute(route))
                    app = item;
            }

            // Change list accordingly
            if (app != null)
            {
                var isContain = OpenedApps.Where(s => s.Title == app.Title).Select(s => s).FirstOrDefault();
                if (isContain == null)
                {
                    var record = AppList.Where(s => s.Title == app.Title).Select(s => s).FirstOrDefault();
                    OpenedApps.Add(record);
                }
            }

            StateHasChanged();
        }

        public void AddOpenedApp(TaskBarMenuItem item)
        {
            // Change list accordingly
            if (item != null)
            {
                var isContain = OpenedApps.Where(s => s.Title == item.Title).Select(s => s).FirstOrDefault();
                if (isContain == null)
                {
                    var record = AppList.Where(s => s.Title == item.Title).Select(s => s).FirstOrDefault();
                    OpenedApps.Add(record);
                }
            }

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: RemoveOpenedApp                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Remove app from taskbar and navigate to the latest one                                */
        /*---------------------------------------------------------------------------------------*/
        public void RemoveOpenedApp(string route)
        {
            RemoveOpenedAppNoNavigate(route);

            if (OpenedApps.Count == 0)
            {
                NavManager.NavigateTo("/");
            }
            else
            {
                NavManager.NavigateTo(OpenedApps[OpenedApps.Count - 1].Route);
            }

        }

        public void RemoveOpenedApp(TaskBarMenuItem item)
        {
            RemoveOpenedAppNoNavigate(item);

            if (OpenedApps.Count == 0)
            {
                NavManager.NavigateTo("/");
            }
            else
            {
                NavManager.NavigateTo(OpenedApps[OpenedApps.Count - 1].Route);
            }
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: RemoveOpenedAppNoNavigate                                              */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Remove app from taskbar                                                               */
        /*---------------------------------------------------------------------------------------*/
        private void RemoveOpenedAppNoNavigate(string route)
        {
            if (route == "/index")
                route = "/";

            TaskBarMenuItem app = null;
            foreach (var item in AppList)
            {
                if (item.CompareRoute(route))
                    app = item;
            }

            if (app != null)
            {
                var record = OpenedApps.Where(s => s.Title == app.Title).Select(s => s).FirstOrDefault();
                if (record != null)
                {
                    if (record.DataContainer != null)
                        record.DataContainer.SetNull();

                    OpenedApps.Remove(record);
                }
            }

            StateHasChanged();
        }

        public void RemoveOpenedAppNoNavigate(TaskBarMenuItem item)
        {
            if (item != null)
            {
                var record = OpenedApps.Where(s => s.Title == item.Title).Select(s => s).FirstOrDefault();
                if (record != null)
                {
                    if (record.DataContainer != null)
                        record.DataContainer.SetNull();

                    OpenedApps.Remove(record);
                }
            }

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: ClearOpenedApp                                                         */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Remove all app from taskbar                                                           */
        /*---------------------------------------------------------------------------------------*/
        public void ClearOpenedApp()
        {
            var routeList = OpenedApps.Select(s => s.Route).ToList();
            foreach (var item in routeList)
            {
                RemoveOpenedAppNoNavigate(item);
            }
            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: ShowDescription                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Show a summary about the application of cursor is hover                               */
        /*---------------------------------------------------------------------------------------*/
        public void ShowDescription(string title, string icon, string text)
        {
            DescText = text;
            DescIcon = icon;
            DescTitle = title;
            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: Usage                                                                  */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Hide/SHow the taskbar box                                                             */
        /*---------------------------------------------------------------------------------------*/
        private void Usage()
        {
            string actPage = NavManager.Uri.Substring(NavManager.BaseUri.Length - 1);
            if (actPage == "") actPage = "/";

            foreach (var item in Layout.Apps)
            {
                if (item.CompareRoute(actPage))
                {
                    DescIcon = item.Image;
                    DescText = item.Text;
                    DescTitle = item.Title;
                }
            }

            IsVisible = !IsVisible;
            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: Hide                                                                   */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Hide the taskbar box                                                                  */
        /*---------------------------------------------------------------------------------------*/
        public void Hide()
        {
            IsVisible = false;
            StateHasChanged();
        }

    }
}
