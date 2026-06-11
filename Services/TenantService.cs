using BankOs.Data;
using BankOs.DTOs.Tenant;
using BankOs.Interfaces;
using BankOs.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BankOs.Services;

public class TenantService : ITenantService
{
    private readonly MasterDbContext _master;
    private readonly IConfiguration _config;

    public TenantService(MasterDbContext master, IConfiguration config)
    {
        _master = master;
        _config = config;
    }

    public async Task<TenantRegistry> CreateAsync(CreateTenantDto dto)
    {
        var exists = await _master.Tenants.AnyAsync(t => t.Slug == dto.Slug);
        if (exists)
            throw new InvalidOperationException($"Ya existe un tenant con el slug '{dto.Slug}'");

        var connectionString = BuildConnectionString(dto.Slug);

        // 1. Crear BD del tenant y correr migraciones
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        using var tenantDb = new TenantDbContext(options);
        await tenantDb.Database.MigrateAsync();

        // 2. Seed inicial de TenantSettings con los valores del DTO
        //    Sin esto la tabla queda vacía y los servicios no tienen configuración
        var settings = new TenantSettings
        {
            Id = Guid.NewGuid(),
            TransferFee = dto.Config.TransferFee,
            ExchangeFeePercent = dto.Config.ExchangeFeePercent,
            MinTransactionAmount = dto.Config.MinTransactionAmount,
            MaxTransactionAmount = dto.Config.MaxTransactionAmount,
            DailyLimit = dto.Config.DailyLimit,
            MainCurrency = dto.Config.MainCurrency,
            CommissionType = dto.Config.CommissionType,
            CommissionValue = dto.Config.CommissionValue,
            ExchangeRates = JsonSerializer.Serialize(dto.Config.ExchangeRates),
            BankName = dto.Name, // usa el nombre del tenant como valor inicial
            PrimaryColor = "#1A73E8",
            SecondaryColor = "#FBBC04",
            LogoUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        tenantDb.TenantSettings.Add(settings);
        await tenantDb.SaveChangesAsync();

        // 3. Registrar el tenant en la BD maestra
        var tenant = new TenantRegistry
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Slug = dto.Slug,
            WebhookUrl = dto.WebhookUrl,
            ConnectionString = connectionString,
            Config = JsonSerializer.Serialize(new TenantConfig
            {
                MainCurrency = dto.Config.MainCurrency,
                MaxTransactionAmount = dto.Config.MaxTransactionAmount,
                CommissionType = dto.Config.CommissionType,
                CommissionValue = dto.Config.CommissionValue,
                ExchangeRates = dto.Config.ExchangeRates
            }),
            CreatedAt = DateTime.UtcNow
        };

        _master.Tenants.Add(tenant);
        await _master.SaveChangesAsync();

        return tenant;
    }

    public async Task<TenantRegistry?> GetBySlugAsync(string slug)
    {
        return await _master.Tenants.FirstOrDefaultAsync(t => t.Slug == slug);
    }

    public async Task<List<TenantRegistry>> GetAllAsync()
    {
        return await _master.Tenants.ToListAsync();
    }

    private string BuildConnectionString(string slug)
    {
        var master = _config.GetConnectionString("Master")!;
        var dbName = $"bankos_{slug}";
        return Regex.Replace(master, @"Database=\w+", $"Database={dbName}");
    }
}