using System.Data;
using Core;
using Core.Mapping;
using Core.Services;
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
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<BoxService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddControllers();

builder.Services.AddAutoMapper(typeof(MappingProfile));
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