using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    /// <summary>
    /// Class to store the header of the ticket what is the first message and meta data
    /// </summary>
    public class TicketHeader
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }
        /// <summary>
        /// Category where ticket belongs
        /// </summary>
        /// <example>Test</example>
        public string Category { get; set; }
        /// <summary>
        /// Reference value to help to decide that new ticket or update the old one
        /// </summary>
        /// <example>test_01</example>
        public string Reference { get; set; }
        /// <summary>
        /// Can be Open or Close or empty
        /// </summary>
        /// <example></example>
        public string Status { get; set; }
        /// <summary>
        /// Time when ticket was created
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// Short description about the topic of ticket
        /// </summary>
        /// <example>Test ticket</example>
        public string Title { get; set; }
    }
}
