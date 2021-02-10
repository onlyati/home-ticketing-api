using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    /// <summary>
    /// Class to receive change request from JSON
    /// </summary>
    public class TicketChangeData
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }
        /// <summary>
        /// Category where the change belongs
        /// </summary>
        /// <example>Test</example>
        public string Category { get; set; }
        /// <summary>
        /// Reference value of ticket
        /// </summary>
        /// <example>test_01</example>
        public string Reference { get; set; }
        /// <summary>
        /// Short description about the topic of ticket
        /// </summary>
        /// <example>Test ticket</example>
        public string Title { get; set; }
    }
}
