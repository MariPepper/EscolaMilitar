using Microsoft.EntityFrameworkCore;
using EscolaMilitar.Models; // Ensure you have the correct namespace

namespace EscolaMilitar.Data
{
    public class EscolaMilitarContext : DbContext
    {
        public EscolaMilitarContext(DbContextOptions<EscolaMilitarContext> options) : base(options) { }

        public DbSet<Militares> Militares { get; set; }
        public DbSet<Registos> Registos { get; set; }
    }
}


