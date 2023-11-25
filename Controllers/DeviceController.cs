using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Project.Lib.Database;
using DotNetEnv;
using Project.Models;
using Project.Lib;
using Project.Lib.Web;

namespace Project.Controllers;

public class QueryFiltersDevices{
    public int? PagingStart { get; set; }
    public int? PagingEnd   { get; set; }
    public int? ownerId { get; set; }
}

public class DeviceRequest{
    public string? Token { get; set; }
    public int OwnerId { get; set; }
}

[ApiController]
[Route("api/device")]
public class DeviceController : ControllerBase{

    private ILogger<DeviceController> _logger;
    private IDataBaseContext _dbContext;

    public DeviceController(ILogger<DeviceController> logger, IDataBaseContext database){
        this._logger = logger;
        this._dbContext = database;
    }

    [HttpPost]
    public ActionResult Post([FromBody] DeviceRequest payload){
        if(AuthGetter.Get(Request, _dbContext) == null) return BadRequest(ResultObject.BuildUnauthenticated());
        if(string.IsNullOrWhiteSpace(payload.Token)) return BadRequest(ResultObject.BuildErrors("Campos inválidos!"));
        Device device = new Device() { Token = payload.Token, OwnerID = payload.OwnerId};
        return device.Save(_dbContext) ? Ok(ResultObject.Build("Dispositivo criado com sucesso!")) : StatusCode(StatusCodes.Status500InternalServerError, ResultObject.BuildErrors("Erro interno"));
    }

    [HttpGet]
    public ActionResult Get([FromQuery] int id){
        if(AuthGetter.Get(Request, _dbContext) == null) return BadRequest(ResultObject.BuildUnauthenticated());
        Device? record = Device.GetByID(id, _dbContext);
        if(record == null) return BadRequest(ResultObject.BuildErrors("Dispositivo inexistente!"));
        return Ok(ResultObject.Build(record));
    }

    [HttpGet("all")]
    public ActionResult List([FromQuery] QueryFiltersDevices filter){
        if(AuthGetter.Get(Request, _dbContext) == null) return BadRequest(ResultObject.BuildUnauthenticated());
        Device.DeviceResult[]? records = Device.GetRecords(filter.PagingStart ?? 0, filter.PagingEnd ?? 50, filter.ownerId, _dbContext);
        if(records == null) return StatusCode(StatusCodes.Status500InternalServerError, ResultObject.BuildErrors("Erro interno"));
        return Ok(ResultObject.Build(records));
    }
}