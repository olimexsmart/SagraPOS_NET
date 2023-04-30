using Microsoft.AspNetCore.Mvc;
using SagraPOS.Models;


namespace SagraPOS.Controllers;

[ApiController]
[Route("[action]")]
public class InfoAPI : ControllerBase
{
    private readonly MenuDB db;

    public InfoAPI(MenuDB db)
    {
        this.db = db;
    }

    [HttpGet]
    public ActionResult<InfoOrdersDTO> GetInfoOrders()
    {
        InfoOrdersDTO iod = new();

        // Complete money total
        iod.OrdersTotal = db.OrdersLog.Select(x => x.Total).Sum();
        // Total orders
        iod.NumOrders = db.OrdersLog.Count();
        // Get menu entries and loop on them
        var menuEntries = db.MenuEntries;
        foreach(var me in menuEntries)
        {
            InfoOrdersDTO.InfoOrderEntry ioe = new();
            ioe.MenuEntryName = me.name; // TODO uppercase
            // ioe.QuantitySold = TODO 
            iod.AddEntry(ioe);
        }

        return Ok(iod);
    }

    [HttpDelete]
    public ActionResult ResetInfoOrders()
    {
        return Ok();
    }
}