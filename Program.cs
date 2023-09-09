using System.Net;
using GrpcService.ServiceGet;
using HessBackend.Middlewares;
using Interfaces;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using MongoDB.Driver;
using Repository;
using ServicesGrpc.ServiceSent;
using Utils;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 5270,
        cfg => { cfg.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; });


    serverOptions.Listen(IPAddress.Any, 5271,
        cfg => { cfg.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1; });
});


builder.Configuration.AddJsonFile("data/appsettings.json");
var connectionString = builder.Configuration.GetConnectionString("MainDB");
builder.Services.AddGrpc();
var client = new MongoClient(connectionString);
var database = client.GetDatabase("MessageDB");

// Регистрируем клиент и базу данных в сервисной коллекции
var services = builder.Services;
services.AddSingleton<IMongoClient>(client);
services.AddSingleton<IMongoDatabase>(database);
services.AddSingleton<IMessageRepository, MessageRepository>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ServicesGrpc.ServiceSent.OrderService>();

builder.Services.AddAutoMapper(typeof(AutoMappingProfiles).Assembly);

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    o.ReportApiVersions = true;
    o.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new MediaTypeApiVersionReader("ver"));
});
builder.Services.AddVersionedApiExplorer(setup =>
  {
      setup.GroupNameFormat = "'v'VVV";
      setup.SubstituteApiVersionInUrl = true;
  });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();


var app = builder.Build();
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseMiddleware<TokenHandlerMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }
    options.RoutePrefix = "api/swagger_message";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<FeedbackService>();

app.Run();