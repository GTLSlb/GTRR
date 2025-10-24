using Serilog;

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


builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Host.UseSerilog();
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
