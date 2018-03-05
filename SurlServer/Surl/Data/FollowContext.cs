using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Surl.Data
{
    public class FollowContext : DbContext
    {
        public FollowContext (DbContextOptions<FollowContext> options) : base(options) { }

        public DbSet<Follow> Follow { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Follow>()
                .HasKey(c => new { c.FollowingID, c.FollowedID });
        }
    }
}
