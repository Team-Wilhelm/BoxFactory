using System.Data;
using System.Diagnostics;
using BoxFactoryAPI.Exceptions;
using Core.Mapping;
using Core.Services;
using Infrastructure;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var Uri = new Uri(Environment.GetEnvironmentVariable("pgconn")!);

var ProperlyFormattedConnectionString = string.Format(
        "Server={0};Database={1};User Id={2};Password={3};Port={4};Pooling=true;MaxPoolSize=5;",
        Uri.Host,
        Uri.AbsolutePath.Trim('/'),
        Uri.UserInfo.Split(':')[0],
        Uri.UserInfo.Split(':')[1],
        Uri.Port > 0 ? Uri.Port : 5432);

builder.Services.AddSingleton<IDbConnection>(container =>
{
    var connection = new NpgsqlConnection(ProperlyFormattedConnectionString);
    connection.Open();
    return connection;
});

builder.Services.AddScoped<BoxRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<BoxService>();
builder.Services.AddControllers(options => options.Filters.Add<ExceptionFilter>());
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

app.UseExceptionHandler(a => a.Run(async context =>
{
    var trace = Activity.Current?.Id ?? context.TraceIdentifier;
    const int statusCode = StatusCodes.Status500InternalServerError;

    context.Response.StatusCode = statusCode;
    await context.Response.WriteAsJsonAsync(
        new ErrorResponse("", statusCode, trace, "Something went wrong"));
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();