using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseController.Model
{
    public enum UserRole
    {
        Admin,
        User
    }

    // Add this object to use "AllowAuthorized" as attribute in REST API endpoints
    public class AllowAuthorizedAttribute : AuthorizeAttribute
    {
        public AllowAuthorizedAttribute(UserRole role) : base()
        {
            Roles = role.ToString();
        }
    }
}
