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
    [Route("api/Questions")]
    public class QuestionsController : Controller
    {
        private readonly QuestionContext _context;
        private IConfiguration _configuration { get; }
        private SqlConnection _conn;
        private SqlCommand _comm;

        public QuestionsController(QuestionContext context, IConfiguration configuration)
        {
            _context = context;

            _configuration = configuration;
            _conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _comm = _conn.CreateCommand();
        }

        // POST: api/Questions/Create
        // Create a question
        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateQuestion([FromBody] Question question)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            question.PostDate = DateTime.Now;

            _context.Question.Add(question);
            await _context.SaveChangesAsync();

            return Ok("Question created successfully");
        }

        // POST: api/Questions/Edit/5
        // Edit question, description and tags
        [Authorize]
        [HttpPost("Edit/{id:long}")]
        public async Task<IActionResult> EditQuestion([FromRoute] long id, [FromBody] Question question)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbQuestion = await _context.Question.SingleOrDefaultAsync(c => c.QuestionID == id);

            // Only the question poster can edit it
            // Later moderators can be added and then here can be done a check like this
            //  Check if UserID is in the moderator list, then let him/her edit the question
            if (question.UserID != dbQuestion.UserID)
            {
                return Unauthorized();
            }
            
            dbQuestion.QuestionText = question.QuestionText;
            dbQuestion.Description = question.Description;
            dbQuestion.Tags = question.Tags;

            _context.Entry(dbQuestion).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Question updated successfully");
        }

        // GET: api/Questions/QuestionForEdit/5
        // Get a question later to be edited
        [Authorize]
        [HttpGet("QuestionForEdit/{qID:long}/{uID:int}")]
        public async Task<IActionResult> GetQuestionForEdit([FromRoute] long qID, [FromRoute] int uID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbQuestion = await _context.Question.SingleOrDefaultAsync(c => c.QuestionID == qID);

            if (dbQuestion == null)
            {
                return NotFound();
            }

            if (uID != dbQuestion.UserID)
            {
                return Unauthorized();
            }

            return Ok(dbQuestion);
        }

        // GET: api/Questions/5
        // Get a specific question
        [HttpGet("{id:long}")]
        public IActionResult GetQuestion([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            QuestionModel question = new QuestionModel();

            try
            {
                _comm.CommandText =
                    @"SELECT *,
	                    (SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
	                    (SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
	                    (SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes
	                    FROM [Question] AS Q,
	                    (SELECT COUNT(*) AS Answers, ISNULL(MAX(CAST(Verified AS INT)), 0) AS Verified 
                            FROM [Comment] WHERE QuestionID = @questionID) AS C 
	                    WHERE Q.QuestionID = @questionID;";
                _comm.Parameters.AddWithValue("@questionID", id);

                _comm.CommandType = CommandType.Text;
                _conn.Open();

                SqlDataReader reader = _comm.ExecuteReader();

                while (reader.Read())
                {
                    question.QuestionID = Convert.ToInt64(reader[0]);
                    question.UserID = Convert.ToInt32(reader[1]);
                    question.QuestionText = reader[2].ToString();
                    question.Description = reader[3].ToString();
                    question.Tags = reader[4].ToString();
                    question.PostDate = Convert.ToDateTime(reader[5]);
                    question.Answers = Convert.ToInt32(reader[6]);
                    question.Verified = Convert.ToBoolean(reader[7]);
                    question.Username = reader[8].ToString();
                    question.Views = Convert.ToInt32(reader[9]);
                    question.Likes = Convert.ToInt32(reader[10]);
                }

                return Ok(question);
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

        // GET: api/Questions/Sorted/Popular/0/Search
        // Sort and Search
        [HttpGet("Sorted/{type:alpha}/{id:int}/{search}")]
        public IActionResult GetQuestions([FromRoute] string type, [FromRoute] int id, [FromRoute] string search)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<QuestionModel> questions = new List<QuestionModel>();

            try
            {
                switch (type)
                {
                    case "Popular":
                        _comm.CommandText =
                            @"SELECT * FROM
	                            (SELECT TOP 100 PERCENT Q.*,
		                            (SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
 		                            (SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		                            (SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		                            (SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		                            (SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		                            FROM [Question] AS Q 
		                            ORDER BY Likes DESC) AS NewQuestion";

                        if (search != "_")
                            _comm.CommandText +=
                                $" WHERE CONCAT_WS('', QuestionText, Tags, Username, Description) LIKE '%{search}%'";

                        break;

                    case "Recent":
                        _comm.CommandText =
                            @"SELECT * FROM
	                            (SELECT TOP 100 PERCENT Q.*,
		                            (SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		                            (SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		                            (SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		                            (SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		                            (SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		                            FROM [Question] AS Q 
		                            ORDER BY Q.PostDate DESC) AS NewQuestion";

                        if (search != "_")
                            _comm.CommandText +=
                                $" WHERE CONCAT_WS('', QuestionText, Tags, Username, Description) LIKE '%{search}%'";

                        break;

                    case "Friend":
                        _comm.CommandText =
                            @"SELECT * FROM
	                            (SELECT TOP 100 PERCENT Q.*,
		                            (SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		                            (SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		                            (SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		                            (SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		                            (SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		                            FROM [Question] AS Q
		                            WHERE Q.UserID IN (SELECT FollowedID FROM [Follow] WHERE FollowingID = @userID)
		                            ORDER BY Q.PostDate DESC) AS NewQuestion";
                        _comm.Parameters.AddWithValue("@userID", id);

                        if (search != "_")
                            _comm.CommandText +=
                                $" WHERE CONCAT_WS('', QuestionText, Tags, Username, Description) LIKE '%{search}%'";

                        break;

                    case "Own":
                        _comm.CommandText =
                            @"SELECT * FROM
	                            (SELECT TOP 100 PERCENT Q.*,
		                            (SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		                            (SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		                            (SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		                            (SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		                            (SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		                            FROM [Question] AS Q 
		                            WHERE Q.UserID = @userID
		                            ORDER BY Q.PostDate DESC) AS NewQuestion";
                        _comm.Parameters.AddWithValue("@userID", id);

                        if (search != "_")
                            _comm.CommandText +=
                                $" WHERE CONCAT_WS('', QuestionText, Tags, Username, Description) LIKE '%{search}%'";

                        break;

                    case "Trending":
                        _comm.CommandText =
                            @"SELECT * FROM
	                            (SELECT TOP 100 PERCENT
		                            Q.*, 
		                            (SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		                            (SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		                            (SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		                            (SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,	
		                            (SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified,
		                            (SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID 
			                            AND ViewTime > DATEADD(HOUR, -24*7, GETDATE())) AS NewViews,
		                            (SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID 
			                            AND LikeTime > DATEADD(HOUR, -24*7, GETDATE())) AS NewLikes,
		                            (SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID
			                            AND AnswerDate > DATEADD(HOUR, -24*7, GETDATE())) AS NewAnswers
		                            FROM [Question] AS Q) AS NewQuestion";

                        if (search == "_")
                            _comm.CommandText +=
                                " ORDER BY (0.2 * NewViews + 0.3 * NewAnswers + 0.5 * NewLikes) DESC";
                        else
                            _comm.CommandText +=
                                $" WHERE CONCAT_WS('', QuestionText, Tags, Username, Description) LIKE '%{search}%'" +
                                " ORDER BY (0.2 * NewViews + 0.3 * NewAnswers + 0.5 * NewLikes) DESC";

                        break;

                    case "Unanswered":
                        _comm.CommandText =
                            @"SELECT * FROM 
	                            (SELECT TOP 100 PERCENT 
		                            Q.*, 
		                            (SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		                            (SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		                            (SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		                            (SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		                            (SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		                            FROM [Question] AS Q LEFT JOIN [Comment] AS C ON (Q.QuestionID = C.QuestionID) 
		                            ORDER BY Q.PostDate DESC) AS NewQuestion
	                            WHERE Answers = 0";

                        if (search != "_")
                            _comm.CommandText +=
                                $" AND CONCAT_WS('', QuestionText, Tags, Username, Description) LIKE '%{search}%'";

                        break;

                    case "Unverified":
                        _comm.CommandText =
                            @"SELECT * FROM 
	                            (SELECT TOP 100 PERCENT 
		                            Q.*, 
		                            (SELECT Username FROM [User] WHERE UserID = Q.UserID) AS Username,
		                            (SELECT COUNT(*) FROM [View] WHERE QuestionID = Q.QuestionID) AS Views,
		                            (SELECT ISNULL(SUM(LikeValue), 0) FROM [LikeQuestion] WHERE QuestionID = Q.QuestionID) AS Likes,
		                            (SELECT COUNT(*) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Answers,
		                            (SELECT ISNULL(MAX(CAST(Verified AS INT)), 0) FROM [Comment] WHERE QuestionID = Q.QuestionID) AS Verified
		                            FROM [Question] AS Q 
		                            ORDER BY Q.PostDate DESC) AS NewQuestion
	                            WHERE Verified = 0;";

                        if (search != "_")
                            _comm.CommandText +=
                                $" AND CONCAT_WS('', QuestionText, Tags, Username, Description) LIKE '%{search}%'";

                        break;
                }

                _comm.CommandType = CommandType.Text;
                _conn.Open();

                SqlDataReader reader = _comm.ExecuteReader();

                while (reader.Read())
                {
                    QuestionModel question = new QuestionModel()
                    {
                        QuestionID = Convert.ToInt64(reader[0]),
                        UserID = Convert.ToInt32(reader[1]),
                        QuestionText = reader[2].ToString(),
                        Description = reader[3].ToString(),
                        Tags = reader[4].ToString(),
                        PostDate = Convert.ToDateTime(reader[5]),
                        Username = reader[6].ToString(),
                        Views = Convert.ToInt32(reader[7]),
                        Likes = Convert.ToInt32(reader[8]),
                        Answers = Convert.ToInt32(reader[9]),
                        Verified = Convert.ToBoolean(reader[10])
                    };

                    if (type == "Trending")
                    {
                        question.NewViews = Convert.ToInt32(reader[11]);
                        question.NewLikes = Convert.ToInt32(reader[12]);
                        question.NewAnswers = Convert.ToInt32(reader[13]);
                    }

                    questions.Add(question);
                }

                return Ok(questions);
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

        // DELETE: api/Questions/5
        // Delete a question
        [Authorize]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteQuestion([FromRoute] long id, [FromBody] int UserID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbQuestion = await _context.Question.SingleOrDefaultAsync(c => c.QuestionID == id);

            if (dbQuestion == null)
            {
                return NotFound();
            }

            if (UserID != dbQuestion.UserID)
            {
                return Unauthorized();
            }    

            _context.Question.Remove(dbQuestion);
            await _context.SaveChangesAsync();

            return Ok("Question deleted successfully");
        }
    }

    public class QuestionModel
    {
        public long QuestionID { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string QuestionText { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public DateTime PostDate { get; set; }
        public bool Verified { get; set; }
        public int Answers { get; set; }
        public int NewAnswers { get; set; }
        public int Views { get; set; }
        public int NewViews { get; set; }
        public int Likes { get; set; }
        public int NewLikes { get; set; }
    }
}  