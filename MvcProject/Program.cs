using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;
using MvcProject.Seed;
using MvcProject.Models.Repository;
using MvcProject.Models.DbContext;
using MvcProject.Models.Hash;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Model;
using MvcProject.Models.Service;
using log4net.Config;
using log4net;
using MvcProject.Models.Exceptions;
using MvcProject.Controllers;

var builder = WebApplication.CreateBuilder(args);
XmlConfigurator.Configure(new FileInfo("log4net.config"));
builder.Services.AddRazorPages();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<CasinoDbContext>(options => 
    options.UseSqlServer(connectionString)); 

builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<CasinoDbContext>();
builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(connectionString));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IBankingRequestService, BankingRequestService>();
builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddScoped<IDepositRepository,DepositRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ITransactionRepository,TransactionRepository>();
builder.Services.AddScoped<IWithdrawRepository,WithdrawRepository>();
builder.Services.AddScoped<ICustomExceptions, CustomExceptions>();
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
builder.Services.AddSingleton<ILog>(provider => LogManager.GetLogger(typeof(AdminController)));
builder.Services.AddSingleton<ILog>(provider => LogManager.GetLogger(typeof(CallbackController)));
builder.Services.AddSingleton<ILog>(provider => LogManager.GetLogger(typeof(HomeController)));
builder.Services.AddSingleton<ILog>(provider => LogManager.GetLogger(typeof(TransactionsController)));
builder.Services.AddSingleton<ILog>(provider => LogManager.GetLogger(typeof(WalletController)));


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
