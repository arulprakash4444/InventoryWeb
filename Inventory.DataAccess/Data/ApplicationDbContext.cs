using Inventory.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Inventory.DataAccess.Data
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
            
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<UserNotification> UserNotifications { get; set; }

        public DbSet<LoginAttempt> LoginAttempts { get; set; }

        public DbSet<CarouselItem> CarouselItems { get; set; }

        public DbSet<RestaurantOrder> RestaurantOrders { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // Ensure MobileNumber is unique
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.PhoneNumber) // or u.MobileNumber if you added a separate property
                .IsUnique();

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "drinks" },
                new Category { Id = 2, Name = "biscuits" },
                new Category { Id = 3, Name = "chocolates" },
                new Category { Id = 4, Name = "chips" }
                );



        //    modelBuilder.Entity<Product>()
        //.HasOne(p => p.Category)
        //.WithMany() // because Category has no Products navigation
        //.HasForeignKey(p => p.CategoryId)
        //.OnDelete(DeleteBehavior.SetNull); // or Restict


            modelBuilder.Entity<Product>().HasData(
               new Product
               {
                   Id = 1,
                   Name = "Pepsi",
                   Stock = 0,
                   Price = 40.00m,
                   Description = "750ml bottle Pepsi",
                   LastAdded = null,
                   LastRemoved = null,
                   CategoryId = 1,
                   ImageUrl = null
               },
               new Product
               {
                   Id = 2,
                   Name = "dark choc",
                   Stock = 0,
                   Price = 119.50m,
                   Description = "120g",
                   LastAdded = null,
                   LastRemoved = null,
                   CategoryId = 3,
                   ImageUrl = null
               }
               );
        }


    }
}
