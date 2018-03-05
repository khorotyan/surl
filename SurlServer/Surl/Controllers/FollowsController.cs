using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Surl.Data;

namespace Surl.Controllers
{
    [Produces("application/json")]
    [Route("api/Follows")]
    public class FollowsController : Controller
    {
        private readonly FollowContext _context;
        private IConfiguration _configuration { get; }
        private SqlConnection _conn;
        private SqlCommand _comm;

        public FollowsController(FollowContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            _conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _comm = _conn.CreateCommand();
        }

        // POST: api/Follows/Follow
        // Follow a user
        //[Authorize]
        [HttpPost("Follow")]
        public async Task<IActionResult> FollowUser([FromBody] Follow follow)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            follow.FollowDate = DateTime.Now;

            _context.Follow.Add(follow);
            await _context.SaveChangesAsync();

            return Ok("Successfully followed a user");
        }

        // DELETE: api/Follows/Unfollow
        // Unfollow a user
        //[Authorize]
        [HttpDelete("Unfollow")]
        public async Task<IActionResult> DeleteFollow([FromBody] Follow follow)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbFollow = await _context.Follow.SingleOrDefaultAsync(m => 
                m.FollowingID == follow.FollowingID && m.FollowedID == follow.FollowedID);

            if (dbFollow == null)
            {
                return NotFound();
            }

            _context.Follow.Remove(dbFollow);
            await _context.SaveChangesAsync();

            return Ok("Successfully unfollowed a user");
        }

        // GET: api/Follows/Followers/5
        // Get the user's followers
        //[Authorize]
        [HttpGet("Followers/{id:int}")]
        public IActionResult GetFollowers([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<FollowerModel> users = new List<FollowerModel>();

            try
            {
                 _comm.CommandText = 
                    @"SELECT NewUsers.*, DATEDIFF(MINUTE, FollowDate, GETDATE()) AS FollowMins,
	                    CASE WHEN UserID IN 
		                    (SELECT FollowedID FROM [Follow] WHERE FollowingID = @userID) 
		                    THEN 1 ELSE 0 END AS FollowingUser
	                    FROM (SELECT TOP 100 PERCENT UserID, Username, COUNT(FollowedID) AS FollowerNum 
		                    FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
		                    GROUP BY Username, UserID ORDER BY FollowerNum DESC) AS NewUsers
	                    LEFT JOIN [Follow] ON (FollowingID = NewUsers.UserID) WHERE FollowedID = @userID ORDER BY FollowDate DESC;";
                _comm.Parameters.AddWithValue("@userID", id);

                _comm.CommandType = CommandType.Text;
                _conn.Open();

                SqlDataReader reader = _comm.ExecuteReader();

                while (reader.Read())
                {
                    FollowerModel user = new FollowerModel()
                    {
                        UserID = Convert.ToInt32(reader[0]),
                        Username = reader[1].ToString(),
                        FollowerNum = Convert.ToInt32(reader[2]),
                        FollowMins = Convert.ToInt32(reader[3]),
                        FollowingUser = Convert.ToBoolean(reader[4])
                    };

                    users.Add(user);
                }

                return Ok(users);
            }
            catch (Exception)
            {
                return BadRequest(ModelState);
            }
            finally
            {
                if (_conn != null)
                {
                    _conn.Close();
                }
            }
        }

        // GET: api/Follows/Followings/5
        // Get the users whom the user follows
        //[Authorize]
        [HttpGet("Followings/{id:int}")]
        public IActionResult GetFollowings([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<FollowerModel> users = new List<FollowerModel>();

            try
            {
                _comm.CommandText =
                   @"SELECT NewUsers.*, DATEDIFF(MINUTE, FollowDate, GETDATE()) AS FollowMins
	                    FROM (SELECT TOP 100 PERCENT UserID, Username, COUNT(FollowedID) AS FollowerNum 
		                    FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
		                    GROUP BY Username, UserID ORDER BY FollowerNum DESC) AS NewUsers
	                    LEFT JOIN [Follow] ON (FollowedID = NewUsers.UserID) WHERE FollowingID = @userID ORDER BY FollowDate DESC;";
                _comm.Parameters.AddWithValue("@userID", id);

                _comm.CommandType = CommandType.Text;
                _conn.Open();

                SqlDataReader reader = _comm.ExecuteReader();

                while (reader.Read())
                {
                    FollowerModel user = new FollowerModel()
                    {
                        UserID = Convert.ToInt32(reader[0]),
                        Username = reader[1].ToString(),
                        FollowerNum = Convert.ToInt32(reader[2]),
                        FollowMins = Convert.ToInt32(reader[3])
                    };

                    users.Add(user);
                }

                return Ok(users);
            }
            catch (Exception)
            {
                return BadRequest(ModelState);
            }
            finally
            {
                if (_conn != null)
                {
                    _conn.Close();
                }
            }
        }
    }

    public class FollowerModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public int FollowerNum { get; set; }
        public int FollowMins { get; set; }
        public bool FollowingUser { get; set; }
    }
}