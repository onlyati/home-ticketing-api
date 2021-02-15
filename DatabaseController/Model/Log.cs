using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseController.Model
{
    /// <summary>
    /// Model for Logs database table
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Uniqe ID
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Summary of the log
        /// </summary>
        /// <example>This is a summary</example>
        public string Summary { get; set; }

        /// <summary>
        /// Details of the log, can be longer then summary
        /// </summary>
        /// <example></example>
        public string Details { get; set; }

        /// <summary>
        /// Ticket where it belongs
        /// </summary>
        public TicketData Ticket { get; set; }

        /// <summary>
        /// Time when log was created
        /// </summary>
        public string Time { get; set; }
    }
}
