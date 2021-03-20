using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    public class TicketCreationInput : DatabaseController.Model.TicketCreationTemplate
    {
        /// <summary>
        /// System where the problem happened
        /// </summary>
        /// <example>test-system-1</example>
        [JsonPropertyName("system")]
        public string System { get; set; } = "";

        /// <summary>
        /// Name of category
        /// </summary>
        /// <example>Test</example>
        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = "";
    }
}
