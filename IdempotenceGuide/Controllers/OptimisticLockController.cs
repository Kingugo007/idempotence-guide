using IdempotenceGuide.Dto;
using IdempotenceGuide.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdempotenceGuide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OptimisticLockController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public OptimisticLockController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            // Return ETag for version tracking
            var etag = Convert.ToBase64String(document.RowVersion);
            Response.Headers.Add("ETag", etag);

            return Ok(document);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(
            int id,
            [FromBody] DocumentUpdateRequest request,
            [FromHeader(Name = "If-Match")] string ifMatch)
        {
            if (string.IsNullOrEmpty(ifMatch))
            {
                return BadRequest("If-Match header is required for idempotent updates");
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            var currentEtag = Convert.ToBase64String(document.RowVersion);
            if (ifMatch != currentEtag)
            {
                return StatusCode(412, "Precondition Failed: Document has been modified");
            }

            try
            {
                document.Title = request.Title;
                document.Content = request.Content;
                document.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var newEtag = Convert.ToBase64String(document.RowVersion);
                Response.Headers.Add("ETag", newEtag);

                return Ok(document);
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(409, "Conflict: Document was modified by another request");
            }
        }
    }

}
