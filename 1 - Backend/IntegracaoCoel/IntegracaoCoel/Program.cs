using AppCoel.Core.API.Bootstraps;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiServices();

var app = builder.Build();

// Utilizado para iniciar o banco de dados e criar tabelas
await app.InitializeDatabaseAsync();
await app.CreateOrUpdateSystemAdminUserAsync();

app.UseApiPipeline();

app.Run();