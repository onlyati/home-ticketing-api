using DatabaseController.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    public class TicketListItem : Ticket
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; }
    }
}
