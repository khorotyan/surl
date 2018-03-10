using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        // GET: api/Views
        [HttpGet]
        public IEnumerable<View> GetView()
        {
            return _context.View;
        }

        // GET: api/Views/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetView([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var view = await _context.View.SingleOrDefaultAsync(m => m.QuestionID == id);

            if (view == null)
            {
                return NotFound();
            }

            return Ok(view);
        }

        // PUT: api/Views/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutView([FromRoute] long id, [FromBody] View view)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != view.QuestionID)
            {
                return BadRequest();
            }

            _context.Entry(view).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ViewExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Views
        [HttpPost]
        public async Task<IActionResult> PostView([FromBody] View view)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.View.Add(view);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ViewExists(view.QuestionID))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetView", new { id = view.QuestionID }, view);
        }

        // DELETE: api/Views/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteView([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var view = await _context.View.SingleOrDefaultAsync(m => m.QuestionID == id);
            if (view == null)
            {
                return NotFound();
            }

            _context.View.Remove(view);
            await _context.SaveChangesAsync();

            return Ok(view);
        }

        private bool ViewExists(long id)
        {
            return _context.View.Any(e => e.QuestionID == id);
        }
    }
}