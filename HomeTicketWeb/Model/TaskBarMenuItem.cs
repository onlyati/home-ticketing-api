using HomeTicketWeb.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    /*********************************************************************************************/
    /* Class for 'TaskBar' component. It defines what menu are required and describe them        */
    /*********************************************************************************************/
    public class TaskBarMenuItem
    {
        /*---------------------------------------------------------------------------------------*/
        /* Properties                                                                            */
        /*---------------------------------------------------------------------------------------*/
        public string Title { get; set; }                                                // Title, it will be seen as application name

        public string Route { get; set; }                                                // Relative address of a page/app

        public string Image { get; set; }                                                // Icon for app

        public string Text { get; set; }                                                 // Short summary about the app, it will be showed in taskbar box

        public string DivId { get; set; }                                                // ID of a div in html, must be unique on each row

        public IShareDataModel DataContainer { get; set; }                               // Object which is used for cleanup (remove data from cache)

        /*---------------------------------------------------------------------------------------*/
        /* Constructor                                                                           */
        /*---------------------------------------------------------------------------------------*/
        public TaskBarMenuItem(string title, string route, string image, string text, IShareDataModel dataContainer, string id)
        {
            Title = title;
            Route = route;
            Image = image;
            Text = text;
            DataContainer = dataContainer;
            DivId = id;
        }

        /*---------------------------------------------------------------------------------------*/
        /* Execute comparison betwwen two path string                                            */
        /*---------------------------------------------------------------------------------------*/
        public bool CompareRoute(string otherPath)
        {
            if (otherPath == "/")
                return this.Route == otherPath;
            else if (otherPath == "/index")
                return this.Route == "/";
            else
                return otherPath.StartsWith(this.Route);
        }
    }
}
