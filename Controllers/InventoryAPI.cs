using Microsoft.AspNetCore.Mvc;
using SagraPOS.Models;

namespace SagraPOS.Controllers;

[ApiController]
[Route("[action]")]
public class InventoryAPI : ControllerBase
{
    private readonly ILogger<InventoryAPI> logger;
    private readonly IConfiguration configuration;
    private readonly MenuDB db;

    public InventoryAPI(ILogger<InventoryAPI> logger,
                       IConfiguration configuration,
                       MenuDB db)
    {
        this.logger = logger;
        this.db = db;
        this.configuration = configuration;
    }

    [HttpGet]
    public Dictionary<int, int> GetQuantities() => db.MenuEntries.ToDictionary(k => k.ID, v => v.Inventory);

    [HttpPut]
    public ActionResult SetQuantity([FromQuery] int entryID, [FromQuery] int quantity)
    {
        MenuEntry? m = db.MenuEntries.SingleOrDefault(m => m.ID == entryID);
        if (m is null) return NotFound($"Menu entry with ID {entryID} not found");
        m.Inventory = quantity;
        db.SaveChanges();
        return Ok();
    }
}