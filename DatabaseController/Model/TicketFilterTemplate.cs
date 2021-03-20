using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        /// <summary>
        /// Filter based on category name
        /// </summary>
        /// <example>Application</example>
        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        /// <summary>
        /// Filter based on title
        /// </summary>
        /// <example>Brand new title</example>
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        /// <summary>
        /// Filter based on refernce value
        /// </summary>
        /// <example>new_ref_val</example>
        [JsonPropertyName("reference")]
        public string Reference { get; set; } = "";

        /// <summary>
        /// System name where the ticket belongs
        /// </summary>
        /// <example>atihome.local</example>
        [JsonPropertyName("system")]
        public string System { get; set; } = "";
    }
}
