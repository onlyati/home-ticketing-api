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

        public TaskBarMenuItem(string title, string route, string image)
        {
            Title = title;
            Route = route;
            Image = image;
        }
    }
}
