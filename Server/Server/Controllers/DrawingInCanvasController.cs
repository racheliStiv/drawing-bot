using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrawingInCanvasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public DrawingInCanvasController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("canvas/{canvasId}")]
        public async Task<ActionResult<IEnumerable<DrawingInCanvasDto>>> GetDrawingsForCanvas(int canvasId)
        {
            var drawings = await _context.DrawingsInCanvas
                .Where(d => d.CanvasId == canvasId)
                .ToListAsync();

            return Ok(_mapper.Map<List<DrawingInCanvasDto>>(drawings));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DrawingInCanvasDto>>> GetAllDrawings()
        {
            var drawings = await _context.DrawingsInCanvas
                .Include(d => d.Canvas)
                .ToListAsync();

            return Ok(_mapper.Map<List<DrawingInCanvasDto>>(drawings));
        }

        [HttpPost]
        public async Task<ActionResult<DrawingInCanvasDto>> CreateDrawing(CreateDrawingInCanvasDto dto)
        {
            var drawing = _mapper.Map<DrawingInCanvas>(dto);
            _context.DrawingsInCanvas.Add(drawing);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDrawingsForCanvas), new { canvasId = drawing.CanvasId }, _mapper.Map<DrawingInCanvasDto>(drawing));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDrawing(int id, UpdateDrawingInCanvasDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch.");

            var drawing = await _context.DrawingsInCanvas.FindAsync(id);
            if (drawing == null)
                return NotFound();

            _mapper.Map(dto, drawing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDrawing(int id)
        {
            var drawing = await _context.DrawingsInCanvas.FindAsync(id);
            if (drawing == null)
                return NotFound();

            _context.DrawingsInCanvas.Remove(drawing);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
