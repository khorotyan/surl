using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Surl.Data
{
    public class Follow
    {
        public int FollowingID { get; set; }
        public int FollowedID { get; set; }
        public DateTime FollowDate { get; set; }
    }
}
