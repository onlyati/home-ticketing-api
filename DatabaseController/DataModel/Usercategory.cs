using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key point to users table
        /// </summary>
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// Foreign key points to Categories
        /// </summary>
        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonIgnore]
        public virtual Category Category { get; set; }

        [JsonIgnore]
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
