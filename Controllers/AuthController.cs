using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Project.Lib.Database;
using Project.Models;
using Project.Lib.Security;
using Project.Lib.Web;


namespace Project.Controllers;

public class AuthRequest{
    public string? Login { get; set; }
    public string? Password { get; set; }
    public bool isAdmin { get; set; }
    public string? Email { get; set; }
    public bool Notify { get; set; }
}

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase{
    private ILogger _logger;
    private IDataBaseContext _context;
    public AuthController(ILogger<AuthController> logger, IDataBaseContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpPost("register")]
    public ActionResult PostRegister([FromBody] AuthRequest body)
    {
        if(string.IsNullOrWhiteSpace(body.Login) || string.IsNullOrWhiteSpace(body.Password)) return BadRequest("Login and Password must not be empty");
        User user = new User(body.Login, Hasher.CalculateSHA256Hash(body.Password), isAdmin: body.isAdmin, email: body.Email, notify: body.Notify);
        return user.Save(_context) ? Ok(ResultObject.Build("Usuário criado com sucesso!")) : StatusCode(StatusCodes.Status500InternalServerError, ResultObject.BuildErrors("Erro interno"));
    }

    [HttpPost("login")]
    public ActionResult PostLogin([FromBody] AuthRequest body){
        string? login = body.Login;
        string? password = body.Password;
        if(string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password)) return BadRequest(ResultObject.BuildErrors("Login and Password must not be empty"));
        User? user = Models.User.Validate(login, Hasher.CalculateSHA256Hash(password), _context);
        if(user == null) return BadRequest(ResultObject.BuildErrors("Login not found"));
        string token = Guid.NewGuid().ToString();
        user.Token = token;
        if(!user.Save(_context)) return StatusCode(StatusCodes.Status500InternalServerError, ResultObject.BuildErrors("Erro interno"));
        return Ok(ResultObject.Build(token));
    }
}