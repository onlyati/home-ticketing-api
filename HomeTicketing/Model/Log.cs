using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    /* Model for Logs database table */
    public class Log
    {
        public int Id { get; set; }
        public string Summary { get; set; }
        public string Details { get; set; }
        public Ticket Ticket { get; set; }
        public string Time { get; set; }
    }
}
