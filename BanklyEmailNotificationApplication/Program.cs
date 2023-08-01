using BanklyEmailNotificationApplication.Model;
using BanklyEmailNotificationApplication.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add the EmailBackgroundService as a hosted service
builder.Services.AddHostedService<EmailBackgroundService>();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
