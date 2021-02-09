using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    public class TicketCreateHeader
    {
        public string Summary { get; set; }
        public string Category { get; set; }
        public string Reference { get; set; }
        public string Details { get; set; }
        public string Title { get; set; }
    }
}
