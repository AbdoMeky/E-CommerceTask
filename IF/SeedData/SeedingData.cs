using Core.Entities;
using IF.SeedData.SeedingHelper;

namespace IF.SeedData
{
    public class SeedingData
    {
        public List<ApplicationUser> Admins { get; set; } = new();
        public List<ApplicationUser> Users { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<SeedingProduct> Products { get; set; } = new();
    }
}
