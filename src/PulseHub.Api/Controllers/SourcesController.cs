using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseHub.Dal.Data;
using PulseHub.Domain.Models;

namespace PulseHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SourcesController(PulseHubDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Source>>> GetAll()
    {
        return await dbContext.Sources
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Source>> GetById(int id)
    {
        var source = await dbContext.Sources.FindAsync(id);

        if (source is null)
            return NotFound();

        return source;
    }

    [HttpPost]
    public async Task<ActionResult<Source>> Create(Source source)
    {
        source.Id = 0;
        source.CreatedAtUtc = DateTime.UtcNow;

        dbContext.Sources.Add(source);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = source.Id }, source);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Source source)
    {
        var existingSource = await dbContext.Sources.FindAsync(id);

        if (existingSource is null)
            return NotFound();

        existingSource.Name = source.Name;
        existingSource.SourceType = source.SourceType;
        existingSource.Url = source.Url;
        existingSource.IsEnabled = source.IsEnabled;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var source = await dbContext.Sources.FindAsync(id);

        if (source is null)
            return NotFound();

        dbContext.Sources.Remove(source);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}