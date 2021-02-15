using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseController.Model
{
    /// <summary>
    /// Model for categories
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Uniqe ID of category in database
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Name of category
        /// </summary>
        /// <example>System</example>
        public string Name { get; set; }
    }
}
