using Microsoft.EntityFrameworkCore;
using ProcessMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options) { }
        public DbSet<Analysis> Analyses { get; set; }

    }
}
