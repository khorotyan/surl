using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surl.Data
{
    public class LikeComment
    {
        public long CommentID { get; set; }
        public int UserID { get; set; }
        public short LikeValue { get; set; }
        public DateTime LikeTime { get; set; }
    }
}
