var builder = WebApplication.CreateBuilder(args);

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
