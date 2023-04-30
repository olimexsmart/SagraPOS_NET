using Microsoft.AspNetCore.Mvc;
using SagraPOS.Models;

namespace SagraPOS.Controllers;

[ApiController]
[Route("[action]")]
public class MenuAPI : ControllerBase
{
    private readonly ILogger<MenuAPI> logger;
    private readonly MenuDB db;

    public MenuAPI(ILogger<MenuAPI> logger, MenuDB db)
    {
        this.logger = logger;
        this.db = db;
    }

    [HttpGet]
    public IEnumerable<MenuCategory> GetCategories()
    {
        return db.Categories;
    }

    [HttpGet]
    public IEnumerable<dynamic> GetEntries()
    {
        return db.MenuEntries.Select(x => new
        {
            id = x.ID,
            categoryID = x.categoryID,
            name = x.name,
            price = x.price
        });
    }

    [HttpGet]
    public ActionResult GetImage([FromQuery] int id)
    {
        var image = db.MenuEntries.Where(x => x.ID == id)
                                  .Select(x => x.image)
                                  .ToArray();
        if (image.Any())
            return File(image[0] ?? new byte[0], "image/jpeg");
        else
            return NotFound();
    }
}
