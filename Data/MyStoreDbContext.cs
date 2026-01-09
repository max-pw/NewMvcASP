using Microsoft.EntityFrameworkCore;
using MyStoreMVC.Models;
using System.Collections.Generic;

namespace MyStoreMVC.Data
{
    public class MyStoreDbContext : DbContext
    {
        public MyStoreDbContext(DbContextOptions<MyStoreDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
    }
}
