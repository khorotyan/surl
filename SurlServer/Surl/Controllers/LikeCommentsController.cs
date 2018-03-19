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
    [Route("api/LikeComments")]
    public class LikeCommentsController : Controller
    {
        private readonly LikeCommentContext _context;

        public LikeCommentsController(LikeCommentContext context)
        {
            _context = context;
        }

        // POST: api/LikeComments/Like
        // Like a comment
        [Authorize]
        [HttpPost("Like")]
        public async Task<IActionResult> Like([FromBody] LikeComment like)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            like.LikeTime = DateTime.Now;

            _context.LikeComment.Add(like);
            await _context.SaveChangesAsync();

            return Ok("You liked the comment");
        }
    }
}