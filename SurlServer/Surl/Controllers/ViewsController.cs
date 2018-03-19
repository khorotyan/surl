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
    [Route("api/Views")]
    public class ViewsController : Controller
    {
        private readonly ViewContext _context;

        public ViewsController(ViewContext context)
        {
            _context = context;
        }

        // POST: api/Views/View
        // View a question
        [Authorize]
        [HttpPost("View")]
        public async Task<IActionResult> ViewQuestion([FromBody] View view)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            view.ViewTime = DateTime.Now;

            _context.View.Add(view);
            await _context.SaveChangesAsync();

            return Ok("You viewed the question");
        }
    }
}