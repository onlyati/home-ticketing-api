using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseController.Model
{
    /// <summary>
    /// Class for change ticket request
    /// </summary>
    public class TicketChangeTemplate
    {
        /// <summary>
        /// ID which shows which ticket will be changed
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Filter based on category name
        /// </summary>
        /// <example>Application</example>
        string Category { get; set; }

        /// <summary>
        /// Filter based on title
        /// </summary>
        /// <example>Brand new title</example>
        string Title { get; set; }

        /// <summary>
        /// Filter based on refernce value
        /// </summary>
        /// <example>new_ref_val</example>
        string Refernce { get; set; }
    }
}
