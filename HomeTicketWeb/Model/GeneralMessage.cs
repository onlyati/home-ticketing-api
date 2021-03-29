using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public class GeneralMessage
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
