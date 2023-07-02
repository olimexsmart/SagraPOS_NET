using Microsoft.AspNetCore.Mvc;
using SagraPOS.Helpers;
using SagraPOS.Models;

namespace SagraPOS.Controllers;

[ApiController]
[Route("[action]")]
public class SettingsAPI : ControllerBase
{
    private readonly ILogger<SettingsAPI> logger;
    private readonly IConfiguration configuration;
    private readonly MenuDB db;
    private readonly SettingsHelper sh;

    public SettingsAPI(ILogger<SettingsAPI> logger,
                      IConfiguration configuration,
                      MenuDB db,
                      SettingsHelper sh)
    {
        this.logger = logger;
        this.db = db;
        this.sh = sh;
        this.configuration = configuration;
    }

    [HttpGet]
    public IEnumerable<Printer> GetPrinters() => db.Printers;

    [HttpGet]
    public bool CheckPin([FromQuery] int pin) => db.Settings.Where(x => x.Key == "PIN")
                                                            .Select(x => x.ValueInt)
                                                            .First() == pin;
}