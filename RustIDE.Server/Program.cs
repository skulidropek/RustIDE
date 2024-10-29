using Microsoft.CodeAnalysis.Completion;
using RustIDE.Server.Services;
using Microsoft.EntityFrameworkCore;
using RustIDE.Server.Data;
using CompletionService = RustIDE.Server.Services.CompletionService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddScoped<CodeExecutorService>();
builder.Services.AddScoped<CompletionService>();
builder.Services.AddSingleton<AIInteractionService>();
builder.Services.AddSingleton<HooksService>();

// Добавьте эту строку для подключения к PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление Health Checks
builder.Services.AddHealthChecks();

// Установка порта из переменной окружения или использование порта по умолчанию (8080)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

// Middleware for serving static files (e.g., React SPA)
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply CORS policy globally
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Настройка пути для Health Checks
app.UseHealthChecks("/health");

// Fallback to serve SPA (React, etc.)
app.MapFallbackToFile("/index.html");

app.Run();
