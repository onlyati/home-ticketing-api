using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketing.Model
{
    /// <summary>
    /// Model for Categories database table
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Category ID
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }
        /// <summary>
        /// Category name
        /// </summary>
        /// <example>Test</example>
        public string Name { get; set; }
    }
}
