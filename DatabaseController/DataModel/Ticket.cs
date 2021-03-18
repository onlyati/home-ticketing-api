using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace DatabaseController.DataModel
{
    public partial class Ticket
    {
        public Ticket()
        {
            Logs = new HashSet<Log>();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Properties                                                                            */
        /*---------------------------------------------------------------------------------------*/
        /// <summary>
        /// ID of ticket, value used by database
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Used as foreign key, points to category entry
        /// </summary>
        [JsonPropertyName("category_id")]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Reference value to identify which ticket are belongs to one issue
        /// </summary>
        /// <example>memory-shortage</example>
        [JsonPropertyName("reference")]
        public string Reference { get; set; }

        /// <summary>
        /// Status can be Open or Close
        /// </summary>
        /// <example>Open</example>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// Time when the ticket was created
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        /// <summary>
        /// Title for ticket, it can be seen if it is listed
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Record which user created the ticket, used as foreign key to users table
        /// </summary>
        [JsonPropertyName("user_id")]
        public int? UserId { get; set; }

        /// <summary>
        /// Record where the ticket is created, used as foreign key to systems table
        /// </summary>
        [JsonPropertyName("system_id")]
        public int SystemId { get; set; }

        [JsonIgnore]
        public virtual Category Category { get; set; }

        [JsonIgnore]
        public virtual System System { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }

        [JsonIgnore]
        public virtual ICollection<Log> Logs { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Methods                                                                               */
        /*---------------------------------------------------------------------------------------*/
        public override bool Equals(object obj)
        {
            return Equals(obj as Ticket);
        }

        public bool Equals(Ticket other)
        {
            if (Id != other.Id)
                return false;
            if (CategoryId != other.CategoryId)
                return false;
            if (Reference != other.Reference)
                return false;
            if (Status != other.Status)
                return false;
            if (Time != other.Time)
                return false;
            if (Title != other.Title)
                return false;
            if (UserId != other.UserId)
                return false;
            if (SystemId != other.SystemId)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, CategoryId, Reference, Status, Time, Title, UserId, SystemId);
        }
    }
}
