using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ScruMster.Areas.Identity.Data;

namespace ScruMster.Data
{
    public class DbInitializer
    {
        public static void Initialize(ScruMsterContext context)
        {
            context.Database.EnsureCreated();

            // Look for any users.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }
            var adminUser = new ScruMsterUser {Email = "admin@admin.com", FirstName = "Admin", LastName = "Admin", Id = "AdminID", LockoutEnabled = false, 
                UserName = "admin@admin.com", NormalizedUserName = "ADMIN@ADMIN.COM", NormalizedEmail = "ADMIN@ADMIN.COM",
                PasswordHash = "AQAAAAEAACcQAAAAEMzu4cyqr+GKK5y5D35IokDD9QHWwwTbdOXz9Q3tif7gliP4iPsS7G/M0atzjNCu7Q=="}; // Password == "Abc_123"

            context.Users.Add(adminUser);
            context.SaveChanges();
        }
    }
}