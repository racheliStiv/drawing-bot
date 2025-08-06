using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;
using Server.Data;
using Server.DTOs;
using Server.Models;
using System.Text.Json;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CanvasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CanvasController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CanvasSummaryDto>>> GetCanvases()
        {
            var canvases = await _context.Canvases.ToListAsync();
            return Ok(_mapper.Map<List<CanvasSummaryDto>>(canvases));
        }

        [HttpGet("{id}")]
        //public async Task<ActionResult<CanvasDto>> GetCanvas(int id)
        //{
        //    var canvas = await _context.Canvases.Include(c => c.Drawings)
        //                                        .FirstOrDefaultAsync(c => c.Id == id);
        //    if (canvas == null)
        //        return NotFound();

        //    return Ok(_mapper.Map<CanvasDto>(canvas));
        //}


        public async Task<IActionResult> GetCanvas(int id)
        {
            var canvas = await _context.Canvases
                .Include(c => c.Drawings)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (canvas == null)
                return NotFound();

            var drawings = new List<JsonElement>();
            foreach (var drawing in canvas.Drawings)
            {
                try
                {
                    // מצפה ש־DrawingAttributes הוא JSON כמו {"type":"line",...}
                    var element = JsonSerializer.Deserialize<JsonElement>(drawing.DrawingAttributes);
                    drawings.Add(element);
                }
                catch (JsonException)
                {
                    // אם לא תקין, שומרים fallback עם ה־raw string
                    var fallback = JsonSerializer.SerializeToElement(new
                    {
                        raw = drawing.DrawingAttributes
                    });
                    drawings.Add(fallback);
                }
            }

            return Ok(new
            {
                canvasId = canvas.Id,
                drawings // מערך של אובייקטים עם כל המאפיינים של כל ציור
            });
        }


        [HttpPost]
        public async Task<IActionResult> CreateCanvasFromRaw([FromBody] CanvasWithDrawingsRequest request)
        {
            // 1. צור קנבס
            var canvas = new Canvas
            {
                Name = request.CanvasName
            };
            _context.Canvases.Add(canvas);
            await _context.SaveChangesAsync(); // כדי לקבל את canvas.Id

            // 2. נסה לפרס את ה־JSON שהגיע (ממחרוזת) למערך
            try
            {
                using var doc = JsonDocument.Parse(request.Drawings);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    return BadRequest("drawingsJson חייב להיות מערך JSON.");

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    // שמור כל צורה כ־JSON גולמי ב־DrawingAttributes
                    var raw = element.GetRawText();

                    var drawing = new DrawingInCanvas
                    {
                        CanvasId = canvas.Id,
                        DrawingAttributes = raw
                    };
                    _context.DrawingsInCanvas.Add(drawing);
                }

                await _context.SaveChangesAsync();
            }
            catch (JsonException)
            {
                return BadRequest("drawingsJson אינו JSON תקין.");
            }

            return CreatedAtAction(nameof(GetCanvas), new { id = canvas.Id }, new { canvasId = canvas.Id });
        }
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateCanvas(int id, UpdateCanvasDto dto)
        //{
        //    if (id != dto.Id)
        //        return BadRequest("ID mismatch.");

        //    var canvas = await _context.Canvases.FindAsync(id);
        //    if (canvas == null)
        //        return NotFound();

        //    _mapper.Map(dto, canvas);
        //    await _context.SaveChangesAsync();
        //    return NoContent();
        //}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCanvas(int id, [FromBody] CanvasWithDrawingsRequest request)
        {
            // 1. בדוק אם הקנבס קיים
            var canvas = await _context.Canvases
                .Include(c => c.Drawings)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (canvas == null)
                return NotFound();

            // 2. מחק את כל הציורים הקודמים
            _context.DrawingsInCanvas.RemoveRange(canvas.Drawings);

            // 3. נסה לנתח את הציורים החדשים (כ-JSON)
            try
            {
                using var doc = JsonDocument.Parse(request.Drawings);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    return BadRequest("drawingsJson חייב להיות מערך JSON.");

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    var raw = element.GetRawText();

                    var drawing = new DrawingInCanvas
                    {
                        CanvasId = canvas.Id,
                        DrawingAttributes = raw
                    };

                    _context.DrawingsInCanvas.Add(drawing);
                }

                await _context.SaveChangesAsync();
            }
            catch (JsonException)
            {
                return BadRequest("drawingsJson אינו JSON תקין.");
            }

            // 4. החזר תשובה תקינה
            return NoContent(); // קוד 204: בוצע בהצלחה, אין תוכן להחזיר
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCanvas(int id)
        {
            var canvas = await _context.Canvases.FindAsync(id);
            if (canvas == null)
                return NotFound();

            _context.Canvases.Remove(canvas);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
