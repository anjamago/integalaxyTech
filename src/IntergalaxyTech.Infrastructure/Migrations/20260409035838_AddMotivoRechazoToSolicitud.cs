using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntergalaxyTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMotivoRechazoToSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "Solicitudes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                table: "Solicitudes");
        }
    }
}
