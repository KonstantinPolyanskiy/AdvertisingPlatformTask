using Core.Application.Contracts;
using Core.Application.Services.AdPlatformService.Contract;
using Core.Application.Services.AdPlatformService.Impl;
using Core.Data.Storages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SupportNonNullableReferenceTypes();
});

builder.Services.AddSingleton<InMemoryAdPlatformStorage>();
builder.Services.AddSingleton<IAdPlatformReader>(sp => sp.GetRequiredService<InMemoryAdPlatformStorage>());
builder.Services.AddSingleton<IAdPlatformWriter>(sp => sp.GetRequiredService<InMemoryAdPlatformStorage>());
builder.Services.AddSingleton<IAdPlatformService, AdPlatformService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();

app.Run();