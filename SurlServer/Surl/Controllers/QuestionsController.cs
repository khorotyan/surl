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
        private readonly QuestionContext _questionContext;
        private readonly TagContext _tagContext;
        private IConfiguration _configuration { get; }
        private SqlConnection _conn;
        private SqlCommand _comm;

        public QuestionsController(QuestionContext questionContext, TagContext tagContext, CommentContext commentContext,
            ViewContext viewContext, IConfiguration configuration)
        {
            _questionContext = questionContext;
            _tagContext = tagContext;

            _configuration = configuration;
            _conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _comm = _conn.CreateCommand();
        }

        // POST: api/Questions/Create
        // Create a question
        //[Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateQuestion([FromBody] Question question, [FromBody] Tag[] tags)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            question.PostDate = DateTime.Now;

            _questionContext.Question.Add(question);
            await _questionContext.SaveChangesAsync();

            _tagContext.Tag.AddRange(tags);
            await _tagContext.SaveChangesAsync();

            return Ok("Question created successfully");
        }

        // POST: api/Questions/Update/5
        // Update question or its description
        //[Authorize]
        [HttpPost("Update/{id:int}")]
        public async Task<IActionResult> UpdateQuestion([FromRoute] int id, [FromBody] Question question)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbQuestion = await _questionContext.Question.SingleOrDefaultAsync(c => c.QuestionID == id);
            dbQuestion.QuestionText = question.QuestionText;
            dbQuestion.Description = question.Description;

            _questionContext.Entry(dbQuestion).State = EntityState.Modified;
            await _questionContext.SaveChangesAsync();

            return Ok("Question updated successfully");
        }
    }
}  