using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor.Data
{
    public class AppDbContext : DbContext
    {
        //public DbSet<User> Users { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //    => options.UseSqlite("Data Source=app.db");
    }
}
