using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseController.Model
{
    /// <summary>
    /// Class for filter ticket request
    /// </summary>
    public class TicketFilterTemplate
    {
        /// <summary>
        /// Filter based on status, like Open and Close
        /// </summary>
        /// <example>Open</example>
        public string Status { get; set; } = "";

        /// <summary>
        /// Filter based on category name
        /// </summary>
        /// <example>Application</example>
        public string Category { get; set; } = "";

        /// <summary>
        /// Filter based on title
        /// </summary>
        /// <example>Brand new title</example>
        public string Title { get; set; } = "";

        /// <summary>
        /// Filter based on refernce value
        /// </summary>
        /// <example>new_ref_val</example>
        public string Refernce { get; set; } = "";
    }
}
