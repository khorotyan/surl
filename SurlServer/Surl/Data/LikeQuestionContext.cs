using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Surl.Data
{
    public class LikeQuestionContext : DbContext
    {
        public LikeQuestionContext(DbContextOptions<LikeQuestionContext> options) : base(options) { }

        public DbSet<LikeQuestion> LikeQuestion { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LikeQuestion>()
                .HasKey(c => new { c.QuestionID, c.UserID });
        }
    }
}
