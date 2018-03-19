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
    [Route("api/Comments")]
    public class CommentsController : Controller
    {
        private readonly CommentContext _commentContext;
        private readonly QuestionContext _questionContext;
        private IConfiguration _configuration { get; }
        private SqlConnection _conn;
        private SqlCommand _comm;

        public CommentsController(CommentContext commentContext, QuestionContext questionContext, 
            IConfiguration configuration)
        {
            _commentContext = commentContext;
            _questionContext = questionContext;

            _configuration = configuration;
            _conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _comm = _conn.CreateCommand();
        }

        // POST: api/Comments/Create
        // Create a comment
        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateComment([FromBody] Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            comment.AnswerDate = DateTime.Now;

            _commentContext.Comment.Add(comment);
            await _commentContext.SaveChangesAsync();

            return Ok("Answer created successfully");
        }

        // POST: api/Comments/Edit/5
        // Edit a comment
        [Authorize]
        [HttpPost("Edit/{id:long}")]
        public async Task<IActionResult> EditComment([FromRoute] long id, [FromBody] Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbComment = await _commentContext.Comment.SingleOrDefaultAsync(c => c.CommentID == id);

            if (comment.UserID != dbComment.UserID)
            {
                return Unauthorized();
            }

            dbComment.CommentText = comment.CommentText;

            _commentContext.Entry(dbComment).State = EntityState.Modified;
            await _commentContext.SaveChangesAsync();

            return Ok("Comment updated successfully");
        }

        // POST: api/Comments/Verify/5
        // Verify a comment
        [Authorize]
        [HttpPost("Verify/{id:long}")]
        public async Task<IActionResult> VerifyComment([FromRoute] long id, [FromBody] int UserID, 
            [FromBody] bool Verified)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbComment = await _commentContext.Comment.SingleOrDefaultAsync(c => c.CommentID == id);
            var dbQuestion = await _questionContext.Question
                .SingleOrDefaultAsync(c => c.QuestionID == dbComment.QuestionID);

            if (UserID != dbQuestion.UserID)
            {
                return Unauthorized();
            }

            dbComment.Verified = Verified;

            _commentContext.Entry(dbComment).State = EntityState.Modified;
            await _commentContext.SaveChangesAsync();

            return Ok("Comment updated successfully");
        }

        // GET: api/Comments/CommentForEdit/5
        // Get a comment information later to be edited
        [Authorize]
        [HttpGet("CommentForEdit/{cID:long}/{uID:int}")]
        public async Task<IActionResult> GetCommentForEdit([FromRoute] long cID, [FromRoute] int uID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbComment = await _commentContext.Comment.SingleOrDefaultAsync(c => c.CommentID == cID);

            if (dbComment == null)
            {
                return NotFound();
            }

            if (uID != dbComment.UserID)
            {
                return Unauthorized();
            }

            return Ok(dbComment);
        }

        // GET: api/Comments/Sorted/Popular
        // Get comments
        [HttpGet("Sorted/{type:alpha}")]
        public IActionResult GetComments([FromRoute] string type)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<CommentModel> comments = new List<CommentModel>();

            try
            {
                if (type == "Popular")
                {
                    _comm.CommandText =
                        @"SELECT Comment.*, 
	                        (SELECT Username FROM [User] WHERE UserID = Comment.UserID) AS Username,
	                        (SELECT ISNULL(SUM(LikeValue), 0) FROM LikeComment WHERE CommentID = Comment.CommentID) 
	                        AS Likes FROM [Comment] ORDER BY Likes DESC;";
                }
                else if (type == "Recent")
                {
                    _comm.CommandText =
                        @"SELECT Comment.*, 
	                        (SELECT Username FROM [User] WHERE UserID = Comment.UserID) AS Username,
	                        (SELECT ISNULL(SUM(LikeValue), 0) FROM LikeComment WHERE CommentID = Comment.CommentID) 
	                        AS Likes FROM [Comment] ORDER BY AnswerDate;";
                }

                _comm.CommandType = CommandType.Text;
                _conn.Open();

                SqlDataReader reader = _comm.ExecuteReader();

                while (reader.Read())
                {
                    // Also get the username
                    CommentModel comment = new CommentModel()
                    {
                        CommentID = Convert.ToInt64(reader[0]),
                        QuestionID = Convert.ToInt64(reader[1]),
                        UserID = Convert.ToInt32(reader[2]),
                        CommentText = reader[3].ToString(),
                        AnswerDate = Convert.ToDateTime(reader[4]),
                        Verified = Convert.ToBoolean(reader[5]),
                        Username = reader[6].ToString(),
                        Likes = Convert.ToInt32(reader[7])
                    };

                    comments.Add(comment);
                }

                return Ok(comments);
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

        // POST: api/Comments/5
        // Delete a comment
        [Authorize]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteComment([FromRoute] long id, [FromBody] int UserID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbComment = await _commentContext.Comment.SingleOrDefaultAsync(c => c.CommentID == id);

            if (dbComment == null)
            {
                return NotFound();
            }

            // Only the comment poster can delete it
            // Later moderators can be added and then here can be done a check like this
            //  Check if UserID is in the moderator list, then let him/her delete the question
            if (UserID != dbComment.UserID)
            {
                return Unauthorized();
            }  

            _commentContext.Comment.Remove(dbComment);
            await _commentContext.SaveChangesAsync();

            return Ok("Comment deleted successfully");
        }
    }

    public class CommentModel
    {
        public long CommentID { get; set; }
        public long QuestionID { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string CommentText { get; set; }
        public DateTime AnswerDate { get; set; }
        public bool Verified { get; set; }
        public int Likes { get; set; }
    }
}