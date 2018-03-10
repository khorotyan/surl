using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surl.Data
{
    public class Question
    {
        public long QuestionID { get; set; }
        public int UserID { get; set; }
        public string QuestionText { get; set; }
        public string Description { get; set; }
        public DateTime PostDate { get; set; }
    }
}
