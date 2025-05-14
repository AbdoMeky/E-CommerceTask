using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helper
{
    public class JwtSettings
    {
        public string? SecretKey { get; set; } //= string.Empty;
        public string? ValidAudiance { get; set; } //= string.Empty;
        public string? ValidIssuer { get; set; } //= string.Empty;
    }
}
