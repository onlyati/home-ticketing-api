using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    /*********************************************************************************************/
    /* This page contains definition which helps to save data from OpenPage.razor                */
    /*********************************************************************************************/
    public class CreateTicket : IShareDataModel
    {
        [Required]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [Required]
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("details")]
        public string Details { get; set; }

        [Required]
        [JsonPropertyName("system")]
        public string SystemName { get; set; }

        [Required]
        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; }

        public void SetNull()
        {
            Title = null;
            Summary = null;
            Details = null;
            SystemName = null;
            CategoryName = null;
        }
    }
}
