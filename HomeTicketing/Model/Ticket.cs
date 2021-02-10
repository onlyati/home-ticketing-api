using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    /// <summary>
    ///  Model for Tickets database table
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Unique ID 
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }
        /// <summary>
        /// Category where the ticket belongs
        /// </summary>
        /// <example>Test</example>
        public int Category { get; set; }
        /// <summary>
        /// Reference value of a ticket
        /// </summary>
        /// <example>test_01</example>
        public string Reference { get; set; }
        /// <summary>
        /// Status, can be Open or Close
        /// </summary>
        /// <example></example>
        public string Status { get; set; }
        /// <summary>
        /// Time when ticket was created
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// Title of ticket about its topic
        /// </summary>
        /// <example>Test ticket</example>
        public string Title { get; set; }
    }
}
