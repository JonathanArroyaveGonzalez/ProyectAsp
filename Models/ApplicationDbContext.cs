using DB_MVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DB_MVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BicicletaModel> Bicicletas { get; set; }
    }
}