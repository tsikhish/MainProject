using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace MvcProject.Models.DbContext
{
    public class CasinoDbContext : IdentityDbContext
    {
        public CasinoDbContext(DbContextOptions<CasinoDbContext> options) : base(options)
        {
        }
        public DbSet<Wallet> Wallets { get; set; }
    }
}
