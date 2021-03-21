using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DatabaseController.DataModel;

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
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = "";

        /// <summary>
        /// Category where it will belongs
        /// </summary>
        /// <example>Test</example>
        [JsonIgnore]
        public Category Category { get; set; } = null;

        /// <summary>
        /// Reference value to decide update log or create new ticket
        /// </summary>
        /// <example>test_01</example>
        [JsonPropertyName("reference")]
        public string Reference { get; set; } = "";

        /// <summary>
        /// Details or the ticket, can be more line
        /// </summary>
        /// <example></example>
        [JsonPropertyName("details")]
        public string Details { get; set; } = "";

        /// <summary>
        /// Short description about the topic of ticket
        /// </summary>
        /// <example>Test ticket</example>
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonIgnore]
        public User CreatorUser { get; set; } = null;
    }
}
