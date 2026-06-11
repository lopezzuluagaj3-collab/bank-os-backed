using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BankOs.Data;
using BankOs.Interfaces;
using BankOs.Middleware;
using BankOs.Services;
using BankOs.Services.Admin;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankOS API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Escribe: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.AddSecurityDefinition("TenantSlug", new OpenApiSecurityScheme
    {
        Name = "X-Tenant-Slug",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Slug del tenant (solo en desarrollo)"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "TenantSlug"
                }
            },
            Array.Empty<string>()
        }
    });
});

// BD maestra
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Master")));

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
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
            RoleClaimType = "role",
            NameClaimType = "user_id"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminGlobal", policy =>
        policy.RequireClaim(
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            "admin_global"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("role", "administrador"));

    options.AddPolicy("AnyRole", policy =>
        policy.RequireClaim("role", "administrador", "cliente"));
});

// Servicios
builder.Services.AddScoped<ITenantResolver, TenantResolver>();
builder.Services.AddScoped<TenantDbContextFactory>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITenantSettingsService, TenantSettingsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

var app = builder.Build();

// Migrar BDs al arrancar
using (var scope = app.Services.CreateScope())
{
    var master = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
    await master.Database.MigrateAsync();

    var tenants = await master.Tenants.ToListAsync();
    foreach (var tenant in tenants)
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql(tenant.ConnectionString)
            .Options;
        using var tenantDb = new TenantDbContext(options);
        await tenantDb.Database.MigrateAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS debe ir antes de auth y controllers
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseMiddleware<RateLimitMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();