using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Project.Lib.Database;
using DotNetEnv;
using Project.Models;
using Project.Lib;
using Project.Lib.Web;

namespace Project.Controllers;

public class QueryFiltersCustomers{
    public int? PagingStart { get; set; }
    public int? PagingEnd   { get; set; }
    public string? Name { get; set; }
    public string? Doc { get; set; }
}

public class CustomerRequest{
    public string? Name { get; set; }
    public Customer.DocumentType? DocType { get; set; }
    public string? Doc { get; set; }
}

[ApiController]
[Route("api/wlc/customer")]
public class CustomerController : ControllerBase{

    private ILogger<CustomerController> _logger;
    private IDataBaseContext _dbContext;

    public CustomerController(ILogger<CustomerController> logger, IDataBaseContext database){
        this._logger = logger;
        this._dbContext = database;
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] CustomerRequest payload){
        
    }

    [HttpPost("batch")]
    public async Task<ActionResult> Batch([FromBody] CustomerRequest[] payload){
        
    }

    [HttpGet]
    public ActionResult Get([FromQuery] int id){
        
    }

    [HttpGet("all")]
    public ActionResult List([FromQuery] QueryFiltersCustomers filter){
        
    }
}