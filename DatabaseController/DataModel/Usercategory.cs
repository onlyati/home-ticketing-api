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
        /*---------------------------------------------------------------------------------------*/
        /* Properties                                                                            */
        /*---------------------------------------------------------------------------------------*/
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

        /*---------------------------------------------------------------------------------------*/
        /* Methods                                                                               */
        /*---------------------------------------------------------------------------------------*/
        public override bool Equals(object obj)
        {
            return Equals(obj as Usercategory);
        }

        public bool Equals(Usercategory other)
        {
            if (Id != other.Id)
                return false;
            if (UserId != other.UserId)
                return false;
            if (CategoryId != other.CategoryId)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, UserId, CategoryId);
        }
    }
}
