using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace DatabaseController.DataModel
{
    public partial class Category
    {
        public Category()
        {
            Tickets = new HashSet<Ticket>();
            Usercategories = new HashSet<Usercategory>();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Properties                                                                            */
        /*---------------------------------------------------------------------------------------*/
        /// <summary>
        /// ID of category, used by database
        /// </summary>
        /// <example>1</example>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of category
        /// </summary>
        /// <example>Systgem</example>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// System ID, used as foreign key to point system in another table
        /// </summary>
        /// <example>5</example>
        [JsonIgnore]
        public int? SystemId { get; set; }

        [JsonPropertyName("system")]
        public virtual System System { get; set; }

        [JsonIgnore]
        public virtual ICollection<Ticket> Tickets { get; set; }

        [JsonIgnore]
        public virtual ICollection<Usercategory> Usercategories { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Methods                                                                               */
        /*---------------------------------------------------------------------------------------*/
        public override bool Equals(object obj)
        {
            return Equals(obj as Category);
        }

        public bool Equals(Category other)
        {
            if (Id != other.Id)
                return false;
            if (Name != other.Name)
                return false;
            if (SystemId != other.SystemId)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, SystemId);
        }
    }
}
