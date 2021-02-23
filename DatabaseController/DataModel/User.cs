using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseController.DataModel
{
    public partial class User
    {
        public User()
        {
            Logs = new HashSet<Log>();
            Tickets = new HashSet<Ticket>();
            Usercategories = new HashSet<Usercategory>();
        }

        /// <summary>
        /// ID of user entry, used by database
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Name of the user
        /// </summary>
        /// <example>ati</example>
        public string Username { get; set; }

        /// <summary>
        /// Password has stored on 512 bytes
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Email address of user
        /// </summary>
        /// <example>ati@atihome.local</example>
        public string Email { get; set; }

        public virtual ICollection<Log> Logs { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<Usercategory> Usercategories { get; set; }
    }
}
