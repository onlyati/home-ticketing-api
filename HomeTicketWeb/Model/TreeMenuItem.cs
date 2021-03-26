using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    /*********************************************************************************************/
    /* Object to describe elements in 'TreeMenu' component                                       */
    /*********************************************************************************************/
    public class TreeMenuItem : IShareDataModel
    {
        public string Section { get; set; }

        public string Title { get; set; }

        public int Id { get; set; }

        public bool Selected { get; set; } = false;

        public void SetNull()
        {
            Section = null;
            Title = null;
            Id = 0;
            Selected = false;
        }
    }
}
