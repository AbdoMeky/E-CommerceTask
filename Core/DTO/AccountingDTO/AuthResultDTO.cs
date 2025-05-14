using System.Text.Json.Serialization;

namespace Core.DTO.AccountingDTO
{
    public class AuthResultDTO
    {
        public bool IsAuthenticated { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        [JsonIgnore]
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpired { get; set; }
    }
}
