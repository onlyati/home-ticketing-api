using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace DatabaseController.DataModel
{
    public partial class System
    {
        public System()
        {
            Categories = new HashSet<Category>();
            Tickets = new HashSet<Ticket>();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Properties                                                                            */
        /*---------------------------------------------------------------------------------------*/
        /// <summary>
        /// ID of systems, value used by database
        /// </summary>
        /// <example>1</example>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the system
        /// </summary>
        /// <example>atihome</example>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonIgnore]
        public virtual ICollection<Category> Categories { get; set; }

        [JsonIgnore]
        public virtual ICollection<Ticket> Tickets { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Methods                                                                               */
        /*---------------------------------------------------------------------------------------*/
        public override bool Equals(object obj)
        {
            return Equals(obj as System);
        }

        public bool Equals(System other)
        {
            if (Id != other.Id)
                return false;
            if (Name != other.Name)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }
}
