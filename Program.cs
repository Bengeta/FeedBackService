using System.Net;
using System.Reflection;
using GrpcService.ServiceGet;
using HessBackend.Middlewares;
using HessLibrary.Utils;
using Interfaces;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using MongoDB.Driver;
using Repository;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
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
var database = client.GetDatabase("FeedbackDB");

// Регистрируем клиент и базу данных в сервисной коллекции
var services = builder.Services;
services.AddSingleton<IMongoClient>(client);
services.AddSingleton<IMongoDatabase>(database);
services.AddSingleton<IMessageRepository, MessageRepository>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ServicesGrpc.ServiceSent.OrderService>();

builder.Services.AddAutoMapper(typeof(Feedback.Utils.AutoMappingProfiles).Assembly, typeof(HessLibrary.Utils.AutoMappingProfiles).Assembly);

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

configureLogging();
builder.Host.UseSerilog();

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
    options.RoutePrefix = "api/swagger_feedback";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<FeedbackService>();

app.Run();

void configureLogging()
{
    var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    var Configuration = new ConfigurationBuilder()
         .AddJsonFile("data/appsettings.json", optional: false, reloadOnChange: true)
         .Build();

    Serilog.Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .WriteTo.Console()
        .WriteTo.Debug()
        .WriteTo.Elasticsearch(ConfigureElasticSerach(Configuration, enviroment))
        .Enrich.WithProperty("Environment", enviroment)
        .ReadFrom.Configuration(Configuration)
        .CreateLogger();
}
ElasticsearchSinkOptions ConfigureElasticSerach(IConfigurationRoot configurationRoot, string enviroment)
{
    return new ElasticsearchSinkOptions(new Uri(configurationRoot["ElasticConfiguration:Uri"]))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{enviroment.ToLower()}-{DateTime.UtcNow:yyyy-MM-dd}",
        NumberOfReplicas = 1,
        NumberOfShards = 2,
    };
}