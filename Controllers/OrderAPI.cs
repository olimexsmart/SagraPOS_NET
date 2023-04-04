using Microsoft.AspNetCore.Mvc;
using ESCPOS_NET.Emitters;
using ESCPOS_NET;
using ESCPOS_NET.Utilities;
using ESCPOS_NET.Extensions;
using SagraPOS.Models;
using System.Text;

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
    private const string dashLine = "---------------------";
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
    }

    [HttpPost]
    public ActionResult ConfirmOrder([FromBody] IEnumerable<OrderEntryDTO> orderToPrint)
    {
        return printOrder(orderToPrint).Result;
    }

    private async Task<ActionResult> printOrder(IEnumerable<OrderEntryDTO> orderToPrint)
    {
        // Exit immediately if empty
        if (!orderToPrint.Any())
            return BadRequest("Empty order array");
        // Fill these variable getting result from DB
        float total = 0;
        Dictionary<int, Dictionary<string, int>> printCategories = new();
        // Dictionary<int, int> printQuantities = new();
        foreach (var o in orderToPrint)
        {
            // Get MenuEntry object from db
            MenuEntry? me = db.menuEntries.Where(x => x.ID == o.EntryID)
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
        }

        if (debugMode)
            return Ok(printCategories);

        // TODO logo
        // Use the vars defined before to printe the receipt
        var e = new EPSON();
        ByteArrayBuilder bab = new();
        bab.Append(e.LeftAlign());
        // Loop on categories
        foreach (var category in printCategories)
        {
            bab.Append(e.SetStyles(PrintStyle.DoubleHeight | PrintStyle.DoubleWidth | PrintStyle.Bold));
            bab.Append(e.PrintLine(dashLine));
            foreach (var entry in category.Value)
            {
                bab.Append(e.PrintLine($"{entry.Key,-18}{entry.Value,2}X"));
                bab.Append(e.PrintLine(dashLine));
                bab.Append(e.PrintLine(dashLine));
                bab.Append(e.PrintLine(dashLine));
                bab.Append(e.PrintLine(dashLine));
                // bab.Append(e.PrintLine(""));
                // bab.Append(e.PrintLine(""));
                // bab.Append(e.PrintLine(""));
                // bab.Append(e.PrintLine(""));
            }
            bab.Append(e.PrintLine(dashLine));
            bab.Append(e.SetStyles(PrintStyle.None));
            // bab.Append(e.FullCut());
            bab.Append(0x1b);
            bab.Append(0x6d);

        }
        /*
        var deb = bab.ToArray();
        StringBuilder hex = new StringBuilder(deb.Length * 2);
        foreach (byte b in deb)
            hex.AppendFormat("{0:x2}\n", b);
        Console.WriteLine(hex.ToString());
        */
        try
        {
            await printer.WriteAsync(bab.ToArray());
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
}
