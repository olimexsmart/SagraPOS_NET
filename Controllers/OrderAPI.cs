using Microsoft.AspNetCore.Mvc;
using ESCPOS_NET.Emitters;
using ESCPOS_NET;
using ESCPOS_NET.Utilities;
using SagraPOS.Models;

namespace SagraPOS.Controllers;

[ApiController]
[Route("[action]")]
public class OrderAPI : ControllerBase
{
    private readonly ILogger<OrderAPI> logger;
    private readonly IConfiguration configuration;
    private readonly MenuDB db;
    private readonly bool debugMode;
    private readonly string printerIP;
    private readonly int printerPort;
    private readonly string dashLine;
    private readonly ImmediateNetworkPrinter printer;

    public OrderAPI(ILogger<OrderAPI> logger, IConfiguration configuration, MenuDB db)
    {
        this.logger = logger;
        this.db = db;
        // Init internal state from appsetting configuration                     
        this.configuration = configuration;
        if (!bool.TryParse(configuration["DebugMode"], out debugMode))
            debugMode = false;
        printerIP = configuration["Printer:IP"] ?? throw new NullReferenceException("Missing printer IP setting");
        if (!int.TryParse(configuration["Printer:port"]
                            ?? throw new NullReferenceException("Missing printer port setting"), out printerPort))
        { throw new FormatException("Printer port setting non parable as int"); }
        // Init printer
        printer = new ImmediateNetworkPrinter(new ImmediateNetworkPrinterSettings()
        {
            ConnectionString = $"{printerIP}:{printerPort}",
            PrinterName = "P3"
        });
        dashLine = configuration["Printer:dashLine"] ?? throw new NullReferenceException("Missing printer dashLine setting");
    }

    [HttpPost]
    public ActionResult ConfirmOrder([FromBody] IEnumerable<OrderEntryDTO> orderToPrint)
    {
        // Compute the total
        // Update DB
        // TODO check if rowid is better than explicit ID column
        // TODO check how to get the ID just inserted
        // Print only in case of a succesfull transaction
        ActionResult ar = printOrder(orderToPrint, out float total);
        if (ar is OkObjectResult)
        {
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
        }

        return ar;
    }

    private ActionResult printOrder(IEnumerable<OrderEntryDTO> orderToPrint, out float total)
    {
        total = 0;
        // Exit immediately if empty
        if (!orderToPrint.Any())
            return BadRequest("Empty order array");
        // Fill these variable getting result from DB
        Dictionary<int, Dictionary<string, int>> printCategories = new();
        bool hasTens = false; // True if some quantity ha 2 digits // TODO set a maximum quantity value
        foreach (var o in orderToPrint)
        {
            // Get MenuEntry object from db
            MenuEntry? me = db.MenuEntries.Where(x => x.ID == o.EntryID)
                                         .Select(x => new MenuEntry()
                                         {
                                             categoryID = x.categoryID,
                                             name = x.name,
                                             price = x.price
                                         }).FirstOrDefault();
            if (me is null)
                return NotFound($"MenuEntry with ID {o.EntryID} not found");
            // Add entry to the dictionary
            if (!printCategories.ContainsKey(me.categoryID))
                printCategories.Add(me.categoryID, new Dictionary<string, int>());
            // Check if already present
            if (printCategories[me.categoryID].ContainsKey(me.name))
                return BadRequest($"Order contains duplicate IDs with value: {o.EntryID}");
            printCategories[me.categoryID].Add(me.name, o.Quantity);
            // Update total
            total += me.price * o.Quantity;
            // Update tens presence
            hasTens |= o.Quantity >= 10;
        }
        // Get categories names by ID
        Dictionary<int, string> categoryNames = db.Categories.ToDictionary(k => k.ID, v => v.name);
        // Stop here if in debug mode
        if (debugMode)
        {
            foreach(var pc in printCategories)
                foreach(var pcVal in pc.Value)
                    logger.LogInformation($"Order printing: MenuEntry={pc.Key} Quantity={pcVal.Value}");
            return Ok(printCategories);
        }

        // TODO logo
        // Use the vars defined before to printe the receipt
        var e = new EPSON();
        ByteArrayBuilder bab = new();
        // Loop on categories
        foreach (var category in printCategories)
        {
            // Title with category name
            bab.Append(PrinterSetDimensions(0, 0));
            bab.Append(e.SetStyles(PrintStyle.Italic | PrintStyle.Bold));
            bab.Append(e.CenterAlign());
            bab.Append(e.PrintLine("------ " + categoryNames[category.Key].ToUpper() + " ------"));
            bab.Append(e.PrintLine(""));
            // Body with the ordered entries
            bab.Append(e.LeftAlign());
            bab.Append(e.SetStyles(PrintStyle.Bold));
            bab.Append(PrinterSetDimensions(3, 1));
            foreach (var entry in category.Value)
            {
                if (hasTens)
                    bab.Append(e.PrintLine($"{entry.Key.ToUpper(),-18}x{entry.Value,2}"));
                else
                    bab.Append(e.PrintLine($"{entry.Key.ToUpper(),-19}x{entry.Value,1}"));
            }
            bab.Append(e.FullCut());
        }
        // Order total
        bab.Append(e.Print($"TOTALE:         "));
        bab.Append(ByteSplicer.Combine(
        e.CodePage(CodePage.PC858_EURO),
        new byte[] { 0xD5 },
        e.CodePage(CodePage.PC437_USA_STANDARD_EUROPE_DEFAULT)));
        bab.Append(e.PrintLine($"{total,2:.00}"));
        // Additional info
        bab.Append(e.CenterAlign());
        bab.Append(e.SetStyles(PrintStyle.Italic | PrintStyle.Bold));
        bab.Append(e.PrintLine(""));
        bab.Append(e.PrintLine("IL RICAVATO VIENE DEVOLUTO IN BENEFICENZA"));
        bab.Append(e.PrintImage(System.IO.File.ReadAllBytes("logo.jpg"), true, isLegacy: true, maxWidth: 350));
        bab.Append(e.SetStyles(PrintStyle.None));
        bab.Append(e.PrintLine(""));
        bab.Append(e.PrintLine("github.com/olimexsmart/sagraPOS"));
        bab.Append(e.FullCut());

        try
        {
            printer.WriteAsync(bab.ToArray()).Wait();
        }
        catch (TimeoutException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, "Cannot connect to printer");
        }

        /*await printer.WriteAsync(
            ByteSplicer.Combine(
                e.CenterAlign(),
                // e.PrintImage(File.ReadAllBytes("images/pd-logo-300.png"), true),
                e.PrintLine(""),
                e.SetBarcodeHeightInDots(360),
                e.SetBarWidth(BarWidth.Default),
                e.SetBarLabelPosition(BarLabelPrintPosition.None),
                e.PrintBarcode(BarcodeType.ITF, "0123456789"),
                e.PrintLine(""),
                e.PrintLine("B&H PHOTO & VIDEO"),
                e.PrintLine("420 NINTH AVE."),
                e.PrintLine("NEW YORK, NY 10001"),
                e.PrintLine("(212) 502-6380 - (800)947-9975"),
                e.SetStyles(PrintStyle.Underline),
                e.PrintLine("www.bhphotovideo.com"),
                e.SetStyles(PrintStyle.None),
                e.PrintLine(""),
                e.LeftAlign(),
                e.PrintLine("Order: 123456789        Date: 02/01/19"),
                e.PrintLine(""),
                e.PrintLine(""),
                e.SetStyles(PrintStyle.FontB),
                e.PrintLine("1   TRITON LOW-NOISE IN-LINE MICROPHONE PREAMP"),
                e.PrintLine("    TRFETHEAD/FETHEAD                        89.95         89.95"),
                e.PrintLine("----------------------------------------------------------------"),
                e.RightAlign(),
                e.PrintLine("SUBTOTAL         89.95"),
                e.PrintLine("Total Order:         89.95"),
                e.PrintLine("Total Payment:         89.95"),
                e.PrintLine(""),
                e.LeftAlign(),
                e.SetStyles(PrintStyle.Bold | PrintStyle.FontB),
                e.PrintLine("SOLD TO:                        SHIP TO:"),
                e.SetStyles(PrintStyle.FontB),
                e.PrintLine("  FIRSTN LASTNAME                 FIRSTN LASTNAME"),
                e.PrintLine("  123 FAKE ST.                    123 FAKE ST."),
                e.PrintLine("  DECATUR, IL 12345               DECATUR, IL 12345"),
                e.PrintLine("  (123)456-7890                   (123)456-7890"),
                e.PrintLine("  CUST: 87654321"),
                e.PrintLine(""),
                e.PrintLine(""),
                e.FullCutAfterFeed(1)
            )
        );*/

        return Ok();
    }

    private byte[] PrinterSetDimensions(int heigth, int width)
    {
        if (heigth > 7) heigth = 7;
        if (width > 7) width = 7;
        return new byte[] { 0x1D, 0x21, (byte)((width << 4) + heigth) };
    }
}
