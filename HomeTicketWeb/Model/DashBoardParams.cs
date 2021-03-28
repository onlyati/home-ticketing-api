using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public class DashBoardParams : IShareDataModel
    {
        public bool IsPopUpShowed { get; set; }

        public TicketFilter filter { get; set; }

        public bool FilterFindDone { get; set; }

        public DashBoardParams()
        {
            filter = new TicketFilter();
            IsPopUpShowed = false;
            FilterFindDone = false;
        }

        public void SetNull()
        {
            if (filter != null)
                filter.SetNull();
            IsPopUpShowed = false;
            FilterFindDone = false;
        }
    }

    public class TicketFilter : IShareDataModel
    {
        public string Title { get; set; }

        public string Status { get; set; }

        public string SystemName { get; set; }

        public string CategoryName { get; set; }

        public string Owner { get; set; }

        public bool? Unassigned { get; set; }


        public void SetNull()
        {
            Title = null;
            Status = null;
            SystemName = null;
            CategoryName = null;
            Owner = null;
            Unassigned = null;
        }
    }
}
