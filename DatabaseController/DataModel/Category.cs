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

        /// <summary>
        /// ID of category, used by database
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Name of category
        /// </summary>
        /// <example>Systgem</example>
        public string Name { get; set; }

        /// <summary>
        /// System ID, used as foreign key to point system in another table
        /// </summary>
        /// <example>5</example>
        public int? SystemId { get; set; }

        public virtual System System { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }

        public virtual ICollection<Usercategory> Usercategories { get; set; }
    }
}
