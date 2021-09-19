using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    public class Counter
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
