using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using StreamApi.Services;
using Serilog;


var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((hostContext, services, configuration) => {
    configuration.WriteTo.Console();
    configuration.WriteTo.File("C:\\Logs\\StreamApi\\log.txt");
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", options => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicAuthentication", new AuthorizationPolicyBuilder("BasicAuthentication").RequireAuthenticatedUser().Build());
});

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

var app = builder.Build();
//app.MapGet("/", () => "Hello World!");//for Serilog

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
