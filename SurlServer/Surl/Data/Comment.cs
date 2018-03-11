using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surl.Data
{
    public class Comment
    {
        public long CommentID { get; set; }
        public long QuestionID { get; set; }
        public int UserID { get; set; }
        public string CommentText { get; set; }
        public DateTime AnswerDate { get; set; }
        public bool Verified { get; set; }
    }
}
