using System.Data;
using Core;
using Infrastructure;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IDbConnection>(container =>
{
    var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("box_conn"));
    connection.Open();
    return connection;
});

builder.Services.AddScoped<BoxRepository>();
builder.Services.AddScoped<BoxService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(options =>
{
    options.SetIsOriginAllowed(origin => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();