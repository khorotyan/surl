using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Surl.Data
{
    public class ViewContext : DbContext
    {
        public ViewContext (DbContextOptions<ViewContext> options) : base(options) { }

        public DbSet<View> View { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<View>()
                .HasKey(c => new { c.QuestionID, c.UserID });
        }
    }
}
