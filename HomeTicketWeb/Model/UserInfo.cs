using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    /*********************************************************************************************/
    /* Class which helps to store infoirmation about a user                                      */
    /*********************************************************************************************/
    public class UserInfo
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; } = null;

        [JsonPropertyName("role")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole? Role { get; set; } = null;

        [JsonPropertyName("email")]
        public string Email { get; set; } = null;
    }

    /*********************************************************************************************/
    /* Possible user role types                                                                  */
    /*********************************************************************************************/
    public enum UserRole
    {
        User,
        Admin
    }
}
