using Serilog;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console() // optional, logs to console
    .WriteTo.File(
        path: "Logs/log.txt",
        rollingInterval: RollingInterval.Day, // creates a new file each day
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
           
            "https://gtrr.gtls.store",
            "http://localhost:3000",
            "https://gtrr-api.gtls.store"
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
