using System.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Sorbate.Data;

namespace Sorbate.Controllers;

[ApiController]
[Route("api/data")]
public class DataController : ControllerBase {
    private readonly AppDbContext _db;

    [HttpGet]
    public async Task<IEnumerable<ModRecord>> Get() =>
        await _db.ModRecords
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
}