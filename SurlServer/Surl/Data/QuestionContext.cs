using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Surl.Data
{
    public class QuestionContext  : DbContext
    {
        public QuestionContext(DbContextOptions<QuestionContext> options) : base(options) { }

        public DbSet<Question> Question { get; set; }
    }
}
