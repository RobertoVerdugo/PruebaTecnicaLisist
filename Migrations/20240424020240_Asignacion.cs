using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PruebaTecnicaLisit.Migrations
{
    public partial class Asignacion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdAsignación",
                table: "ServiciosUsuario",
                newName: "IdAsignacion");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdAsignacion",
                table: "ServiciosUsuario",
                newName: "IdAsignación");
        }
    }
}
