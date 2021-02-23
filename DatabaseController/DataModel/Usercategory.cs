using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseController.DataModel
{
    /// <summary>
    /// Store the the assignments of users and categories
    /// </summary>
    public partial class Usercategory
    {
        /// <summary>
        /// ID for this table, used by database
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key point to users table
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Foreign key points to Categories
        /// </summary>
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }
        public virtual User User { get; set; }
    }
}
