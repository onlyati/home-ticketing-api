using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseController.Model
{
    /// <summary>
    /// Model to create JSON when details request is got
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Contains the base summary and the meta data
        /// </summary>
        public TicketHeader Header { get; set; }
        /// <summary>
        /// List about the logs which belongs to ticket
        /// </summary>
        public List<Log> Logs { get; set; }
    }
}
