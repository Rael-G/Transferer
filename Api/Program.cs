using Api.Data.Contexts;
using Api.Data.Interfaces;
using Api.Data.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//TODO: extrair a connection string para um metodo que crie o diretorio caso não exista. Deletar o .gitkeep do diretorio
builder.Services.AddDbContext<TransferoDbContext>(options => 
    options.UseSqlite($"Data Source={Directory.GetCurrentDirectory()}\\Storage\\Db\\Transfero.db;"));

builder.Services.AddScoped<IArchiveRepository, ArchiveRepository>();
builder.Services.AddScoped<IFileStorage>(provider => 
    new LocalFileStorage($"{Directory.GetCurrentDirectory()}\\Storage\\Files"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
