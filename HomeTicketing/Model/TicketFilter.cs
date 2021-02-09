using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    public class TicketFilter
    {
        public string Category { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
    }
}
