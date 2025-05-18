using BibliothequeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<Livre> Livres => Set<Livre>();


}
