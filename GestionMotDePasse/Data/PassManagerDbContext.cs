using GestionMotDePasse.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionMotDePasse.Data
{
    public class PassManagerDbContext : DbContext   
    {
        public DbSet<PasswordEntry> Passwords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionString: "Data Source=passmanager.db");
        }
    }
}
