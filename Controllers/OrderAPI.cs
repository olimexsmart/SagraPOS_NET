using Microsoft.AspNetCore.Mvc;

using SagraPOS.Models;
using SagraPOS.Helpers;

namespace SagraPOS.Controllers;

[ApiController]
[Route("[action]")]
public class OrderAPI : ControllerBase
{
    private readonly ILogger<OrderAPI> logger;
    private readonly IConfiguration configuration;
    private readonly MenuDB db;
    private readonly SettingsHelper sh;
    private readonly PrinterHelper printerService;
    private readonly bool debugMode;


    public OrderAPI(ILogger<OrderAPI> logger,
                    IConfiguration configuration,
                    MenuDB db,
                    SettingsHelper sh,
                    PrinterHelper printerService)
    {
        this.logger = logger;
        this.db = db;
        this.sh = sh;
        this.printerService = printerService;
        // Init internal state from appsetting configuration                     
        this.configuration = configuration;
        if (!bool.TryParse(configuration["DebugMode"], out debugMode))
            debugMode = false;
    }

    [HttpPost]
    public ActionResult ConfirmOrder(
        [FromQuery] int printerID,
        [FromBody] IEnumerable<OrderEntryDTO> orderToPrint)
    {
        // Compute the total
        // Update DB
        // TODO check if rowid is better than explicit ID column
        // TODO check how to get the ID just inserted
        // TODO use exceptions instead of ActionResult
        // Print only in case of a succesfull transaction
        printerService.PrintOrder(printerID, orderToPrint, out float total);

        using var transaction = db.Database.BeginTransaction();
        OrderLog ol = new()
        {
            Total = total,
            Time = DateTime.Now
        };
        db.OrdersLog.Add(ol);
        db.SaveChanges();
        foreach (var o in orderToPrint)
        {
            db.OrderLogItems.Add(new OrderLogItem
            {
                OrderID = ol.ID,
                MenuEntryID = o.EntryID,
                Quantity = o.Quantity
            });
            db.SaveChanges();
        }
        transaction.Commit();

        return Ok();
    }
}
