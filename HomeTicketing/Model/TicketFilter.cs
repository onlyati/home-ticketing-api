using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    /// <summary>
    /// Class to understand the received JSON object for filtering ticket
    /// </summary>
    public class TicketFilter
    {
        /// <summary>
        /// Category, where it will belongs
        /// </summary>
        /// <example>Test</example>
        public string Category { get; set; }
        /// <summary>
        /// Refernce value
        /// </summary>
        /// <example>test_01</example>
        public string Reference { get; set; }
        /// <summary>
        /// Status can be Open or Close or empty
        /// </summary>
        /// <example></example>
        public string Status { get; set; }
        /// <summary>
        /// Short description about the topic of ticket
        /// </summary>
        /// <example>Test ticket</example>
        public string Title { get; set; }
    }
}
