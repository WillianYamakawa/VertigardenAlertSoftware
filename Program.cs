using Project.Lib.Database;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDataBaseContext, MySQLContext>();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder => {
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

