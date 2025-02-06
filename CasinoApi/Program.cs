using CasinoApi.Controllers;
using CasinoApi.Repositories;
using CasinoApi.Repositories.IRepositories;
using log4net;
using log4net.Config;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IGenerateTokens, GenerateTokens>();
builder.Services.AddScoped<ICreatingGames, CreatingGames>();
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
builder.Services.AddSingleton<ILog>(provider => LogManager.GetLogger(typeof(CreatingGames)));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";  
    options.Cookie.HttpOnly = true;       
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.Use(async (context, next) =>
{
    var logger = LogManager.GetLogger(typeof(Program));
    logger.Info($"Request to {context.Request.Path} received.");
    await next.Invoke();
});
app.MapControllers();

app.Run();
