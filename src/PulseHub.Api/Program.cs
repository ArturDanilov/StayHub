using Microsoft.EntityFrameworkCore;
using PulseHub.Business.Managers;
using PulseHub.Business.Repositories;
using PulseHub.Dal.Data;
using PulseHub.Dal.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PulseHubDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PulseHubDb")));

builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyManager, PropertyManager>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
