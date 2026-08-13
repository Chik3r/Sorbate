using System.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Sorbate.Data;

namespace Sorbate.Controllers;

[ApiController]
[Route("api/data")]
public class DataController(IStorage storage) : ControllerBase {
    [HttpGet]
    public async Task<IEnumerable<ModRecord>> ListMods() =>
        await storage.ListMods(0, 10);

    [HttpGet("tmod/{id:int}")]
    public async Task<IActionResult> GetModDownloadLink(int id) {
        string? link = await storage.GetModDownloadLink(id);

        if (link == null)
            return NotFound();

        return Ok(link);
    }
    
    [HttpGet("icon/{id:int}")]
    public async Task<IActionResult> GetIconDownloadLink(int id) {
        string? link = await storage.GetIconDownloadLink(id);

        if (link == null)
            return NotFound();

        return Ok(link);
    }
}