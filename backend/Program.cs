using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetworkInventory.Api.Data;
using NetworkInventory.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- База данных (SQLite + EF Core) ---
// EF Core генерирует параметризованные SQL-запросы → защита от SQL-инъекций
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JwtService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// --- JWT Authentication ---
// Токен читается из HttpOnly cookie (не из Authorization header) — см. OnMessageReceived
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        // Извлечение JWT из HttpOnly cookie вместо заголовка Authorization
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(AuthCookie.Name, out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// --- CORS (Cross-Origin Resource Sharing) ---
// Строгая политика: разрешены запросы ТОЛЬКО с origin фронтенда (Vite dev server).
// Это предотвращает несанкционированные cross-origin запросы с вредоносных сайтов.
// AllowCredentials необходим для передачи HttpOnly cookies между доменами.
builder.Services.AddCors(options =>
{
    options.AddPolicy("StrictFrontendPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Инициализация БД (seed)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("StrictFrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
