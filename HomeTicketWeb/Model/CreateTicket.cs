using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public class CreateTicket
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Summary { get; set; }

        public string Details { get; set; }

        [Required]
        public string SystemName { get; set; }

        [Required]
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
