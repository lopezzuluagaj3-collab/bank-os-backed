using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankOs.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddUserDocumentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentId",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Users");
        }
    }
}
