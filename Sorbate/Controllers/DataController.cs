using System.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Sorbate.Data;

namespace Sorbate.Controllers;

[ApiController]
[Route("api/data")]
public class DataController(IStorage storage) : ControllerBase {
    [HttpGet]
    public async Task<SorbateApiListing> ListMods(int page = 0, int limit = 10, string? name = null, string? author = null, string? version = null) {
        List<SorbateApiRecord> records = [];
        foreach (ModRecord mod in await storage.ListMods(page, limit, name, author, version)) {
            records.Add(await ToWebRecord(mod));
            // TODO: Optimize this
        }

        return new SorbateApiListing {
            Records = records,
            Total = records.Count,
        };
    }

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

    private async Task<SorbateApiRecord> ToWebRecord(ModRecord record) {
        string? iconUrl = await storage.GetIconDownloadLink(record.Id);

        return new SorbateApiRecord {
            Id = record.Id,
            IconUrl = iconUrl,
            Author = record.Author ?? "Unknown",
            Timestamp = record.Timestamp,
            DisplayName = record.DisplayName,
            InternalName = record.InternalName!,
            ModLoaderVersion = record.ModLoaderVersion!,
            Version = record.Version!
        };
    }
}