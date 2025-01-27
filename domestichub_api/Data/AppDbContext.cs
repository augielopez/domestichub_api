using domestichub_api.Models;
using Microsoft.EntityFrameworkCore;

namespace domestichub_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Define DbSets for your tables
        public DbSet<Email> Emails { get; set; }
    }
}

