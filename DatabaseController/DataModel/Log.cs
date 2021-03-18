using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace DatabaseController.DataModel
{
    public partial class Log
    {
        /*---------------------------------------------------------------------------------------*/
        /* Properties                                                                            */
        /*---------------------------------------------------------------------------------------*/
        /// <summary>
        /// ID of the log, valuse used by database
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Ticket short summary
        /// </summary>
        /// <example>This is a short summary</example>
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        /// <summary>
        /// Used as foreign key to recognize which ticket its belongs
        /// </summary>
        /// <example>1</example>
        [JsonPropertyName("ticket_id")]
        public int TicketId { get; set; }

        /// <summary>
        /// Longer description about the problem
        /// </summary>
        /// <example>This is a loooooooooooooooooonger description</example>
        [JsonPropertyName("details")]
        public string Details { get; set; }

        /// <summary>
        /// Timestamp when the log was recorded
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime? Time { get; set; }

        /// <summary>
        /// Point, that by who created the log entry
        /// </summary>
        [JsonPropertyName("user_id")]
        public int? UserId { get; set; }

        [JsonIgnore]
        public virtual Ticket Ticket { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Methods                                                                               */
        /*---------------------------------------------------------------------------------------*/
        public override bool Equals(object obj)
        {
            return Equals(obj as Log);
        }

        public bool Equals(Log other)
        {
            if (Id != other.Id)
                return false;
            if (Summary != other.Summary)
                return false;
            if (TicketId != other.TicketId)
                return false;
            if (Details != other.Details)
                return false;
            if (Time != other.Time)
                return false;
            if (UserId != other.UserId)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Summary, TicketId, Details, Time, UserId);
        }
    }
}
