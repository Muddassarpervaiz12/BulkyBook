using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.DataAcces;
public class ApplicationDbContext :IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :base (options)
    {

    }
    public DbSet<Product> products { get; set; }
    public DbSet<Catogery> catogeries { get; set; }
    public DbSet<CoverType> coverTypes  { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Company> Companies { get; set; }
}
