using ESCPOS_NET.Emitters;
using ESCPOS_NET;
using ESCPOS_NET.Utilities;
using SagraPOS.Models;

namespace SagraPOS.Helpers;

public class PrinterHelper
{
    private readonly ILogger<PrinterHelper> logger;
    private readonly IConfiguration configuration;
    private readonly MenuDB db; // TODO remove this dependency
    private readonly SettingsHelper sh;
    private readonly IEnumerable<Printer> printersDB;
    private readonly Dictionary<int, ImmediateNetworkPrinter> printersESC;
    private readonly bool debugMode;
    private readonly int maxItems;

    public PrinterHelper(ILogger<PrinterHelper> logger,
                         IConfiguration configuration,
                         MenuDB db,
                         SettingsHelper sh)
    {
        this.logger = logger;
        this.db = db;
        this.sh = sh;
        this.configuration = configuration;
        // Init internal state from appsetting configuration                     
        if (!bool.TryParse(configuration["DebugMode"], out debugMode))
            debugMode = false;
        // Init max items limit
        int.TryParse(configuration["MaxItems"], out maxItems);
        // Pre-load from DB the printers available
        printersDB = db.Printers;
        // Init physical printers
        printersESC = new();
        foreach (var p in printersDB)
            printersESC.Add(p.ID, new ImmediateNetworkPrinter(new ImmediateNetworkPrinterSettings()
            {
                ConnectionString = $"{p.IP}:{p.Port}",
                PrinterName = p.Name
            }));
    }

    internal void PrintOrder(int printerID, IEnumerable<OrderEntryDTO> orderToPrint, out float total)
    {
        total = 0;
        // Exit immediately if empty
        if (!orderToPrint.Any())
            throw new InvalidDataException("Empty order");
        // Fill these variable getting result from DB
        Dictionary<int, Dictionary<string, int>> printCategories = new();
        bool hasTens = false; // True if some quantity ha 2 digits
        foreach (var o in orderToPrint)
        {
            // Enforce maximum quantity
            o.Quantity = Math.Min(o.Quantity, maxItems);
            // Get MenuEntry object from DB
            MenuEntry? me = db.MenuEntries.Where(x => x.ID == o.EntryID)
                                          .Select(x => new MenuEntry()
                                          {
                                              CategoryID = x.CategoryID,
                                              Name = x.Name,
                                              Price = x.Price
                                          })
                                          .FirstOrDefault();
            if (me is null)
                throw new KeyNotFoundException($"MenuEntry with ID {o.EntryID} not found");
            // Add entry to the dictionary
            if (!printCategories.ContainsKey(me.CategoryID))
                printCategories.Add(me.CategoryID, new Dictionary<string, int>());
            // Check if already present
            if (printCategories[me.CategoryID].ContainsKey(me.Name))
                throw new InvalidDataException($"Order contains duplicate IDs with value: {o.EntryID}");
            printCategories[me.CategoryID].Add(me.Name, o.Quantity);
            // Update total
            total += me.Price * o.Quantity;
            // Update tens presence
            hasTens |= o.Quantity >= 10;
        }
        // Get categories names by ID
        Dictionary<int, string> categoryNames = db.Categories.ToDictionary(k => k.ID, v => v.name);
        // Stop here if in debug mode
        if (debugMode)
        {
            logger.LogInformation($"Selected printer: {printerID}");
            logger.LogInformation($"Order total: {total}");
            foreach (var pc in printCategories)
                foreach (var pcVal in pc.Value)
                    logger.LogInformation($"Order printing: MenuEntry={pc.Key} Quantity={pcVal.Value}");
            return;
        }
        // Exit if printer ID is not recognized
        if (!printersESC.ContainsKey(printerID))
            throw new KeyNotFoundException($"Printer with ID={printerID} is not configured");
        // Load logo
        byte[]? logo = db.Settings.Where(x => x.Key == "PrintLogo").Select(x => x.ValueBlob).First();
        string? overLogoText = db.Settings.Where(x => x.Key == "OverLogoText").Select(x => x.ValueString).First();
        string? underLogoText = db.Settings.Where(x => x.Key == "UnderLogoText").Select(x => x.ValueString).First();
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
            bab.Append(PrinterSetDimensions(2, 1));
            foreach (var entry in category.Value)
            {
                if (hasTens)
                    bab.Append(e.PrintLine($"{entry.Key.ToUpper(),-20}x{entry.Value,2}"));
                else
                    bab.Append(e.PrintLine($"{entry.Key.ToUpper(),-21}x{entry.Value,1}"));
            }
            bab.Append(e.FullCutAfterFeed(3));
        }
        // Order total
        bab.Append(PrinterSetDimensions(3, 2));
        bab.Append(e.Print($"TOTALE:".PadRight(9)));
        bab.Append(PrintEuro(e));
        bab.Append(e.PrintLine($"{total:.00}"));
        // Additional info
        bab.Append(e.CenterAlign());
        bab.Append(e.SetStyles(PrintStyle.Italic | PrintStyle.Bold));
        bab.Append(e.PrintLine(""));
        if (overLogoText is not null)
            bab.Append(e.PrintLine(overLogoText));
        if (logo is not null)
            bab.Append(e.PrintImage(logo, true, isLegacy: true, maxWidth: 350));
        bab.Append(e.SetStyles(PrintStyle.None));
        bab.Append(e.PrintLine(""));
        if (underLogoText is not null)
            bab.Append(e.PrintLine(underLogoText));
        bab.Append(e.FullCutAfterFeed(3));
        // Confirm printing
        printersESC[printerID].WriteAsync(bab.ToArray()).Wait();
    }

    public void PrintInfo(int printerID, InfoOrdersDTO info)
    {
        if (debugMode)
            logger.LogInformation($"Printing info on {printerID}");
        var e = new EPSON();
        ByteArrayBuilder bab = new();
        // Preamble with general info
        bab.Append(e.SetStyles(PrintStyle.Bold));
        bab.Append(e.CenterAlign());
        bab.Append(e.PrintLine($"Totale ordini: {info.NumOrders}"));
        bab.Append(e.Print($"Totale ricavi: "));
        bab.Append(PrintEuro(e));
        bab.Append(e.PrintLine($"{info.OrdersTotal:0.00}"));
        // Loop on each item
        foreach (var item in info.InfoOrderEntries)
        {
            bab.Append(e.SetStyles(PrintStyle.Bold));
            bab.Append(e.CenterAlign());
            bab.Append(e.PrintLine(""));
            bab.Append(e.PrintLine(item.MenuEntryName.ToUpper()));
            bab.Append(e.LeftAlign());
            bab.Append(e.SetStyles(PrintStyle.None));
            bab.Append(e.PrintLine($"Vendute: {item.QuantitySold}"));
            bab.Append(e.PrintLine($"Percentuale vendite: {item.TotalSoldPercentage * 100:0.0}%"));
            bab.Append(e.Print("Ricavi: "));
            bab.Append(PrintEuro(e));
            bab.Append(e.PrintLine($"{item.TotalSold:0.00}"));
            bab.Append(e.PrintLine($"Percentuale ricavi: {item.TotalPercentage * 100:0.0}%"));
        }
        // Confirm printing
        bab.Append(e.FullCutAfterFeed(3));
        printersESC[printerID].WriteAsync(bab.ToArray()).Wait();
    }

    private byte[] PrinterSetDimensions(int heigth, int width)
    {
        if (heigth > 7) heigth = 7;
        if (width > 7) width = 7;
        return new byte[] { 0x1D, 0x21, (byte)((width << 4) + heigth) };
    }

    private byte[] PrintEuro(EPSON e)
    {
        return ByteSplicer.Combine(
        e.CodePage(CodePage.PC858_EURO),
        new byte[] { 0xD5 },
        e.CodePage(CodePage.PC437_USA_STANDARD_EUROPE_DEFAULT));
    }
}