using GTRR_DataAccessLayer;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: "Logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        outputTemplate:
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] ({ProcessId}/{ThreadId}) {SourceContext} | {Message:lj}{NewLine}{Exception}")
    .CreateLogger();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(

            "https://gtrr.gtls.store",
            "https://gtrr-api.gtls.store",
            "http://localhost:3000",
            "http://localhost:3001"
          
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<GTRR_DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GTRRConnectionString")));
builder.Services.AddScoped<GTRR_HelperDAL>();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
    });



var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins");

app.UseAuthorization();

app.MapControllers();


app.Run();
