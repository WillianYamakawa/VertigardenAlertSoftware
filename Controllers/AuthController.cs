using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Project.Lib.Database;
using Project.Models;
using Project.Lib.Security;


namespace Project.Controllers;

public class AuthRequest{
    public string? Login { get; set; }
    public string? Password { get; set; }
    public bool isAdmin { get; set; }
}

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(8);
    private static readonly byte[] JwtKey = Encoding.UTF8.GetBytes(Env.GetString("JWT_SECRET"));
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
        User user = new User(body.Login, Hasher.CalculateSHA256Hash(body.Password), isAdmin: body.isAdmin);
        return user.Save(_context) ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
    }

    [HttpPost("login")]
    public ActionResult PostLogin([FromBody] AuthRequest body){
        string? login = body.Login;
        string? password = body.Password;
        if(string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password)) return BadRequest("Login and Password must not be empty");
        User? user = Models.User.Validate(login, Hasher.CalculateSHA256Hash(password), _context);
        if(user == null) return BadRequest("Login not found");
        string token = Guid.NewGuid().ToString();
        user.Token = token;
        if(!user.Save(_context)) return StatusCode(StatusCodes.Status500InternalServerError);
        return Ok(token);
    }
}