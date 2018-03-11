using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surl.Data
{
    public class LikeQuestion
    {
        public long QuestionID { get; set; }
        public int UserID { get; set; }
        public short LikeValue { get; set; }
        public DateTime LikeTime { get; set; }
    }
}
