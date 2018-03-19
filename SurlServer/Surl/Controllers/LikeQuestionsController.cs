using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Surl.Data;

namespace Surl.Controllers
{
    [Produces("application/json")]
    [Route("api/LikeQuestions")]
    public class LikeQuestionsController : Controller
    {
        private readonly LikeQuestionContext _context;

        public LikeQuestionsController(LikeQuestionContext context)
        {
            _context = context;
        }

        // POST: api/LikeQuestions/Like
        // Like a question
        [Authorize]
        [HttpPost("Like")]
        public async Task<IActionResult> Like([FromBody] LikeQuestion like)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            like.LikeTime = DateTime.Now;

            _context.LikeQuestion.Add(like);
            await _context.SaveChangesAsync();

            return Ok("You liked the question");
        }
    }
}