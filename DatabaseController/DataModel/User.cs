﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DatabaseController.Model;

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

        /*---------------------------------------------------------------------------------------*/
        /* Properties                                                                            */
        /*---------------------------------------------------------------------------------------*/
        /// <summary>
        /// ID of user entry, used by database
        /// </summary>
        /// <example>1</example>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the user
        /// </summary>
        /// <example>ati</example>
        [JsonPropertyName("username")]
        public string Username { get; set; }

        /// <summary>
        /// Password has stored on 512 bytes
        /// </summary>
        [JsonIgnore]
        public string Password { get; set; }

        /// <summary>
        /// Email address of user
        /// </summary>
        /// <example>ati@atihome.local</example>
        [JsonPropertyName("email")]
        public string Email { get; set; }

        /// <summary>
        /// Enum type for user role
        /// </summary>
        /// <example>Admin</example>
        [JsonPropertyName("role")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; }

        [JsonIgnore]
        public virtual ICollection<Log> Logs { get; set; }

        [JsonIgnore]
        public virtual ICollection<Ticket> Tickets { get; set; }

        [JsonIgnore]
        public virtual ICollection<Usercategory> Usercategories { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Methods                                                                               */
        /*---------------------------------------------------------------------------------------*/
        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }

        public bool Equals(User other)
        {
            if (Id != other.Id)
                return false;
            if (Username != other.Username)
                return false;
            if (Password != other.Password)
                return false;
            if (Email != other.Email)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Username, Password, Email);
        }
    }
}
