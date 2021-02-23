using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseController.DataModel
{
    public partial class Log
    {
        /// <summary>
        /// ID of the log, valuse used by database
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ticket short summary
        /// </summary>
        /// <example>This is a short summary</example>
        public string Summary { get; set; }

        /// <summary>
        /// Used as foreign key to recognize which ticket its belongs
        /// </summary>
        /// <example>1</example>
        public int TicketId { get; set; }

        /// <summary>
        /// Longer description about the problem
        /// </summary>
        /// <example>This is a loooooooooooooooooonger description</example>
        public string Details { get; set; }

        /// <summary>
        /// Timestamp when the log was recorded
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// Point, that by who created the log entry
        /// </summary>
        public int? UserId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual User User { get; set; }
    }
}
