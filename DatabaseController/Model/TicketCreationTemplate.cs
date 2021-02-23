using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseController.Model
{
    /// <summary>
    /// Class to understand JSON object for ticket creation
    /// </summary>
    public class TicketCreationTemplate
    {
        /// <summary>
        /// Summary of ticket
        /// </summary>
        /// <example>This is a test ticket</example>
        public string Summary { get; set; } = "";
        /// <summary>
        /// Category where it will belongs
        /// </summary>
        /// <example>Test</example>
        public string Category { get; set; } = "";
        /// <summary>
        /// Reference value to decide update log or create new ticket
        /// </summary>
        /// <example>test_01</example>
        public string Reference { get; set; } = "";
        /// <summary>
        /// Details or the ticket, can be more line
        /// </summary>
        /// <example></example>
        public string Details { get; set; } = "";
        /// <summary>
        /// Short description about the topic of ticket
        /// </summary>
        /// <example>Test ticket</example>
        public string Title { get; set; } = "";
        /// <summary>
        /// System name where the ticket belongs
        /// </summary>
        /// <example>atihome.local</example>
        public string System { get; set; } = "";
    }
}
