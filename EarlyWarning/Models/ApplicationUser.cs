using EarlyWarning.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EarlyWarning.Models
{
    public class ApplicationUser : IdentityUser
    {
        [NotMapped]
        public string? RoleId { get; set; }
        [NotMapped]
        public string? Role { get; set; }
        [NotMapped]
        public string? FullName { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem>? RoleList { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid LocationId { get; set; }
        public Locations Location { get; set; }
        public LocationLevel UserLocationLevl { get; set; }
        public int UsernameChangeLimit { get; set; } = 10;
        public byte[]? ProfilePicture { get; set; }
        public string? Signature { get; set; }
    }
}
