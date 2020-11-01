using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    /* Model to read inputs for ticket creation */
    public class TicketReq
    {
        public int Id { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }
        public string Time { get; set; }
        public string Details { get; set; }
        public string Title { get; set; }
    }
}
