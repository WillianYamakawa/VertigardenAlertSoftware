using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Project.Lib.Database;
using DotNetEnv;
using Project.Models;
using Project.Lib;
using Project.Lib.Web;

namespace Project.Controllers;

public class QueryFiltersWLC{
    public int? PagingStart { get; set; }
    public int? PagingEnd   { get; set; }
    public int? Customer { get; set; }
    public string? Device   { get; set; }
    public DateTime? DateStart   { get; set; }
    public DateTime? DateEnd     { get; set; }
}

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
    public async Task<ActionResult> Post([FromQuery] string token){
        if(string.IsNullOrWhiteSpace(token)) return BadRequest(ResultObject.BuildErrors("Empty token"));
        Device? device = Device.GetByToken(token, _dbContext);
        if(device == null) return BadRequest(ResultObject.BuildErrors("Invalid token"));
        WarningData data = new WarningData(){
            CapturedAt = DateTime.Now,
            DeviceID = device.ID
        };
        await Task.Run(() => {
            string server = Env.GetString("SMTP_SERVER", null);
            int port = Env.GetInt("SMTP_PORT", 0);
            string user = Env.GetString("SMTP_USER", null);
            string password = Env.GetString("SMTP_PASSWORD", null);
            if(string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password) || port == 0) {
                _logger.LogError("SMTP settings are invalid (Could not send Email)");
                return;
            }
            SMTPSender sender = new SMTPSender(server, port, user, password);
            string[]? emails = Models.User.GetAllActiveEmails(_dbContext);
            if(emails == null){
                _logger.LogError("Could not fetch emails from database");
                return;
            }
            foreach(string email in emails){
                try{
                    Customer? customer = device.GetOwner(_dbContext);
                    string notDefined = "{NÃO DEFINIDO}";
                    sender.SendEmail(email, $"ALERTA DE NIVEL - {customer?.Name ?? notDefined}", $"Houve um registro de alerta de nível para {customer?.Name ?? notDefined} ({customer?.Document ?? notDefined} as {data.CapturedAt})", user, false);
                }catch(Exception e){
                    _logger.LogError($"Error in SMTP sending ({email}): {e.Message}");
                }
            }
        });
        if(!data.Save(_dbContext)) return StatusCode(StatusCodes.Status500InternalServerError);
        return Ok();
    }

    [HttpGet]
    public ActionResult Get([FromQuery] QueryFiltersWLC filter){
        Models.User? user = AuthGetter.Get(Request, _dbContext);
        if(user == null) return BadRequest(ResultObject.BuildUnauthenticated());
        WarningData.Result[]? records = WarningData.GetRecords(filter.PagingStart ?? 0, filter.PagingEnd ?? 50, filter.Customer, filter.Device, filter.DateStart, filter.DateEnd, _dbContext);
        if(records == null) return StatusCode(StatusCodes.Status500InternalServerError, ResultObject.BuildErrors("Erro interno"));
        return Ok(ResultObject.Build(records));
    }
}