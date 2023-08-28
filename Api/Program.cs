using Api.Data.Contexts;
using Api.Data.Interfaces;
using Api.Data.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TransferoDbContext>(options => 
    options.UseSqlite($"Data Source={Directory.GetCurrentDirectory()}\\Storage\\Db\\Transfero.db;"));

builder.Services.AddScoped<IArchiveRepository, ArchiveRepository>();
builder.Services.AddScoped<IFileStorage>(provider => 
    new LocalFileStorage($"{Directory.GetCurrentDirectory()}\\Storage\\Files\""));

var app = builder.Build();

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
