using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Project.Lib.Database;
using Project.Models;

namespace Project.Controllers;

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
    [Authorize]
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
}