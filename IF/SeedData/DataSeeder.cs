using Core.Entities;
using IF.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;


namespace IF.SeedData
{
    public static class DataSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = serviceProvider.GetRequiredService<AppDbContext>())
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                await context.Database.MigrateAsync();

                await EnsureRoleExists(roleManager, "Admin");
                await EnsureRoleExists(roleManager, "User");
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string jsonFilePath = Path.Combine(basePath, "SeedData", "seeding_data.json");

                if (!File.Exists(jsonFilePath))
                {
                    Console.WriteLine("File not found: " + jsonFilePath);
                    return;
                }

                var jsonData = File.ReadAllText(jsonFilePath);
                var seedData = JsonConvert.DeserializeObject<SeedingData>(jsonData);
                if (seedData == null) return;

                await SeedAdmins(userManager, seedData);

                await SeedUsers(userManager, seedData);

                await SeedCategories(context, userManager, seedData);

                await SeedProducts(context, userManager, seedData);

                await context.SaveChangesAsync();
            }
        }

        private static async Task EnsureRoleExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private static async Task SeedAdmins(UserManager<ApplicationUser> userManager, SeedingData seedData)
        {
            var existingAdmins = await userManager.GetUsersInRoleAsync("Admin");
            if (!existingAdmins.Any())
            {
                foreach (var coach in seedData.Admins)
                {
                    var newAdmn = new ApplicationUser
                    {
                        UserName = coach.Email,
                        Email = coach.Email,
                        Name= coach.Name,
                        PhoneNumber = coach.PhoneNumber,
                        ProfilePictureUrl = coach.ProfilePictureUrl,
                    };

                    var result = await userManager.CreateAsync(newAdmn, "P@ssw0rd");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newAdmn, "Admin");
                    }
                }
            }
        }

        private static async Task SeedUsers(UserManager<ApplicationUser> userManager, SeedingData seedData)
        {
            var existingUsers = await userManager.GetUsersInRoleAsync("User");
            if (!existingUsers.Any())
            {
                foreach (var user in seedData.Users)
                {
                    var newUser = new ApplicationUser
                    {
                        UserName = user.Email,
                        Email = user.Email,
                        Name = user.Name,
                        PhoneNumber = user.PhoneNumber,
                        ProfilePictureUrl = user.ProfilePictureUrl,
                    };

                    var result = await userManager.CreateAsync(newUser, "P@ssw0rd");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, "User");
                    }
                }
            }
        }

        private static async Task SeedCategories(AppDbContext context, UserManager<ApplicationUser> userManager, SeedingData seedData)
        {
            var existingCategories = await context.Categories.AnyAsync();
            if (!existingCategories)
            {
                var categoriesList=new List<Category>();
                foreach (var category in seedData.Categories)
                {
                    categoriesList.Add(new Category { Name = category });
                }
                await context.Categories.AddRangeAsync(categoriesList);
            }
        }

        private static async Task SeedProducts(AppDbContext context, UserManager<ApplicationUser> userManager, SeedingData seedData)
        {
            var existingTrainings = await context.Products.AnyAsync();
            if (!existingTrainings)
            {
                var categories = await context.Categories.ToListAsync();
                var products= new List<Product>();
                foreach (var product in seedData.Products)
                {
                    var category = categories.FirstOrDefault(c => c.Name == product.CategoryName);
                    if (category != null)
                    {
                        products.Add(new Product
                        {
                            Name = product.Name,
                            CategoryId = category.Id,
                            Quantity = product.Quantity,
                            ImagePath = product.ImagePath,
                            Description = product.Description,
                            Price = product.Price
                        });
                    }
                }
                await context.Products.AddRangeAsync(products);
            }
        }
    }
}
