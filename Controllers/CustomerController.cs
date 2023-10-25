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
[Route("api/customer")]
public class CustomerController : ControllerBase{

    private ILogger<CustomerController> _logger;
    private IDataBaseContext _dbContext;

    public CustomerController(ILogger<CustomerController> logger, IDataBaseContext database){
        this._logger = logger;
        this._dbContext = database;
    }

    [HttpPost]
    public ActionResult Post([FromBody] CustomerRequest payload){
        if(string.IsNullOrWhiteSpace(payload.Name) || payload.DocType == null || string.IsNullOrWhiteSpace(payload.Doc)) return BadRequest("Campos inválidos!");
        Customer customer = new Customer(payload.Name, payload.DocType ?? Customer.DocumentType.CNPJ, Customer.SanitizeDoc(payload.Doc));
        return customer.Save(_dbContext) ? Ok(ResultObject.Build("Cliente criado com sucesso!")) : StatusCode(StatusCodes.Status500InternalServerError, ResultObject.BuildErrors("Erro interno"));
    }

    [HttpGet]
    public ActionResult Get([FromQuery] int id){
        if(AuthGetter.Get(Request, _dbContext) == null) return BadRequest(ResultObject.BuildUnauthenticated());
        Customer? record = Customer.GetByID(id, _dbContext);
        if(record == null) return BadRequest(ResultObject.Build("Cliente inexistente!"));
        return Ok(ResultObject.Build(record));
    }

    [HttpGet("all")]
    public ActionResult List([FromQuery] QueryFiltersCustomers filter){
        if(AuthGetter.Get(Request, _dbContext) == null) return BadRequest(ResultObject.BuildUnauthenticated());
        Customer.Result[]? records = Customer.GetRecords(filter.PagingStart ?? 0, filter.PagingEnd ?? 50, filter.Name, filter.Doc, _dbContext);
        if(records == null) return StatusCode(StatusCodes.Status500InternalServerError, ResultObject.Build("Erro interno"));
        return Ok(ResultObject.Build(records));
    }
}