using Microsoft.EntityFrameworkCore;
using PulseHub.Dal.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PulseHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PulseHubDb")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();