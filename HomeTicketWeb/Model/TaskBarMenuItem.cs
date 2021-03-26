using HomeTicketWeb.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public class TaskBarMenuItem
    {
        public string Title { get; set; }

        public string Route { get; set; }

        public string Image { get; set; }

        public string Text { get; set; }

        public ContextMenu ContextMenu { get; set; }

        public IShareDataModel DataContainer { get; set; }

        public string DivId { get; set; }

        public TaskBarMenuItem(string title, string route, string image, string text, IShareDataModel dataContainer, string id)
        {
            Title = title;
            Route = route;
            Image = image;
            Text = text;
            DataContainer = dataContainer;
            DivId = id;
        }

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
