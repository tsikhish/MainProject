using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;
using MvcProject.Seed;
using MvcProject.Models.DbContext;
using log4net.Config;
using log4net;
using MvcProject.Controllers;
using MvcProject.Repository;
using MvcProject.Repository.IRepository;
using MvcProject.Service;
using MvcProject.Models;
using MvcProject.Middleware;
using MvcProject.Service.IService;

var builder = WebApplication.CreateBuilder(args);

var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

builder.Services.AddRazorPages();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<CasinoDbContext>(options => 
    options.UseSqlServer(connectionString)); 

builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<CasinoDbContext>();

builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(connectionString));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IBankingRequestService, BankingRequestService>();

builder.Services.AddScoped<IUserRepository,UserRepository>();

builder.Services.AddScoped<IDepositRepository,DepositRepository>();
builder.Services.AddScoped<IDepositService,DepositService>();

builder.Services.AddScoped<IWalletRepository, WalletRepository>();

builder.Services.AddScoped<ITransactionService,TransactionService>();
builder.Services.AddScoped<ITransactionRepository,TransactionRepository>();

builder.Services.AddScoped<IWithdrawRepository,WithdrawRepository>();
builder.Services.AddScoped<IWithdrawService,WithdrawService>();

builder.Services.AddSingleton<ILoggerFactoryService, LoggerFactoryService>();

builder.Services.AddSingleton<ILog>(provider =>
    LogManager.GetLogger("GlobalLogger"));

builder.Services.AddHttpClient();

var app = builder.Build();

await Seed.InitializeAsync(app.Services);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    var logger = LogManager.GetLogger(typeof(Program));
    logger.Info($"Request to {context.Request.Path} received.");
    await next.Invoke();
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
