using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EarlyWarning.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string LocationName { get; set; }
        public string Email { get; set; }
        public Guid? LocationId { get; set; }
        public DateTime LockoutEnd { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
