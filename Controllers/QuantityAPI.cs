using Microsoft.AspNetCore.Mvc;
using SagraPOS.Models;

namespace SagraPOS.Controllers;

[ApiController]
[Route("[action]")]
public class QuantityAPI : ControllerBase
{
    private readonly ILogger<QuantityAPI> logger;
    private readonly IConfiguration configuration;
    private readonly MenuDB db;

    public QuantityAPI(ILogger<QuantityAPI> logger,
                      IConfiguration configuration,
                      MenuDB db)
    {
        this.logger = logger;
        this.db = db;
        this.configuration = configuration;
    }

    [HttpGet]
    public Dictionary<int, int> GetQuantities() => db.MenuEntries.ToDictionary(k => k.ID, v => v.Quantity);

    [HttpPut]
    public ActionResult SetQuantity([FromQuery] int entryID, [FromQuery] int quantity)
    {
        logger.LogError($"entri id: {entryID}");
        MenuEntry? m = db.MenuEntries.SingleOrDefault(m => m.ID == entryID);
        if (m is null) return NotFound($"Menu entry with ID {entryID} not found");
        m.Quantity = quantity;
        db.SaveChanges();
        return Ok();
    }
}