using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Surl.Data;
using Surl.Middleware;

namespace Surl.Controllers
{
    [Produces("application/json")]
    [Route("api/Users")]
    public class UsersController : Controller
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly UserContext _context;
        private SqlConnection _conn;
        private SqlCommand _comm;

        public UsersController(UserContext context, IConfiguration configuration, ILogger<UsersController> logger)
        {
            _logger = logger;

            _context = context;
            _configuration = configuration;

            _conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _comm = _conn.CreateCommand();
        }

        // POST: api/Users/Create
        // Create a user
        [HttpPost("Create")]
        public async Task<IActionResult> CreateUser([FromForm] User user)
        {
            //_logger.LogDebug("Some Log");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbUser = await _context.User.SingleOrDefaultAsync(c => c.Username == user.Username);

            if (dbUser != null)
            {
                var response = new ContentResult()
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Content = "Username already exists"
                };

                return response; 
            }

            user.Password = SecurePasswordHasher.Hash(user.Password);
            user.RegisterDate = DateTime.Now;

            _context.User.Add(user);
            await _context.SaveChangesAsync();
            
            var token = Authentication.GenerateToken(user.Username, _configuration);

            var retVal = new {
                user.UserID,
                user.Email,
                user.Username,
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };

            return Ok(retVal);
        }

        // POST: api/Users/LoAugin
        // Login a user
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbUser = await _context.User.SingleOrDefaultAsync(c => c.Email == user.Email);

            bool verified = SecurePasswordHasher.Verify(user.Password, dbUser.Password);

            if (verified == true)
            {
                var token = Authentication.GenerateToken(dbUser.Username, _configuration);

                var retVal = new
                {
                    dbUser.UserID,
                    dbUser.Email,
                    dbUser.Username,
                    Token = new JwtSecurityTokenHandler().WriteToken(token)
                };

                return Ok(retVal);
            }
            else
            {
                return Unauthorized();
            }
        }

        // GET: api/Users
        // Get all the users
        [Authorize]
        [HttpGet]
        public IActionResult GetUsers()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var users = from User in _context.User select new { User.UserID, User.Username, User.RegisterDate };

            return Ok(users);
        }

        // GET: api/Users/5
        // Get information about a specific user
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.User.SingleOrDefaultAsync(m => m.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/Users/Top
        // Get the users with the most followers
        [Authorize]
        [HttpGet("Top/{id:int}")]
        public IActionResult GetTopUsers([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<FollowerModel> users = new List<FollowerModel>();

            try
            {
                _comm.CommandText =
                   @"SELECT UserID, Username, COUNT(FollowedID) AS FollowerNum,
	                    MAX(CASE WHEN FollowingID = @userID THEN 1 ELSE 0 END) AS FollowingUser
	                    FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
	                    GROUP BY UserID, Username ORDER BY FollowingUser, FollowerNum DESC;";
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
                        FollowingUser = Convert.ToBoolean(reader[3])
                    };

                    if (user.UserID != id)
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

        // GET: api/Users/Search/5/somesearch
        // Search for a user
        [Authorize]
        [HttpGet("Search/{id:int}/{search}")]
        public IActionResult Search([FromRoute] int id, [FromRoute] string search)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<FollowerModel> users = new List<FollowerModel>();

            try
            {
                _comm.CommandText = String.Format(
                   @"SELECT UserID, Username, COUNT(FollowedID) AS FollowerNum,
	                    MAX(CASE WHEN FollowingID = @userID THEN 1 ELSE 0 END) AS FollowingUser
	                    FROM [User] LEFT JOIN [Follow] ON (FollowedID = UserID) 
	                    WHERE Username LIKE '%{0}%' 
	                    GROUP BY UserID, Username  ORDER BY FollowerNum DESC, LEN(Username);", search);
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
                        FollowingUser = Convert.ToBoolean(reader[3])
                    };

                    if (user.UserID != id)
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
}
