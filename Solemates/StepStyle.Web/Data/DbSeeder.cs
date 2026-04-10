using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StepStyle.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StepStyle.Web.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Brands.Any() || context.Categories.Any())
            {
                return;
            }

            var nike = new Brand { Name = "Nike" };
            var adidas = new Brand { Name = "Adidas" };
            context.Brands.AddRange(nike, adidas);

            var sneakers = new Category { Name = "Кросівки" };
            var boots = new Category { Name = "Черевики" };
            context.Categories.AddRange(sneakers, boots);

            var size41 = new Size { Value = "41" };
            var size42 = new Size { Value = "42" };
            context.Sizes.AddRange(size41, size42);

            await context.SaveChangesAsync();

            var product1 = new Product
            {
                Name = "Nike Air Force 1",
                Description = "Класичні стильні кросівки на кожен день.",
                Price = 4500.00m,
                SKU = "NK-AF1-WHT",
                Gender = Gender.Unisex,
                BrandId = nike.Id,
                CategoryId = sneakers.Id
            };
            context.Products.Add(product1);
            await context.SaveChangesAsync();

            var variant1 = new ProductVariant
            {
                ProductId = product1.Id,
                SizeId = size42.Id,
                QuantityInStock = 15
            };
            context.ProductVariants.Add(variant1);

            await context.SaveChangesAsync();
        }

        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            string adminEmail = "admin@stepstyle.com";
            string adminPassword = "Password123!"; 

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true 
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}