using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Project.Lib.Database;
using Project.Models;

namespace Project.Controllers;

public class QueryFilters{
    public int? PagingStart { get; set; }
    public int? PagingEnd   { get; set; }
    public string? Customer { get; set; }
    public string? Device   { get; set; }
    public DateTime? DateStart   { get; set; }
    public DateTime? DateEnd     { get; set; }
}
//Ex: /api/wlc?PagingStart=1&PagingEnd=50&Customer={Documento do cliente}

[ApiController]
[Route("api/wlc")]
public class WaterLevelController : ControllerBase{

    private ILogger<WaterLevelController> _logger;
    private IDataBaseContext _dbContext;

    public WaterLevelController(ILogger<WaterLevelController> logger, IDataBaseContext database){
        this._logger = logger;
        this._dbContext = database;
    }

    [HttpPost]
    public ActionResult Post([FromQuery] string token){
        if(string.IsNullOrWhiteSpace(token)) return BadRequest("Empty token");
        Device? device = Device.GetByToken(token, _dbContext);
        if(device == null) return BadRequest("Invalid token");
        WarningData data = new WarningData(){
            CapturedAt = DateTime.Now,
            DeviceID = device.ID
        };
        if(!data.Save(_dbContext)) return StatusCode(StatusCodes.Status500InternalServerError);
        return Ok(device.Token);
    }

    [HttpGet]
    public ActionResult Get([FromQuery] QueryFilters filter){
        WarningData.WarningDataLevelResult[]? records = WarningData.GetRecords(filter.PagingStart ?? 0, filter.PagingEnd ?? 50, filter.Customer, filter.Device, filter.DateStart, filter.DateEnd, _dbContext);
        if(records == null) return StatusCode(StatusCodes.Status500InternalServerError);
        return Ok(records);
    }
}