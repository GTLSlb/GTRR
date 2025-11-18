using GTRR_DataAccessLayer;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sentry;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.UseSentry(o =>
{
    o.Dsn = builder.Configuration["Sentry:Dsn"];
    o.Debug = true;
    o.TracesSampleRate = 1.0;
    o.AttachStacktrace = true;
});


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .WriteTo.Sentry(o =>
    {
        o.Dsn = builder.Configuration["Sentry:Dsn"];
        o.MinimumBreadcrumbLevel = Serilog.Events.LogEventLevel.Information;
        o.MinimumEventLevel = Serilog.Events.LogEventLevel.Error;
        o.AttachStacktrace = true;
    })
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
            "https://gtrr.gtls.com.lb",
            "https://gtrr-api.gtls.com.lb",
            "http://localhost:3000",
            "http://localhost:3001"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddDbContext<GTRR_DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GTRRConnectionString")));

builder.Services.AddScoped<GTRR_HelperDAL>();

var app = builder.Build();


SentrySdk.CaptureMessage("Sentry initialized successfully!");
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.UseAuthorization();
app.MapControllers();
app.Run();
