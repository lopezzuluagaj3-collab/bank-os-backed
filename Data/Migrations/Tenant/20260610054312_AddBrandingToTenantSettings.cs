using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankOs.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddBrandingToTenantSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "TenantSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CommissionType",
                table: "TenantSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionValue",
                table: "TenantSettings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ExchangeRates",
                table: "TenantSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "TenantSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainCurrency",
                table: "TenantSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "TenantSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryColor",
                table: "TenantSettings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankName",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "CommissionType",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "CommissionValue",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "ExchangeRates",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "MainCurrency",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "SecondaryColor",
                table: "TenantSettings");
        }
    }
}
