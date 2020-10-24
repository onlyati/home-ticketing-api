using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    /* Model for Tickets database table */
    public class Ticket
    {
        public Guid Id { get; set; }
        public int Category { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }
        public string Time { get; set; }
        public string Title { get; set; }
    }
}
