namespace Blog.Migrations
{
    using Blog.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Blog.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Models.ApplicationDbContext context)
        {
            // Classes to work with users and roles (provided by Microsoft packages)
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            //Check if the roles are already created.
            //If not, create them.
            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            if (!context.Roles.Any(r => r.Name == "Moderator"))
            {
                roleManager.Create(new IdentityRole { Name = "Moderator" });
            }

            //Check if the admin user is already created.
            //If not, create it.
            ApplicationUser adminUser = null;
            //If not, add it.

           
            if (!context.Users.Any(p => p.UserName == "admin@myblogapp.com"))
            {
                adminUser = new ApplicationUser();
                adminUser.UserName = "admin@myblogapp.com";
                adminUser.Email = "admin@myblogapp.com";
                adminUser.FirstName = "Admin";
                adminUser.LastName = "User";
                adminUser.DisplayName = "Admin User";

                userManager.Create(adminUser, "Password-1");
            }
            else
            {
                adminUser = context.Users.Where(p => p.UserName == "admin@myblogapp.com")
                    .FirstOrDefault();
            }

            //Check if the adminUser is already on the Admin role
            if (!userManager.IsInRole(adminUser.Id, "Admin"))
            {
                userManager.AddToRole(adminUser.Id, "Admin");
            }

            ApplicationUser moderator = null;

            if (!context.Users.Any(p => p.UserName == "h.patel405.hp@gmail.com"))
            {
                moderator = new ApplicationUser();
                moderator.UserName = "h.patel405.hp@gmail.com";
                moderator.Email = "h.patel405.hp@gmail.com";
                moderator.FirstName = "Harsh";
                moderator.LastName = "Patel";
                moderator.DisplayName = "Harsh";

                userManager.Create(moderator, "HarshXyZ@12");
            }
            else
            {
                moderator = context.Users.Where(p => p.UserName == "h.patel405.hp@gmail.com")
                    .FirstOrDefault();
            }

            //Check if the adminUser is already on the Admin role
            //If not, add it.

            if (!userManager.IsInRole(moderator.Id, "Moderator"))
            {
                userManager.AddToRole(moderator.Id, "Moderator");
            }

        }
    }
}

