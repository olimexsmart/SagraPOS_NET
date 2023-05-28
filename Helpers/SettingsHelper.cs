
using SagraPOS.Models;

namespace SagraPOS.Helpers;

public class SettingsHelper
{
    private readonly MenuDB db;
    public SettingsHelper(MenuDB db) => this.db = db;

    public string GetSettingString(string key)
    {
        string? val = db.Settings.Where(x => x.Key == key)
                                .Select(x => x.ValueString)
                                .FirstOrDefault();
        return val ?? throw new NullReferenceException($"Key {key} not found");
    }

    public float GetSettingNumber(string key)
    {
        float? val = db.Settings.Where(x => x.Key == key)
                                .Select(x => x.ValueNum)
                                .FirstOrDefault();
        return val ?? throw new NullReferenceException($"Key {key} not found");
    }

    public byte[] GetSettingBlob(string key)
    {
        byte[]? val = db.Settings.Where(x => x.Key == key)
                                .Select(x => x.ValueBlob)
                                .FirstOrDefault();
        return val ?? throw new NullReferenceException($"Key {key} not found");
    }

    /* Useless but a nice example of a JOIN
    public IEnumerable<string?> GetPrinterConnectionStrings()
    {
        return db.Settings.Join(db.SettingCategories,
                                s => s.Category, sc => sc.ID,
                                (s, sc) => new { Settings = s, SettingCategories = sc })
                          .Where(ssc => ssc.SettingCategories.Name == "PrinterIPs")
                          .Select(ssc => ssc.Settings.ValueString);
    }
    */
}