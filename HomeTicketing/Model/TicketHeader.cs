using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    public class TicketHeader
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }
        public string Time { get; set; }
        public string Title { get; set; }
    }
}
