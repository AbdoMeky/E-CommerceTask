using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public ICollection<Order>Orders { get; set; }=new List<Order>();
        public ICollection<RefreshToken>? RefreshTokens { get; set; }=new List<RefreshToken>();
    }
}
