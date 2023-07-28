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

    private class OrderEntry
    {
        required public int ID { get; init; }
        required public string Name { get; init; }
        required public int Quantity { get; init; }
        required public float Price { get; init; }
        public float TotalPrice { get => Quantity * Price; }
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            OrderEntry? oe = obj as OrderEntry;
            if (oe is null) return false;
            return ID == oe.ID;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    internal void PrintOrder(int printerID, IEnumerable<OrderEntryDTO> orderToPrint, out float total)
    {
        total = 0;
        // Exit immediately if empty
        if (!orderToPrint.Any())
            throw new InvalidDataException("Empty order");
        // Fill these variable getting result from DB
        Dictionary<int, List<OrderEntry>> printCategories = new();
        bool hasTens = false; // True if some quantity ha 2 digits
        foreach (var o in orderToPrint)
        {
            // Enforce maximum quantity
            o.Quantity = Math.Min(o.Quantity, maxItems);
            // Get MenuEntry object from DB
            MenuEntry? me = db.MenuEntries.Where(x => x.ID == o.EntryID)
                                          .Select(x => new MenuEntry()
                                          {
                                              PrintCategoryID = x.PrintCategoryID,
                                              Name = x.Name,
                                              Price = x.Price
                                          })
                                          .FirstOrDefault();
            if (me is null)
                throw new KeyNotFoundException($"MenuEntry with ID {o.EntryID} not found");
            // Create an instance of the order entry
            OrderEntry orderEntry = new()
            {
                ID = o.EntryID,
                Name = me.Name,
                Quantity = o.Quantity,
                Price = me.Price
            };
            // Add entry to the dictionary
            if (!printCategories.ContainsKey(me.PrintCategoryID))
                printCategories.Add(me.PrintCategoryID, new List<OrderEntry>());
            // Check if already present
            if (printCategories[me.PrintCategoryID].Contains(orderEntry))
                throw new InvalidDataException($"Order contains duplicate IDs with value: {o.EntryID}");
            printCategories[me.PrintCategoryID].Add(orderEntry);
            // Update total
            total += orderEntry.TotalPrice;
            // Update tens presence
            hasTens |= o.Quantity >= 10;
        }
        // Get categories names by ID
        Dictionary<int, string> categoryNames = db.PrintCategories.ToDictionary(k => k.ID, v => v.Name);
        // Stop here if in debug mode
        if (debugMode)
        {
            logger.LogInformation($"Selected printer: {printerID}");
            logger.LogInformation($"Order total: {total}");
            foreach (var pc in printCategories)
                foreach (var pcVal in pc.Value)
                    logger.LogInformation($"Order printing: MenuEntry={pc.Key} Quantity={pcVal.Quantity}");
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
        foreach (var category in printCategories.OrderBy(x => x.Key))
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
                    bab.Append(e.PrintLine($"{entry.Name.ToUpper(),-20}x{entry.Quantity,2}"));
                else
                    bab.Append(e.PrintLine($"{entry.Name.ToUpper(),-21}x{entry.Quantity,1}"));
            }
            bab.Append(e.FullCutAfterFeed(3));
        }
        // Order detail
        foreach (var category in printCategories.OrderBy(x => x.Key))
        {
            // Print Category
            bab.Append(PrinterSetDimensions(0, 0));
            bab.Append(e.SetStyles(PrintStyle.Italic | PrintStyle.Bold));
            bab.Append(e.CenterAlign());
            bab.Append(e.PrintLine("------ " + categoryNames[category.Key].ToUpper() + " ------"));
            bab.Append(e.PrintLine(""));
            // Now all entries
            bab.Append(e.SetStyles(PrintStyle.None));
            bab.Append(PrinterSetDimensions(1, 1));
            bab.Append(e.LeftAlign());
            foreach (var entry in category.Value)
            {
                if (hasTens)
                {
                    bab.Append(e.Print($"{entry.Quantity,-2}x{entry.Name.ToUpper(),-16}"));
                    bab.Append(PrintEuro(e));
                    bab.Append(e.PrintLine($"{entry.TotalPrice:.0}"));
                }
                else
                {
                    bab.Append(e.Print($"{entry.Quantity,-1}x{entry.Name.ToUpper(),-17}"));
                    bab.Append(PrintEuro(e));
                    bab.Append(e.PrintLine($"{entry.TotalPrice:.0}"));
                }
            }
        }
        // Order total
        bab.Append(e.PrintLine(""));
        bab.Append(e.PrintLine(""));
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