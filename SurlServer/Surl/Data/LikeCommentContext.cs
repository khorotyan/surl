using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Surl.Data
{
    public class LikeCommentContext : DbContext
    {
        public LikeCommentContext(DbContextOptions<LikeCommentContext> options) : base(options) { }

        public DbSet<LikeComment> LikeComment { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LikeComment>()
                .HasKey(c => new { c.CommentID, c.UserID });
        }
    }
}
