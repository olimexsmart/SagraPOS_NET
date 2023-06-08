using Microsoft.AspNetCore.Mvc;
using SagraPOS.Helpers;
using SagraPOS.Models;

namespace SagraPOS.Controllers;

[ApiController]
[Route("[action]")]
public class InfoAPI : ControllerBase
{
    private readonly MenuDB db;
    private readonly PrinterHelper printerService;

    private readonly int adminPin;

    public InfoAPI(MenuDB db,
                   IConfiguration configuration,
                   PrinterHelper printerService)
    {
        this.db = db;
        this.printerService = printerService;
        if (!int.TryParse(configuration["AdminPin"], out adminPin))
            throw new NullReferenceException("Missing AdminPin setting");
    }

    [HttpGet]
    public ActionResult<InfoOrdersDTO> GetInfoOrders()
    {
        return Ok(GatherInfo());
    }

    [HttpDelete]
    public ActionResult ResetInfoOrders([FromQuery] int pin)
    {
        if (adminPin != pin)
            return Unauthorized("Wrong administrator pin");

        // Drop order table and will cascade to items
        db.OrdersLog.RemoveRange(db.OrdersLog);
        db.SaveChanges();
        return Ok();
    }

    [HttpGet]
    public ActionResult PrintInfo([FromQuery] int printerID)
    {
        printerService.PrintInfo(printerID, GatherInfo());
        return Ok();
    }

    private InfoOrdersDTO GatherInfo()
    {
        InfoOrdersDTO iod = new();
        // Complete money total
        iod.OrdersTotal = db.OrdersLog.Sum(x => x.Total);
        // Total orders
        iod.NumOrders = db.OrdersLog.Count();
        // Total number of items ordered
        int totalItems = db.OrderLogItems.Sum(x => x.Quantity);
        if (totalItems != 0)
        {
            // Get menu entries and loop on them
            var menuEntries = db.MenuEntries;
            foreach (var me in menuEntries)
            {
                InfoOrdersDTO.InfoOrderEntry ioe = new();
                ioe.MenuEntryName = me.Name;
                ioe.QuantitySold = db.OrderLogItems
                                     .Where(x => x.MenuEntryID == me.ID)
                                     .Sum(x => x.Quantity);
                ioe.TotalSold = ioe.QuantitySold * me.Price;
                ioe.TotalPercentage = ((float)ioe.QuantitySold) / totalItems;
                ioe.TotalSoldPercentage = ((float)ioe.TotalSold) / iod.OrdersTotal;

                iod.AddEntry(ioe);
            }
        }
        return iod;
    }
}