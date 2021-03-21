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
    /// Class for change ticket request
    /// </summary>
    public class TicketChangeTemplate
    {
        /// <summary>
        /// ID which shows which ticket will be changed
        /// </summary>
        /// <example>1</example>
        [JsonPropertyName("id")]
        public int Id { get; set; } = -1;

        /// <summary>
        /// Filter based on category name
        /// </summary>
        /// <example>Application</example>
        [JsonIgnore]
        public Category Category { get; set; } = null;

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
        public string Refernce { get; set; } = "";

        [JsonIgnore]
        public User ChangederUser { get; set; } = null;
    }
}
