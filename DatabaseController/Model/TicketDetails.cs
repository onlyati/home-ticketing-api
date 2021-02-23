using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseController.Model
{
    public class TicketDetails
    {
        /// <summary>
        /// Contains the base summary and the meta data
        /// </summary>
        public DataModel.Ticket Header { get; set; }
        /// <summary>
        /// List about the logs which belongs to ticket
        /// </summary>
        public List<DataModel.Log> Logs { get; set; }
    }
}
